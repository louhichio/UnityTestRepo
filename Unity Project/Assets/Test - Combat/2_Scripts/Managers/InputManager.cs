using UnityEngine;
using System.Collections;

public class InputManager : Singleton<InputManager> 
{
	#region Properties
	[Tooltip("Pourcentage de la largeur de l'ecran a parcourir avec le doigt")]
	[SerializeField]
	private float minSwipeInputLength = 10;
	private float maxTapTimer = 0.5f;

	private Vector3 touchStartPosition;
	private float touchStartTime;

	private float screenWidth;
	private bool canAnalyseTouch = false;
	#endregion

	#region Unity
	void Start()
	{
		screenWidth = Screen.width / 100;
	}

	void Update () 
	{
		#if UNITY_EDITOR

		EditorInputCheck();

		#elif UNITY_ANDROID || UNITY_IOS

		MobileInputCheck();

		#endif
	}
	#endregion

	#region Private
	private void EditorInputCheck()
	{
		if (Input.GetKeyDown (KeyCode.Mouse0)) 
		{
			touchStartTime = Time.time;
			touchStartPosition = Input.mousePosition;
		}

		if (Input.GetKeyUp (KeyCode.Mouse0)) 
		{
			float dist = Vector3.Distance (touchStartPosition, Input.mousePosition);

			if (dist >= minSwipeInputLength * screenWidth) 
			{
				PlayerManager.Instance.SwipeDetected ();
			} 
			else 
			{
				float timer = Time.time - touchStartTime;

				if (timer <= maxTapTimer) 
				{
					if (Input.mousePosition.x < Screen.width / 2) 
					{
						PlayerManager.Instance.LeftTapDetected ();
					} 
					else if (Input.mousePosition.x > Screen.width / 2) 
					{
						PlayerManager.Instance.RightTapDetected ();
					}
				}
			}
		}
	}

	private void MobileInputCheck()
	{
		if (Input.touches.Length == 1)
		{
			Touch touch = Input.touches[0];
			switch (touch.phase)
			{
			case TouchPhase.Began :		
				canAnalyseTouch = true;
				touchStartTime = Time.time;
				touchStartPosition = touch.position;
				break;

			case TouchPhase.Canceled :	
				canAnalyseTouch = false;
				break;

			case TouchPhase.Ended :		
				if(canAnalyseTouch)
				{
					float dist = Vector3.Distance (touchStartPosition, touch.position);

					if (dist >= minSwipeInputLength * screenWidth) 
					{
						PlayerManager.Instance.SwipeDetected ();
					} 
					else 
					{
						float timer = Time.time - touchStartTime;

						if (timer <= maxTapTimer) 
						{
							if (touch.position.x < Screen.width / 2) 
							{
								PlayerManager.Instance.LeftTapDetected ();
							} 
							else if (touch.position.x > Screen.width / 2) 
							{
								PlayerManager.Instance.RightTapDetected ();
							}
						}
					}
				}
				break;
			}			
		}
		else if(canAnalyseTouch)	
			canAnalyseTouch = false;
	}
	#endregion
}