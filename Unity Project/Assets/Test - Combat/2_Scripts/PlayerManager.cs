using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : Singleton<PlayerManager> 
{
	#region Properties
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

	private bool isHit = false;
	private bool isAttacked = false;

	private Enemy target;
	private Animator anim;
	private NavMeshAgent agent;

	private enum State
	{
		Idle,
		Attack,
		GetClose,
		Dodge,
		Hit
	}

	private State currentState;
	private bool isBufferOpen = false;
	private bool isChainAttack = false;

	private AnimAction melee;
//	private AnimAction mid;
	private AnimAction range;
//	private AnimAction dash;

	private float distToReach = 0;
	#endregion

	#region Unity
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine (transform.position, transform.forward * coneLength);

		Gizmos.color = Color.blue;
		Vector3 pos;
		pos = Quaternion.AngleAxis (coneAngle, Vector3.up) * transform.forward;
		Gizmos.DrawLine (transform.position, pos * coneLength);
		pos = Quaternion.AngleAxis (-coneAngle, Vector3.up) * transform.forward;
		Gizmos.DrawLine (transform.position, pos * coneLength);
	}

	void Awake()
	{
		anim = transform.GetComponentInChildren<Animator> ();
		agent = GetComponent<NavMeshAgent> ();

		SetActions ();
	}

	void Start()
	{
		currentState = State.Idle;
	}

	void Update()
	{
		StateMachine ();

		if (Time.time >= comboResetTimer) 
		{
			ResetCombo ();
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

	private void StateMachine()
	{
		switch (currentState)
		{
		case State.Attack:
			LookAtTarget ();
			anim.SetFloat ("Distance", Vector3.Distance (transform.position, target.transform.position));
			break;
		case State.GetClose:
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

	private void ResetCombo()
	{
		comboMeter = 0;
		isHit = false;
		comboResetTimer = Mathf.Infinity;
  	}

	private void CloseDistance()
	{
		comboResetTimer = Mathf.Infinity;
		float dist = Vector3.Distance (transform.position, target.transform.position);

//		print (dist + "  " + distToReach + "  " + target);

		if (dist > distToReach) 
		{
//			transform.position += transform.forward.normalized * (Time.deltaTime);

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
			{
				distToReach = melee.maxRange;
				currentState = State.GetClose;
				anim.SetTrigger ("Dash");
			} 
			else
			{
				BasicAttack ();
			}
		}
	}

	private void BasicAttack()
	{
		if (Enemy.ReferenceEquals (target, null)) 
		{
			target = EnemyManager.Instance.GetClosestEnemy ();
		}

		if (Vector3.Distance (transform.position, target.transform.position) > range.maxRange) 
		{
			distToReach = range.maxRange;
			if (currentState == State.GetClose)
				print ("here");
			currentState = State.GetClose;
			anim.SetTrigger ("Dash");
		} 
		else 
		{
			currentState = State.Attack;
			anim.SetFloat ("Distance", Vector3.Distance (transform.position, target.transform.position));
			anim.SetTrigger ("Attack");
		}
	}

	private void CheckHealth()
	{
		if (health <= 0) 
		{
//			GameOver
		}
	}
	#endregion

	#region Public
	public void LeftTapDetected()
	{
		if (currentState != State.Dodge) 
		{
			if (currentState == State.GetClose) 
			{
				agent.velocity = Vector3.zero;
				agent.Stop ();
			}

			currentState = State.Dodge;
			anim.SetTrigger ("Dodge");
			isAttacked = false;
		}
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
		if (currentState != State.Dodge) 
		{
			health -= damage;
			currentState = State.Hit;
		} 
		else 
		{
			isAttacked = true;
			comboMeter++;
			comboResetTimer = Time.time + resetDuration;
		}
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
		currentState = State.Idle;
		comboResetTimer = Time.time + resetDuration;
	}

	public void KnockDownStrikePoint()
	{
		target.KnockedDown (attackDamages);
	}

	public void EndAnim()
	{
		// Wrong Dodge
		if (!isAttacked) 
		{
			ResetCombo ();
		}
		currentState = State.Idle;
	}

	public void LaunchArrow()
	{
		//		ArrowManager.Instance.Launch (target.transform);
	}
	#endregion
}