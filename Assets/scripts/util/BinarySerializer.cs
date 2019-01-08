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
			Type type;

			if(value == null)
				return;
			
			type = value.GetType();
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
				writer.Write((String)value);
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