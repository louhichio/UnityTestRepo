using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : Singleton<EnemyManager> 
{
	#region Properties
	private Transform marker;

	private List<Enemy> enemyList = new List<Enemy>();
	private Enemy currentTargetedEnemy;
	#endregion

	#region Unity
	void Awake () 
	{	
		GenerateListOfEnemys ();
	}

	void Start()
	{
		marker.gameObject.SetActive (false);
	}

	void Update () 
	{	
		if (!Enemy.ReferenceEquals (currentTargetedEnemy, null)) 
		{
			marker.gameObject.SetActive (true);

			Vector3 pos = currentTargetedEnemy.transform.position;
			pos.y = 1.8f;
			marker.position = pos;

			marker.LookAt (Camera.main.transform.position);
		}
		else
			marker.gameObject.SetActive (false);
			
	}
	#endregion

	#region Private
	private void GenerateListOfEnemys()
	{
		foreach (Transform child in transform) 
		{
			if (child.GetComponent<Enemy> ())
				enemyList.Add (child.GetComponent<Enemy> ());
			else if (child.GetComponent<SpriteRenderer> ())
				marker = child;
		}
	}
	#endregion

	#region Public
	public Enemy GetNewTarget(Enemy currentTarget, float coneLength, float coneAngle)
	{
		List<Enemy> temp = new List<Enemy>();

		foreach (var enemy in enemyList) 
		{			
			if (enemy.GetDistance() < coneLength) 
			{
				if (enemy.GetAngle() <= coneAngle && enemy.GetAngle() >= -coneAngle) 
					temp.Add (enemy);
			}
		}

		if (!List<Enemy>.ReferenceEquals (null, temp) && temp.Count > 1) 
		{
			temp.Sort (delegate(Enemy a, Enemy b)
				{
					int compareThreat = a.GetThreat ().CompareTo (b.GetThreat ());
					
					if (compareThreat == 0)
						return (a.GetDistance ()).CompareTo (b.GetDistance ());

					return compareThreat;
				});
			currentTargetedEnemy = temp.Find (x => x != currentTarget);
			return currentTargetedEnemy;
		} 
		else 
		{
			temp = enemyList;
			temp.Sort (delegate(Enemy a, Enemy b) 
				{
//					int compareThreat = a.GetThreat().CompareTo (b.GetThreat());

//					if(compareThreat == 0)
						return (a.GetDistance ()).CompareTo (b.GetDistance ());

//					return compareThreat;
				});
			currentTargetedEnemy = temp.Find (x => x != currentTarget);
			return currentTargetedEnemy;
		}

		return null;
	}

	public Enemy GetClosestEnemy()
	{
		Enemy closestEnemy = null;
		Vector3 pl = PlayerManager.Instance.transform.position;

		float dist = Mathf.Infinity;
		float distTemp;

		foreach (var enemy in enemyList) 
		{
			distTemp = Vector3.Distance (pl, enemy.transform.position);

			if (distTemp < dist) 
			{
				closestEnemy = enemy;
				dist = distTemp;
			}
		}
		return closestEnemy;
	}
	#endregion
}
