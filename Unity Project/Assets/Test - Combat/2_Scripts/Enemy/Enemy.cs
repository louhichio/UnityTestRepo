using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
	public Enemy()
	{
	}
	#region Properties
	public enum State
	{
		None,
		Idle,
		Moving,
		Attack,
		isHit,
		KnockedDown,
		Dead
	}

	public enum Type
	{
		Weak,
		Strong
	}

	public enum Threat
	{
		Vulnerable = 0,
		High = 1,
		Veryhigh = 2
	}

	[Header("Enemy Parameters")]
	public int health = 150;
	public int attackDamages = 15;	
//	[SerializeField]
//	private float meleeRangeMin = 0;
//	[SerializeField]
//	private float meleeRangeMax = 2.12f;
	[SerializeField]
	private float meleeAttackDistanceMax = 2.12f;
	[SerializeField]
	private float knockDownDuration = 5;

	[Header("Enemy Color Change")]
	[SerializeField]
	private SkinnedMeshRenderer mr;
	[SerializeField]
	private Material materialEnemy;
	[SerializeField]
	private Material materialAttack;

	[HideInInspector]
	public Type type;
	private Threat threat;
	private State currentState;
	private Animator anim;
	private NavMeshAgent agent;

	private Transform plTransform;
	private PlayerManager plManager;

	private bool canParry = false;

	private float knockDownTimer = Mathf.Infinity;

	private bool isHit = false;
	private bool isKnockedDown = false;
	private bool isKnockDownOver = true;
	private bool isMove = false;
	private bool isDead = false;
	private bool isAttacking = false;
	private bool waitForEndoFNextHit = false;
	private bool isEndAttack = false;

	private Transform placementPoint;
	private Transform setNewPlacement;
	#endregion

	#region Unity
	public void AwakeEnemy()
	{
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
	}

	public void StartEnemy () 
	{
		plTransform = PlayerManager.Instance.transform;
		plManager = PlayerManager.Instance;

		SwitchState(State.Moving);
		threat = Threat.Vulnerable;

		Init ();
	}

	public void UpdateEnemy () 
	{
		if (placementPoint != setNewPlacement)
			placementPoint = setNewPlacement;
		if (currentState != State.Dead) 
		{
			CheckState ();
			UpdateState ();
		}
	}
	#endregion

	#region Abstract
	public abstract void Init ();
	#endregion

	#region StateMachine
	private void CheckState()
	{
		if (isDead) 
		{
			isDead = false;
			SwitchState(State.Dead);
		}
		else if (isHit) 
		{
			isHit = false;
			SwitchState(State.isHit);
		}
		else if(isKnockedDown)
		{
			isKnockedDown = false;
			SwitchState(State.KnockedDown);
		}
		else if(isAttacking)
		{
			if (currentState == State.KnockedDown && !isKnockDownOver) {} 
			else 
			{
				isAttacking = false;
				SwitchState (State.Attack);
			}
		}
		else if (isMove) 
		{
			if (currentState == State.KnockedDown && !isKnockDownOver) {} 
			else 
			{
				isMove = false;
				SwitchState (State.Moving);
			}
		}
	}

	private void UpdateState()
	{
		switch (currentState) 
		{
		case State.Idle:
			float dist;
			if (placementPoint) 
			{
				dist = Vector3.Distance (transform.position, placementPoint.position);
				if (dist > 0.2f) 
					isMove = true;	
			}
			else 
			{
				dist = Vector3.Distance (transform.position, plTransform.position);

				if (dist > meleeAttackDistanceMax) 
					isMove = true;				
			}
			LookAtPlayer ();
			break;

		case State.Attack:
			if (!anim.GetCurrentAnimatorStateInfo (0).IsName("ATK_Melee_01a"))
				anim.SetTrigger ("Attack");
			LookAtPlayer ();
			break;

		case State.Moving:
			LookAtPlayer ();
			CloseDistance ();
			break;

		case State.isHit:
			LookAtPlayer ();
			break;

		case State.KnockedDown:
			if (Time.time >= knockDownTimer) {
				anim.SetTrigger ("EndKnockedDown");
				knockDownTimer = Mathf.Infinity;
			}
			break;
		}
	}

	private void SwitchState(State newState)
	{
//		print (currentState + "  " + newState);
		if ((currentState != newState || newState == State.isHit) && currentState != State.Dead) 
		{
			//End CurrentState
			switch (currentState) 
			{
			case State.Moving:
				agent.destination = transform.position;
				agent.velocity = Vector3.zero;
				agent.Stop ();
				anim.SetFloat ("xAxis", agent.desiredVelocity.x);
				anim.SetFloat ("zAxis", agent.desiredVelocity.z);
				break;

			case State.isHit:
				if (newState == State.isHit)
					waitForEndoFNextHit = true;
				agent.destination = transform.position;
				agent.velocity = Vector3.zero;
				agent.Stop ();
				anim.SetFloat ("xAxis", agent.desiredVelocity.x);
				anim.SetFloat ("zAxis", agent.desiredVelocity.z);
				break;

			case State.Attack:
				threat = Threat.Vulnerable;
				mr.material = materialEnemy;

				if (!isEndAttack)
					EnemyManager.Instance.EnemyFinishedAttack ();
				else
					isEndAttack = false;
				break;

			case State.KnockedDown:
				isKnockDownOver = true;
				break;
			}

			currentState = newState;

			//Start NewState
			switch(newState)
			{
			case State.Idle:
				anim.SetTrigger ("Idle");
				break;
			case State.Attack:
				mr.material = materialAttack;
				anim.SetTrigger ("Attack");
				threat = type == Type.Weak ? Threat.High: Threat.Veryhigh;
				break;
			case State.Moving:
				anim.SetTrigger ("Walk");
				agent.Resume ();
				break;
			case State.isHit:
				isAttacking = false;
				anim.SetTrigger ("Hit");
				anim.SetFloat ("Distance", Vector3.Distance (plTransform.position, transform.position));
				CheckHealth ();
				break;
			case State.KnockedDown:
				isKnockDownOver = false;
				knockDownTimer = Time.time + knockDownDuration;
				anim.SetTrigger ("KnockedDown");
				CheckHealth ();
				break;
			case State.Dead:
				anim.SetTrigger ("Dead");
				EnemyManager.Instance.CheckAnyEnemyLeft ();
				EnemyManager.Instance.FreePlacementPoint (placementPoint, this);
				placementPoint = null;
				break;
			}
		}
	}
	#endregion

	#region Private
	private void LookAtPlayer()
	{
		Vector3 pos = plTransform.position;
		pos.y = transform.position.y;
		Quaternion wantedrotation = Quaternion.LookRotation(plTransform.position - transform.position);

		transform.rotation = Quaternion.Slerp (transform.rotation, wantedrotation, Time.deltaTime * 5);
	}

	private void CloseDistance()
	{
		float dist;
		if (placementPoint) 
		{
			dist = Vector3.Distance (transform.position, placementPoint.position);

			if (dist > 0.2f) 
			{
				agent.destination = placementPoint.position;
				agent.velocity = agent.desiredVelocity * agent.speed;
				Vector3 dir = transform.InverseTransformDirection (agent.desiredVelocity);
				anim.SetFloat ("xAxis", dir.x);
				anim.SetFloat ("zAxis", dir.z);
			} 
			else
				SwitchState (State.Idle);
		} 
		else 
		{
			dist = Vector3.Distance (transform.position, plTransform.position);

			if (dist > meleeAttackDistanceMax) {
				agent.destination = plTransform.position;
				agent.velocity = agent.desiredVelocity * agent.speed;
				Vector3 dir = transform.InverseTransformDirection (agent.desiredVelocity);
				anim.SetFloat ("xAxis", dir.x);
				anim.SetFloat ("zAxis", dir.z);
			} else
				SwitchState (State.Idle);
		}
	}

	private void CheckHealth()
	{
		if (health <= 0)
			isDead = true;
	}

	private IEnumerator Death()
	{
		yield return new WaitForSeconds (5);
		gameObject.SetActive (false);
	}
	#endregion

	#region Public
	public void Hit(int damage)
	{
		health -= damage;
		isHit = true;
	}

	public void KnockedDown(int damage)
	{
		health -= damage;
		isKnockedDown = true;
	}

	public bool CanParry()
	{
		return canParry;
	}

	public float GetDistance()
	{
		return Vector3.Distance (plTransform.position, transform.position);
	}

	public float GetAngle()
	{
		return Vector3.Angle(transform.position - plTransform.position, plTransform.forward);
	}

	public int GetThreat()
	{
		return (int)threat;
	}

	public bool isAlive()
	{
		return currentState != State.Dead && health > 0;
	}

	public bool CanBeAttacked()
	{
		return isAlive () && currentState != State.KnockedDown;
	}

	public bool CanAttack()
	{
		float dist;
		if (placementPoint) 
		{
			dist = Vector3.Distance (transform.position, placementPoint.position);
			if (dist > 0.2f)
				return false;	
		}
		else 
		{
			dist = Vector3.Distance (transform.position, plTransform.position);
			if (dist > meleeAttackDistanceMax)
				return false;	
		}
		return currentState == State.Idle && !isKnockedDown;
	}

	public void AllowedToAttack()
	{
		isAttacking = true;
	}

	public bool IsInCameraFOV()
	{
		Vector3 pos = Camera.main.WorldToViewportPoint (transform.position);

		if (pos.x < 0 || pos.x > 1 || pos.y < 0 || pos.y > 1)
			return false;
		
		return true;
	}

	public void SetPlacementPoint(Transform point)
	{
		setNewPlacement = point;
		placementPoint = null;
		placementPoint = point;
	}

	public bool Linked()
	{
		return placementPoint != null;
	}

	public Transform GetPlacementPoint()
	{
		return placementPoint;
	}
	#endregion

	#region AnimatorEvents
	public void OpenParry()
	{
		canParry = true;
	}

	public void CloseParry()
	{
		canParry = false;
	}

	public void StrikePoint()
	{
		plManager.Hit (attackDamages);
	}

	public void KnockDownOver()
	{
		isKnockDownOver = true;

		isMove = true;
	}

	public void EndAttack()
	{
		isEndAttack = true;
		EnemyManager.Instance.EnemyFinishedAttack ();

		isMove = true;
	}

	public void EndAnim()
	{
		if (!waitForEndoFNextHit) 
			isMove = true;
		else
			waitForEndoFNextHit = false;
	}

	public void EndDeadAnim()
	{
		StartCoroutine ("Death");
	}


	#endregion
}
