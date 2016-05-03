using System;
using System.Collections.Generic;

public class AnimAction
{
	private List<string> actionAnimList;
	private int currentAction;
	public float maxRange;

	public AnimAction (List<string> actionAnimList)
	{
		this.actionAnimList = actionAnimList;
	}

	public AnimAction (List<string> actionAnimList, float maxRange)
	{
		this.actionAnimList = actionAnimList;
		this.maxRange = maxRange;
		this.currentAction = 0;
	}

	public string GetActionAnimName(int index)
	{
		currentAction = index;
		return actionAnimList [index];
	}

	public string GetNextActionName()
	{
		return actionAnimList[currentAction + 1 < actionAnimList.Count ? currentAction + 1 : 0];
	}

	public void ResetCurrentAction()
	{
		this.currentAction = 0;
	}
}


