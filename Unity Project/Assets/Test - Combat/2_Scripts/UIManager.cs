using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : Singleton<UIManager> 
{
	[SerializeField]
	private Text text;

	#region Unity
	void Start () 
	{
	
	}

	void Update () {
	
	}
	#endregion

	#region Public
	public void ShowText(string value)
	{
		text.text = value;
	}
	#endregion
}
