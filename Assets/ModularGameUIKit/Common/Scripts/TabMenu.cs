// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ricimi
{
	// This UI component provides a tabbed menu that allows you to switch between
	// different, independent tabs of content.
	public class TabMenu : MonoBehaviour
	{
		public List<GameObject> TabOnGroup;
		public List<GameObject> TabOffGroup;
		public List<GameObject> Content;
		public Transform Root;

		private List<Toggle> toggles = new List<Toggle>();
		private GameObject currentGroup;

		private void Start()
		{
			foreach (var toggle in GetComponentsInChildren<Toggle>())
			{
				var tabToggle = toggle.gameObject.AddComponent<TabToggle>();
				tabToggle.TabMenu = this;
				toggles.Add(toggle);
			}

			SetToggleEnabled(0, true);
			for (var i = 1; i < toggles.Count; i++)
			{
				SetToggleEnabled(i, false);
			}
		}

		public void SetToggleEnabled(int index, bool value)
		{
			TabOnGroup[index].SetActive(value ? true : false);
			TabOffGroup[index].SetActive(value ? false : true);

			if (value)
			{
				if (currentGroup != null)
				{
					Destroy(currentGroup);
				}

				currentGroup = Instantiate(Content[index], Root != null ? Root : transform, false);
			}
		}

		public void OnToggleChanged(Toggle toggle, bool value)
		{
			var index = toggles.IndexOf(toggle);
			if (index >= 0)
			{
				SetToggleEnabled(index, value);
			}
		}
	}
}