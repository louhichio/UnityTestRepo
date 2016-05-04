using UnityEngine;
using System.Collections;

public class StrongEnemy : Enemy 
{
	#region Unity
	void Awake()
	{
		AwakeEnemy ();
	}

	void Start()
	{
		StartEnemy ();
	}

	void Update()
	{
		UpdateEnemy ();
	}
	#endregion

	#region Override
	public override void Init ()
	{
		type = Type.Strong;
		health = 90;
		attackDamages = 30;
	}
	#endregion
}
