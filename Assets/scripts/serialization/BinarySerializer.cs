using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Gridsnap.Morons
{
	public abstract class BinarySerializer
	{
		public static void serializeSimpleObject(BinaryWriter writer, System.Object value)
		{
			serializeSimpleObject(writer, value, value.GetType());
		}
		public static void serializeSimpleObject(BinaryWriter writer, System.Object value, Type type)
		{
			writer.Write(type.FullName);
			
			if(type == typeof(bool))
				writer.Write((bool)value);

			if(type == typeof(int))
				writer.Write((int)value);
			
			if(type == typeof(float))
				writer.Write((float)value);

			if(type == typeof(Vector3))
				writeVector3(writer, (Vector3)value);

			if(type == typeof(String))
				if(String.IsNullOrEmpty((String)value))
					writer.Write("");
				else
					writer.Write((String)value);
			
			// You can't actually serialize a GameObject, the only way it can be stored in is the scene.
			// Therefore we actually store the name of an invisible child of the given GameObject which has a specific component.
			if(type == typeof(GameObject) || type == typeof(IdentifiedGameObject))
			{
				IdentifiedGameObject actualValue;

				if(System.Object.Equals(value, null))
				{
					writer.Write("");
					return;
				}

				actualValue = ((IdentifiedGameObject)value);
				writer.Write(actualValue.guid);
			}
		}

		public static System.Object deserializeSimpleObject(BinaryReader reader)
		{
			Type type;

			type = findType(reader.ReadString());

			if(type == typeof(bool))
				return reader.ReadBoolean();

			if(type == typeof(int))
				return reader.ReadInt32();
			
			if(type == typeof(float))
				return reader.ReadSingle();

			if(type == typeof(Vector3))
				return readVector3(reader);

			if(type == typeof(String))
				return reader.ReadString();
			
			// GameObjects can't be "found" at deserialization-time, so we return the GUID of the object in the scene, and it will be resolved later.
			if(type == typeof(GameObject) || type == typeof(IdentifiedGameObject))
				return new IdentifiedGameObject(reader.ReadString(), null);

			return null;
		}

		public static void writeVector3(BinaryWriter writer, Vector3 vector)
		{
			writer.Write(vector.x);
			writer.Write(vector.y);
			writer.Write(vector.z);
		}

		public static Vector3 readVector3(BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Type findType(String qualified)
		{
			return 
				AppDomain.CurrentDomain.GetAssemblies()
				.Select(a => a.GetType(qualified))
				.Where(t => t != null)
				.FirstOrDefault();
		}
	}
}