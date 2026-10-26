// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using TMPro;
using UnityEngine;

namespace Ricimi
{
	// This UI component automatically updates the associated text to the progress
	// of the referenced circular progress bar.
	public class CircularProgressBarText : MonoBehaviour
	{
		[SerializeField]
		private CircularProgressBar progressBar;

		private TextMeshProUGUI text;

		private void Awake()
		{
			text = GetComponent<TextMeshProUGUI>();
		}

		private void Update()
		{
			text.text = ((int)(progressBar.GetFillAmount() * 100)).ToString();
		}
	}
}