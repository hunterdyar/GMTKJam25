using System;
using GMTK;
using UnityEngine;

namespace UI
{
	public class UIEnableChildrenWhenPrimed : MonoBehaviour
	{
		private void OnEnable()
		{
			TimelineRunner.OnStateChange += OnStateChange;
		}

		private void OnDisable()
		{
			TimelineRunner.OnStateChange -= OnStateChange;
		}

		private void OnStateChange(RunnerControlState state)
		{
			SetAllChildrenActive(state == RunnerControlState.WaitingToRecord);
		}

		private void SetAllChildrenActive(bool isActive)
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(isActive);
			}
		}
	}
}