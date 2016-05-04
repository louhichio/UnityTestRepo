using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : Singleton<PlayerManager> 
{
	#region Properties
	private enum State
	{
		None,
		Idle,
		Attack,
		Move,
		Dodge,
		Hit,
		Dead
	}

	[Header("Playable character Movements")]
	[SerializeField]
	private int health = 150;
	[SerializeField]
	private int attackDamages = 15;

	[Header("Combo Meter")]
	[SerializeField]
	private float resetDuration = 1.0f;
	[SerializeField]
	private int comboMeterBonus = 1;

	[Header("Swipe Targeting")]
	[SerializeField]
	private float coneLength = 100;
	[SerializeField]
	private float coneAngle = 45;

	private int comboMeter = 0;
	private float comboResetTimer = Mathf.Infinity;

	private bool isAttacked = false;

	private Enemy target;
	private Animator anim;
	private NavMeshAgent agent;
	private State currentState;

	private bool isBufferOpen = false;
	private bool isChainAttack = false;

	private bool isIdle = true;
	private bool isMove = false;
	private bool isAttack = false;
	private bool isDodge = false;
	private bool isDead = false;
	private bool isHit = false;

	private float distToReach = 0;

	private AnimAction melee;
	private AnimAction range;
	//	private AnimAction dash;
	//	private AnimAction mid;
	#endregion

	#region Unity
	void OnDrawGizmosSetected()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine (transform.position, transform.forward * coneLength);

		Gizmos.color = Color.blue;
		Vector3 pos;
		pos = Quaternion.AngleAxis (coneAngle, Vector3.up) * transform.forward;
		Gizmos.DrawLine (transform.position, pos * coneLength);
		pos = Quaternion.AngleAxis (-coneAngle, Vector3.up) * transform.forward;
		Gizmos.DrawLine (transform.position, pos * coneLength);

//		Gizmos.color = Color.red;
//		Gizmos.DrawSphere (transform.position, 2.12f);
//		Gizmos.color = Color.yellow;
//		Gizmos.DrawSphere (transform.position, 6f);
	}

	void Awake()
	{
		anim = transform.GetComponentInChildren<Animator> ();
		agent = GetComponent<NavMeshAgent> ();

		SetActions ();
	}

	void Start()
	{
		SwitchState (State.Idle);
	}

	void Update()
	{
		CheckState ();
		UpdateState ();

		if (Time.time >= comboResetTimer) 
		{
			ResetCombo ();
		}
	}
	#endregion

	#region StateMachine
	private void CheckState()
	{		
		if(isDead)
		{
			isDead = false;
			SwitchState (State.Dead);
		}
		else if(isDodge) 
		{
			isDodge = false;
			SwitchState (State.Dodge);
		}
		else if (isHit) 
		{
			isHit = false;
			SwitchState (State.Hit);
		}
		else if (isMove) 
		{
			isMove = false;
			SwitchState (State.Move);
		} 
		else if (isAttack) 
		{
			isAttack = false;
			SwitchState (State.Attack);
		}
		else if(isIdle)
		{
			isIdle = false;
			SwitchState (State.Idle);
		}
	}

	private void UpdateState()
	{
		switch (currentState)
		{
		case State.Attack:
			if (!Enemy.ReferenceEquals (target, null)) 
			{
				LookAtTarget ();
				anim.SetFloat ("Distance", Vector3.Distance (transform.position, target.transform.position));
			}
			break;
		case State.Move:
			//			LookAtTarget ();
			anim.SetFloat ("Distance", Vector3.Distance (transform.position, target.transform.position));
			CloseDistance ();
			break;
		case State.Hit:
			ResetCombo ();
			break;
		case State.Dodge:
			break;
		}
	}

	private void SwitchState(State newState)
	{
		if ((currentState != newState || newState == State.Hit) && currentState != State.Dead) 
		{
			// end old state
			switch (currentState)
			{
			case State.Idle:
				break;
			case State.Attack:
				break;
			case State.Move:	
				agent.velocity = Vector3.zero;
				agent.Stop ();
				break;
			case State.Dodge:
				break;
			case State.Hit:
				break;
			case State.Dead:
				break;
			}

			currentState = newState;

			// start newState
			switch (currentState)
			{
			case State.Idle:
				break;
			case State.Attack:
				anim.SetFloat ("Distance", Vector3.Distance (transform.position, target.transform.position));
				anim.SetTrigger ("Attack");
				break;
			case State.Move:
				anim.SetTrigger ("Dash");
				distToReach = melee.maxRange;
				break;
			case State.Dodge:
				anim.SetTrigger ("Dodge");
				isAttacked = false;
				break;
			case State.Hit:
				anim.SetTrigger ("Hit");
				CheckHealth ();
				break;
			case State.Dead:
				anim.SetTrigger ("Dead");
				break;
			}
		}
	}
	#endregion

	#region Private
	private void SetActions()
	{
		List<string> actionAnimList = new List<string> ();

		for(int i = 0; i < 4; i++)
			actionAnimList.Add ("Atk_Melee_0"+ i + "a");
		melee = new AnimAction (actionAnimList, 2.12f);

		//		actionAnimList = new List<string> ();
		//		for(int i = 0; i < 4; i++)
		//			actionAnimList.Add ("Atk_Mid_0"+ i + "a");
		//		mid = new AnimAction (actionAnimList, 3.51f);

		actionAnimList = new List<string> ();
		for(int i = 0; i < 4; i++)
			actionAnimList.Add ("Atk_Range_0"+ i + "a");
		range = new AnimAction (actionAnimList, 6f);

		//		actionAnimList = new List<string> ();
		//		actionAnimList.Add ("Dash_Start");
		//		actionAnimList.Add ("Dash_End");
		//		dash = new AnimAction (actionAnimList);
	}

	private void ResetCombo()
	{
		comboMeter = 0;
		comboResetTimer = Mathf.Infinity;
  	}

	private void CloseDistance()
	{
		comboResetTimer = Mathf.Infinity;
		float dist = Vector3.Distance (transform.position, target.transform.position);

		if (dist > distToReach) 
		{
			agent.destination = target.transform.position;
			agent.velocity = agent.desiredVelocity * agent.speed;
			agent.Resume ();
		}
		else 
		{
			agent.velocity = Vector3.zero;
			agent.Stop ();
			BasicAttack ();
		}
	}

	private void LookAtTarget()
	{
		Vector3 pos;
		pos = target.transform.position;
		pos.y = transform.position.y;
		transform.LookAt (pos);
	}

	private void AttackNewTarget()
	{
		target = EnemyManager.Instance.GetNewTarget (target, coneLength, coneAngle);

		if (!Enemy.ReferenceEquals (target, null)) 
		{
			if (Vector3.Distance (transform.position, target.transform.position) > melee.maxRange) 
				isMove = true;
			else
				BasicAttack ();
		}
	}

	private void BasicAttack()
	{
		if (Enemy.ReferenceEquals (target, null) || !target.CanBeAttacked()) 
		{
			target = EnemyManager.Instance.GetClosestEnemy ();
		}

		if (!Enemy.ReferenceEquals (target, null)) 
		{
			if (Vector3.Distance (transform.position, target.transform.position) > range.maxRange) 
				isMove = true;
			else 
				isAttack = true;
		}
	}

	private void CheckHealth()
	{
		if (health <= 0) 
			isDead = true;
	}
	#endregion

	#region Public
	public void LeftTapDetected()
	{
		isDodge = true;
	}

	public void RightTapDetected()
	{
		if (currentState == State.Idle)
			BasicAttack ();
		else if (isBufferOpen)
			isChainAttack = true;
	}

	public void SwipeDetected()
	{
		agent.velocity = Vector3.zero;
		agent.Stop ();
		AttackNewTarget ();
	}

	public int GetComboMeter()
	{
		return comboMeter;
	}

	public void Hit(int damage)
	{
		if (currentState != State.Dodge &&
			(Enemy.ReferenceEquals(target, null) || !target.CanParry())) 
		{
			isHit = true;
			health -= damage;
		} 
		else 
		{
			isAttacked = true;
			comboMeter++;
			comboResetTimer = Time.time + resetDuration;
		}
	}

	public float GetConeAngle()
	{
		return coneAngle;
	}

	public float GetConeLength()
	{
		return coneLength;
	}

	public bool IsDead()
	{
		return currentState == State.Dead;
	}
	#endregion

	#region AnimatorEvents
	public void OpenBuffer()
	{
		isBufferOpen = true;
		isChainAttack = false;
	}

	public void StrikePoint()
	{
		target.Hit (attackDamages);
		comboMeter += comboMeterBonus;
		comboResetTimer = Time.time + resetDuration;
	}

	public void CloseBuffer()
	{
		isBufferOpen = false;

		if (isChainAttack)
			anim.SetTrigger ("ChainAttack");
	}

	public void EndAttack()
	{
		isIdle = true;
		comboResetTimer = Time.time + resetDuration;
	}

	public void KnockDownStrikePoint()
	{
		target.KnockedDown (attackDamages);
		target = null;
	}

	public void EndAnim()
	{
		// Wrong Dodge
		if (currentState == State.Dodge && !isAttacked) 
			ResetCombo ();
		
		isIdle = true;
	}

	public void LaunchArrow()
	{
		//		ArrowManager.Instance.Launch (target.transform);
	}
	#endregion
}