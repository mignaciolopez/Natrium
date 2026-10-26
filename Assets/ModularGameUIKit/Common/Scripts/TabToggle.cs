// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using UnityEngine;
using UnityEngine.UI;

namespace Ricimi
{
	// This utility component is used to toggle between the different tabs of content
	// in a tabbed menu. See the associated TabMenu script.
	public class TabToggle : MonoBehaviour
	{
		public TabMenu TabMenu;

		private Toggle toggle;

		private void Awake()
		{
			toggle = GetComponent<Toggle>();
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		private void OnDestroy()
		{
			toggle.onValueChanged.RemoveListener(OnValueChanged);
		}

		private void OnValueChanged(bool value)
		{
			TabMenu.OnToggleChanged(toggle, value);
		}
	}
}