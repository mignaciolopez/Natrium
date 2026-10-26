// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace Ricimi
{
	// This UI component represents a rounded progress bar.
	public class CircularProgressBar : MonoBehaviour
	{
		[Range(0,100)]
		public float Percentage = 0;

		[SerializeField]
		private Image progressImage;
		[SerializeField]
		private RectTransform progressEndContainer;
		[SerializeField]
		private RectTransform progressEndImage;

		private void Update()
		{
			UpdateProgress(Percentage);
		}

		private void UpdateProgress(float value)
		{
			float fillAmount = (value / 100.0f);
			progressImage.fillAmount = fillAmount;
			float angle = fillAmount * 360.0f;
			progressEndContainer.localEulerAngles = new Vector3(0, 0, -angle);
			progressEndImage.localEulerAngles = new Vector3(0, 0, angle);
		}

		public float GetFillAmount()
		{
			return progressImage.fillAmount;
		}
	}
}