using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	[DisallowMultipleComponent, ExecuteInEditMode, HideInInspector]
	public class SerializedGameObject : MonoBehaviour
	{
		public static Dictionary<String, IdentifiedGameObject> identifiedObjects = new Dictionary<String, IdentifiedGameObject>();
		public String guid;

		public void OnEnable()
		{
			this.hideFlags = HideFlags.HideInInspector;
			
			if(String.IsNullOrEmpty(guid))
				return;

			identifiedObjects[guid] = new IdentifiedGameObject(guid, gameObject);
		}
	}
}