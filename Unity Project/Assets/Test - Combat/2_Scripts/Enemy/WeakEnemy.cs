using UnityEngine;
using System.Collections;

public class WeakEnemy : Enemy 
{
	public override void Init ()
	{
		type = Type.Weak;
		health = 60;
		attackDamages = 10;
	}
}
