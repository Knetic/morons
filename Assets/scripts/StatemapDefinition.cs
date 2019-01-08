using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	[Serializable]
	public class StatemapDefinition
	{
		private static readonly String GAMEOBJECT_GUID_HOLDER_NAME = "__vale_GUID_HOLDER";

		[SerializeField, HideInInspector]
		public List<StatemapField> definitions;

		public StatemapDefinition()
		{
			definitions = new List<StatemapField>();
			definitions.Add(new StatemapField("self", typeof(IdentifiedGameObject), null, false));
			definitions.Add(new StatemapField("one", typeof(float), 1f, false));
		}

		public IEnumerable<StatemapField> getDefinitions()
		{
			return definitions;
		}

		public System.Object get(String key)
		{
			return
				definitions
				.Where(d => d.name == key)
				.Select(d => d.value)
				.FirstOrDefault();
		}

		public void set(String key, Type t, System.Object value)
		{
			StatemapField field;

			field =
				definitions
				.Where(d => d.name == key)
				.FirstOrDefault();
			
			if(field == null)
			{
				field = new StatemapField();
				field.name = key;
				definitions.Add(field);
			}
			
			field.set(t, value);
		}

		public IEnumerable<StatemapField> getFieldsForType(Type t)
		{
			if(t == typeof(GameObject) || t == typeof(IdentifiedGameObject))
				return 
					definitions
					.Where(d => d.type == typeof(GameObject) || d.type == typeof(IdentifiedGameObject));

			return
				definitions
				.Where(d => d.type == t);
		}

		public void resolveFields()
		{
			IdentifiedGameObject stored, identified;

			foreach(StatemapField field in definitions)
			{
				if(field.type == typeof(IdentifiedGameObject))
				{
					stored = (IdentifiedGameObject)field.value;
					if(stored == null || String.IsNullOrEmpty(stored.guid))
						continue;

					identified = StatemapDefinition.getHeldObject(stored.guid);
					if(identified == null)
					{
						Debug.LogError("Unable to resolve moron gameObject statemap field with guid '" + stored.guid + "'");
						continue;
					}

					stored.gameObject = identified.gameObject;
				}
			}
		}

		public static GameObject getGUIDHolder()
		{
			GameObject holder;

			holder = GameObject.Find(GAMEOBJECT_GUID_HOLDER_NAME);
			if(holder != null)
				return holder;
			
			holder = new GameObject(GAMEOBJECT_GUID_HOLDER_NAME);
			holder.hideFlags = HideFlags.HideInHierarchy;
			return holder;
		}

		public static IdentifiedGameObject getHeldObject(String guid)
		{
			if(SerializedGameObject.identifiedObjects.ContainsKey(guid))
				return SerializedGameObject.identifiedObjects[guid];

			return null;
		}
	}
}