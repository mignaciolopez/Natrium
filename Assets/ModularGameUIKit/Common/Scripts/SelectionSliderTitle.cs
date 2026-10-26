// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using TMPro;
using UnityEngine;

namespace Ricimi
{
	// This utility component provides a companion title text to represent the
	// options in a selection slider.
	public class SelectionSliderTitle : MonoBehaviour
	{
		public SelectionSlider Slider;
		public string TitleText;

		private TextMeshProUGUI title;

		private void Awake()
		{
			title = GetComponent<TextMeshProUGUI>();
		}

		private void OnEnable()
		{
			Slider.OnOptionSelected += OnOptionSelected;
		}

		private void OnDisable()
		{
			Slider.OnOptionSelected -= OnOptionSelected;
		}

		private void OnOptionSelected(int currOption, int numOptions)
		{
			title.text = $"{TitleText} {currOption + 1}/{numOptions}";
		}
	}
}