// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ricimi
{
	// This UI component represents a modular popup that can be used to easily represent
	// many different types of popups.
    public class ModularPopup : Popup
    {
		[Header("Text")]
		public TextMeshProUGUI Title;
		public TextMeshProUGUI Subtitle;
		public TextMeshProUGUI Message;

		[Space]
		[Header("Image")]
		public Image Image;
		public TextMeshProUGUI Caption;

		[Space]
		[Header("Buttons")]
		public GameObject ButtonGroup;
		public List<Button> Buttons;

        public void Initialize(ModularPopupOpener opener)
        {
			SetLabel(Title, opener.Title);
			SetLabel(Subtitle, opener.Subtitle);
			SetLabel(Message, opener.Message);

			SetImage(Image, opener.Image, opener.TintColor);
			SetLabel(Caption, opener.Caption);

			foreach (var button in Buttons)
			{
				button.gameObject.SetActive(false);
			}

			if (opener.Buttons.Count == 0)
			{
				ButtonGroup.SetActive(false);
			}
			else
			{
				for (var i = 0; i < opener.Buttons.Count; i++)
				{
					SetButton(Buttons[i], opener.Buttons[i]);
				}
			}
        }

		private void SetLabel(TextMeshProUGUI label, string text)
		{
			if (label == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(text))
			{
				label.text = text;
			}
			else
			{
				label.gameObject.SetActive(false);
			}
		}

		private void SetImage(Image image, Sprite sprite, Color32 color)
		{
			if (image == null)
			{
				return;
			}

			if (sprite != null)
			{
				image.sprite = sprite;
				image.color = color;
			}
			else
			{
				image.gameObject.SetActive(false);
			}
		}

		private void SetButton(Button button, ButtonInfo info)
		{
			if (button == null)
			{
				return;
			}

			button.gameObject.SetActive(true);
			var label = button.GetComponentInChildren<TextMeshProUGUI>();
			if (label != null)
			{
				label.text = info.Label;
			}
			if (!info.IgnoreButtonClickedEvent)
			{
				button.onClick = info.OnClickedEvent;
			}
			if (info.ClosePopupWhenClicked)
			{
				button.onClick.AddListener(Close);
			}
		}
    }
}
