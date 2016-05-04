using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : Singleton<EnemyManager> 
{
	#region Properties
	private Transform marker;
	[SerializeField]
	private List<Enemy> enemyList = new List<Enemy>();
	private Enemy currentTargetedEnemy;

	private float attTimer = 0;
	private float attWaitTime = 2.0f;

	private bool isEnemyAttacking;
	private Enemy lastAttackingEnemy; 

	[SerializeField]
	private Transform placementPoints;

	private List<Transform> linkedPlacementPoints = new List<Transform>();
	private List<Transform> unlinkedPlacementPoints = new List<Transform>();
	private List<Transform> meleeRangePlacementPoints = new List<Transform>();
	#endregion

	#region Unity
	void Awake () 
	{	
		GenerateListOfEnemys ();
	}

	void Start()
	{
		marker.gameObject.SetActive (false);
		GeneratePositionList ();
	}

	void Update () 
	{
		UpdateMarkerPosition ();
		AttackManager ();
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

	private void UpdateMarkerPosition()
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

	private List<Enemy> GetEnemiesInCone()
	{
		float coneLength = PlayerManager.Instance.GetConeLength ();
		float coneAngle = PlayerManager.Instance.GetConeAngle ();

		List<Enemy> temp = new List<Enemy>();
		foreach (var enemy in enemyList) 
		{			
			if (enemy.CanBeAttacked() && enemy.GetDistance() < coneLength) 
			{
				if (enemy.GetAngle() <= coneAngle && enemy.GetAngle() >= -coneAngle) 
					temp.Add (enemy);
			}
		}
		return temp;
	}

	private void AttackManager()
	{
		if(!isEnemyAttacking && Time.time > attTimer && !PlayerManager.Instance.IsDead())
		{
			List<Enemy> enemiesInCone = enemyList.FindAll (x => 
				x.CanAttack() && 
				x.IsInCameraFOV() &&
				x.GetDistance() < 2.12f);

			if (!List<Enemy>.ReferenceEquals (enemiesInCone, null)) 
			{
				if (enemiesInCone.Count > 1) 
				{
					lastAttackingEnemy = enemiesInCone.Find (x => x != lastAttackingEnemy);
					lastAttackingEnemy.AllowedToAttack ();
					isEnemyAttacking = true;

				} 
				else if (enemiesInCone.Count == 1) 
				{
					lastAttackingEnemy = enemiesInCone [0];
					lastAttackingEnemy.AllowedToAttack ();
					isEnemyAttacking = true;
				}
			}
		}
  	}

	//Sets a placement position to closest Enemy
	private void GeneratePositionList()
	{
		if (placementPoints != null) 
		{
			float dist;
			Enemy temp;

			foreach (Transform point in placementPoints) 
			{
				if (!List<Transform>.ReferenceEquals (meleeRangePlacementPoints, null)) 
				{
					if(meleeRangePlacementPoints.Count < 4)
						meleeRangePlacementPoints.Add (point);						
				} 
				else
					meleeRangePlacementPoints.Add (point);

				dist = Mathf.Infinity;
				temp = null;

				foreach (var enemy in enemyList) 
				{
					float currentDist = Vector3.Distance (enemy.transform.position, point.position);

//					print (currentDist + "   " + dist + "  " + enemy.Linked ());
					if (currentDist < dist && !enemy.Linked ()) 
					{
						temp = enemy;
					}
				}

				if (!Enemy.ReferenceEquals (temp, null)) 
				{
					temp.SetPlacementPoint (point);
					linkedPlacementPoints.Add (point);
				} else
					unlinkedPlacementPoints.Add (point);
			}


		}
		else print("Placement Points Gameobject isn't linked to EnemyManager");
  	}
	#endregion

	#region Public
	public Enemy GetNewTarget(Enemy currentTarget, float coneLength, float coneAngle)
	{
		List<Enemy> temp = GetEnemiesInCone ();

		if (!List<Enemy>.ReferenceEquals (null, temp) && temp.Count > 1) 
		{
			temp.Sort (delegate(Enemy a, Enemy b)
				{
					int compareThreat = b.GetThreat ().CompareTo (a.GetThreat ());
					
					if (compareThreat == 0)
						return (a.GetDistance ()).CompareTo (b.GetDistance ());

					return compareThreat;
				});
			currentTargetedEnemy = temp.Find (x => x != currentTarget);
			return currentTargetedEnemy;
		} 
		else 
		{
			temp = enemyList.FindAll(x => x.CanBeAttacked());

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
	}

	public Enemy GetClosestEnemy()
	{
		currentTargetedEnemy = null;
		Vector3 pl = PlayerManager.Instance.transform.position;

		float dist = Mathf.Infinity;
		float distTemp;

		foreach (var enemy in enemyList) 
		{
			distTemp = Vector3.Distance (pl, enemy.transform.position);

			if (enemy.CanBeAttacked() && distTemp < dist) 
			{
				currentTargetedEnemy = enemy;
				dist = distTemp;
			}
		}
		return currentTargetedEnemy;
	}

	public void CheckAnyEnemyLeft()
	{
		if (Enemy.ReferenceEquals (enemyList.Find (x => x.isAlive()), null)) 
		{
			UIManager.Instance.GameOver ();
		}
	}

	public void EnemyFinishedAttack()
	{
		attTimer = Time.time + attWaitTime;
		isEnemyAttacking = false;
	}

	public void FreePlacementPoint (Transform placementPoint, Enemy enemyToRemove)
	{
		enemyList.Remove (enemyToRemove);

		linkedPlacementPoints.Remove (placementPoint);
		unlinkedPlacementPoints.Add (placementPoint);

		if (meleeRangePlacementPoints.Contains (placementPoint)) 
		{
			float dist = Mathf.Infinity;
			Enemy temp = null;

			foreach (var enemy in enemyList) 
			{
				float currentDist = Vector3.Distance (enemy.transform.position, placementPoint.position);

				//					print (currentDist + "   " + dist + "  " + enemy.Linked ());
				if (currentDist < dist && !meleeRangePlacementPoints.Contains (enemy.GetPlacementPoint())) 
				{
					temp = enemy;
				}
			}

			if (!Enemy.ReferenceEquals (temp, null)) 
			{
				linkedPlacementPoints.Remove (temp.GetPlacementPoint());
				unlinkedPlacementPoints.Add (temp.GetPlacementPoint());


				temp.SetPlacementPoint (placementPoint);
				linkedPlacementPoints.Add (placementPoint);
				unlinkedPlacementPoints.Remove (placementPoint);
			}
		}
	}
	#endregion
}
