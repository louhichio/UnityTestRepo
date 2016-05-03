using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : Singleton<UIManager> 
{
	[SerializeField]
	private Text text;

	[SerializeField]
	private Text comboMeterText;

	#region Unity
	void Start () 
	{
	
	}

	void Update () 
	{
		UpdateComboMeter ();
	}
	#endregion

	#region Private
	private void UpdateComboMeter()
	{
		comboMeterText.text = "ComboMeter:" + PlayerManager.Instance.GetComboMeter ();
	}
	#endregion

	#region Public
	public void ShowText(string value)
	{
		text.text = value;
	}
	#endregion
}
