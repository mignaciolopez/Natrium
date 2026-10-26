// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using System;
using UnityEngine.UI;

namespace Ricimi
{
	// Utility class used to store the button information in the ModularPopupOpener component.
	[Serializable]
	public class ButtonInfo
	{
		public string Label;
		public bool ClosePopupWhenClicked;
		public bool IgnoreButtonClickedEvent;
		public Button.ButtonClickedEvent OnClickedEvent;
	}
}