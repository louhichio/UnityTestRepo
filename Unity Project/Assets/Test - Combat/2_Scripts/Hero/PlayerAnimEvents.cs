using UnityEngine;
using System.Collections;

public class PlayerAnimEvents : MonoBehaviour 
{	
	#region Properties
	PlayerManager pl;
	Animator anim;
	#endregion

	#region Unity
	void Awake () 
	{
		pl = PlayerManager.Instance;
		anim = GetComponent<Animator> ();
	}


	void OnAnimatorMove() 
	{
		Vector3 newPosition = anim.deltaPosition;
		newPosition.y = 0;
		transform.parent.position += newPosition;
	}
	#endregion

	#region Public
	public void OpenBuffer()
	{
		pl.OpenBuffer ();
	}

	public void StrikePoint()
	{
		pl.StrikePoint ();
	}

	public void CloseBuffer()
	{
		pl.CloseBuffer ();
	}

	public void EndAttack()
	{
		pl.EndAttack ();
	}

	public void KnockDownStrikePoint()
	{
		pl.KnockDownStrikePoint ();
	}

	public void LaunchArrow()
	{
		pl.LaunchArrow ();
	}

	public void EndAnim()
	{
		pl.EndAnim ();
	}
	#endregion
}
