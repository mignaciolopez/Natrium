// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ricimi
{
	// This UI component allows you to scroll through a list of different prefabs
	// using buttons.
	public class SelectionSlider : MonoBehaviour
	{
		public List<GameObject> Options;
		public Transform Root;

		public Action<int, int> OnOptionSelected;

		private int prevOption;
		private int selectedOption;
		private GameObject currentGroup;

		private void Start()
		{
			LoadNewGroup();
		}

		public void OnPrevButtonClicked()
		{
			prevOption = selectedOption;
			selectedOption--;
			if (selectedOption < 0)
			{
				selectedOption = Options.Count - 1; 
			}

			LoadNewGroup();
		}

		public void OnNextButtonClicked()
		{
			prevOption = selectedOption;
			selectedOption = (selectedOption + 1) % Options.Count;

			LoadNewGroup();
		}

		private void LoadNewGroup()
		{
			if (currentGroup != null)
			{
				Destroy(currentGroup);
			}

			currentGroup = Instantiate(Options[selectedOption], Root != null ? Root : transform, false);
			OnOptionSelected?.Invoke(selectedOption, Options.Count);
		}
	}
}