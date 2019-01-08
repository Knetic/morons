using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	public class IdentifiedGameObject
	{
		public String guid;
		public GameObject gameObject;

		public IdentifiedGameObject()
		{}

		public IdentifiedGameObject(String guid, GameObject gameObject)
		{
			this.guid = guid;
			this.gameObject = gameObject;
		}
	}
}