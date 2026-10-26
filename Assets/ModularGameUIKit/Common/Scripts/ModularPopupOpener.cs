// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ricimi
{
    // Utility component to open a modular popup. See the associated ModularPopup script.
    public class ModularPopupOpener : PopupOpener
    {
		[Header("Text")]
		public string Title;
		public string Subtitle;
		[TextArea(minLines: 3, maxLines: 3)]
		public string Message;

		[Space]
		[Header("Image")]
		public Sprite Image;
		public Color32 TintColor = Color.white;
		public string Caption;

		[Space]
		[Header("Buttons")]
		public List<ButtonInfo> Buttons;

        public override void OpenPopup()
        {
            base.OpenPopup();
            m_popup.GetComponent<ModularPopup>().Initialize(this);
        }
    }
}
