using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gridsnap.Morons
{
	[Serializable]
	public class StatemapField : ISerializationCallbackReceiver
	{
		[SerializeField]
		public String name;
		
		[NonSerialized]
		public System.Object value;

		public bool editable = true;
		public Type type;

		[SerializeField, HideInInspector]
		protected String stringifiedValue;

		public StatemapField()
		{}

		public StatemapField(String name, Type type, System.Object value, bool editable)
		{
			this.name = name;
			this.editable = editable;
			set(type, value);
		}
		
		public void set(Type type, System.Object value)
		{
			IdentifiedGameObject identified;

			this.type = type;
			this.value = value;

			if(type == typeof(GameObject) || type == typeof(IdentifiedGameObject))
			{
				this.type = typeof(IdentifiedGameObject);

				identified = (IdentifiedGameObject)value;

				if(identified != null && identified.gameObject != null)
					identified = StatemapField.ensureGUID(identified.gameObject);
				
				this.value = identified;
			}
		}

		public void OnBeforeSerialize()
		{
			BinaryWriter writer;
			MemoryStream stream;

			stream = new MemoryStream();
			writer = new BinaryWriter(stream);

			writer.Write(type.FullName);

			if(editable)
				BinarySerializer.serializeSimpleObject(writer, value, type);
			
			stringifiedValue = Convert.ToBase64String(stream.ToArray());
		}

		public void OnAfterDeserialize()
		{
			BinaryReader reader;
				
			reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(stringifiedValue)));
			type = BinarySerializer.findType(reader.ReadString());
			
			if(!editable)
				return;
			value = BinarySerializer.deserializeSimpleObject(reader);
		}

		/// <summary>
		///		Ensures that the given GameObject is present in the scene, and has the appropriate invisible child indicating its presence.
		///		If the [obj] is a prefab, it will be instantiated silently in the scene and assigned a guid corrosponding to its asset guid.
		///		If the [obj] is in the scene, it will be ensured that it has the appropriate invisible child.
		/// 	In either case, the appropriate guid to use for the [obj] will be returned.
		///		If the [obj] could not be found in scene or assets, a blank string is returned.
		/// </summary>
		private static IdentifiedGameObject ensureGUID(GameObject obj)
		{
			IdentifiedGameObject identified;
			SerializedGameObject serializedMarker;
			String guid;

			// see if we have it tracked already
			foreach(IdentifiedGameObject val in SerializedGameObject.identifiedObjects.Values)
				if(val.gameObject == obj)
					return val;
				
			// scene object?
			serializedMarker = obj.GetComponent<SerializedGameObject>();
			if(serializedMarker != null)
			{
				guid = serializedMarker.guid;

				// make sure that if this is a prefab, an instantiated version exists in the scene.
				StatemapDefinition.getHeldObject(guid);

				// add to registry
				identified = new IdentifiedGameObject(guid, obj);
				SerializedGameObject.identifiedObjects[guid] = identified;
				return identified;
			}

			// otherwise, check if it's from the asset database (if we're in the editor).
			identified = ensureAssetGUID(obj);
			if(identified != null)
				return identified;

			// in the scene but just not marked yet.
			guid = GUID.Generate().ToString().Replace("-", "");
			serializedMarker = obj.AddComponent<SerializedGameObject>();
			serializedMarker.guid = guid;

			identified = new IdentifiedGameObject(guid, obj);
			SerializedGameObject.identifiedObjects[guid] = identified;
			return identified;
		}

		private static IdentifiedGameObject ensureAssetGUID(GameObject obj)
		{
			#if UNITY_EDITOR

			IdentifiedGameObject ret;
			GameObject instantiated;
			SerializedGameObject marker;
			String guid, path;

			path = AssetDatabase.GetAssetPath(obj);
			if(String.IsNullOrEmpty(path))
				return null;

			// this is definitely an asset, see if it's already invisibly in the scene.
			guid = AssetDatabase.AssetPathToGUID(path);
			ret = StatemapDefinition.getHeldObject(guid);
			if(ret != null)
				return ret;
				
			// doesn't yet exist. Instantiate an invisible copy in the instance, make sure the guid is discoverable from the prefab.
			guid = GUID.Generate().ToString().Replace("-", "");

			instantiated = (GameObject)GameObject.Instantiate(obj);
			instantiated.transform.SetParent(StatemapDefinition.getGUIDHolder().transform, false);
			marker = instantiated.AddComponent<SerializedGameObject>();
			marker.guid = guid;
			instantiated.SetActive(false);

			ret = new IdentifiedGameObject(guid, obj);
			SerializedGameObject.identifiedObjects[guid] = ret;
			return ret;
			#endif
		}
	}
}