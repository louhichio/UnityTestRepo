using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager> 
{
	[SerializeField]
	private Text comboMeterText;	
	[SerializeField]
	private Image comboMeterBar;

	[SerializeField]
	private Image healthBar;
	[SerializeField]
	private GameObject gameOver;

	#region Unity
	void Start () 
	{
	
	}

	void Update () 
	{
//		UpdateComboMeter ();
	}
	#endregion

	#region Private
	private void UpdateComboMeter()
	{
		comboMeterText.text = "ComboMeter:" + PlayerManager.Instance.GetComboMeter ();
	}
	#endregion

	#region Public
	public void UpdateHealth(float fillAmount)
	{
		healthBar.fillAmount = fillAmount;
	}

	public void UpdateCombo(float fillAmount, int timeLeft)
	{
		comboMeterBar.fillAmount = fillAmount;

		if (fillAmount != 0) 
		{
			timeLeft++;
			comboMeterText.text = timeLeft.ToString ();
		}
		else
			comboMeterText.text = "";
	}

	public void GameOver()
	{
		gameOver.SetActive (true);
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene (0);
	}
	#endregion
}
