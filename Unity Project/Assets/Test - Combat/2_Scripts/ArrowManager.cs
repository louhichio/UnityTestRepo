using UnityEngine;
using System.Collections;

public class ArrowManager : Singleton<ArrowManager> 
{
	#region Properties
	private MeshRenderer mr;
	private Transform target;
	private bool isFollow = false;
	private float speed = 0.01f;
	private float distLeft = 0.05f;

	[SerializeField]
	private Transform initPos;
	#endregion

	#region Unity
	void Awake()
	{
		mr = GetComponentInChildren<MeshRenderer> ();
	}
	void Start () 
	{
		mr.enabled = true;
	}
	void Update () 
	{
		if (isFollow)
			Follow ();
	}
	#endregion

	#region Private
	private void Follow()
	{
		float dist = Vector3.Distance (transform.position, target.position);

		if (dist < distLeft) 
		{
			isFollow = false;
//			mr.enabled = false;
		}
		else 
		{
			transform.LookAt (target.position);
			transform.position += transform.forward.normalized * (Time.deltaTime + speed);
		}
	}
	#endregion

	#region Public
	public void Launch(Transform target)
	{
		this.target = target;

		transform.position = initPos.position;
		transform.LookAt (target.position);

		isFollow = true;
		mr.enabled = true;
	}
	#endregion
}
