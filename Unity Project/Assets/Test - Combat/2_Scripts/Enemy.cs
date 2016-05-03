using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour 
{
	#region Properties
	private enum State
	{
		Idle,
		isHit,
		KnockedDown,
		Dead
	}

	private enum Type
	{
		Melee,
		Ranged
	}

	private enum Threat
	{
		Vulnerable = 0,
		High = 1
	}

	private Type type;
	private Threat threat;
	private State currentState;
	private Animator anim;
	private NavMeshAgent agent;

	private Transform plTransform;
	private PlayerManager plManager;

	private int health = 100;
	private bool canParry = false;
	#endregion

	#region Unity
	void Awake()
	{
		anim = GetComponent<Animator> ();
		agent = GetComponent<NavMeshAgent> ();
	}

	void Start () 
	{
		plTransform = PlayerManager.Instance.transform;
		plManager = PlayerManager.Instance;

		currentState = State.Idle;
		threat = Threat.Vulnerable;
	}

	void Update () 
	{
		switch (currentState) 
		{
		case State.Idle:
			LookAtPlayer ();
			break;
		case State.isHit:
			LookAtPlayer ();
			break;
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
	#endregion

	#region Public
	public void Hit(int damage)
	{
		health -= damage;

		currentState = State.isHit;
		anim.SetTrigger ("Hit");
		anim.SetFloat ("Distance", Vector3.Distance (plTransform.position, transform.position));
	}

	public void KnockedDown(int damage)
	{
		health -= damage;

		currentState = State.KnockedDown;
		anim.SetTrigger ("KnockedDown");
	}

	public bool CanCarry()
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
		plManager.Hit (10);
	}

	public void KnockDownOver()
	{
		currentState = State.Idle;
	}

	public void EndAttack()
	{
		currentState = State.Idle;
	}
	#endregion
}
