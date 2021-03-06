﻿using UnityEngine;
using System.Collections;

public class WeakEnemy : Enemy 
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
		type = Type.Weak;
		health = 60;
		attackDamages = 10;
	}
	#endregion
}
