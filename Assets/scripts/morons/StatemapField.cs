using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	[Serializable]
	public class StatemapField : ISerializationCallbackReceiver
	{
		[SerializeField]
		public String name;
		
		[NonSerialized]
		public System.Object value;

		[SerializeField, HideInInspector]
		protected String stringifiedValue;

		public Type getType()
		{
			return value.GetType();
		}
		
		public void OnBeforeSerialize()
		{
			BinaryWriter writer;
			MemoryStream stream;

			stream = new MemoryStream();
			writer = new BinaryWriter(stream);

			BinarySerializer.serializeSimpleObject(writer, value);
			stringifiedValue = Convert.ToBase64String(stream.ToArray());
		}

		public void OnAfterDeserialize()
		{
			BinaryReader reader;

			reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(stringifiedValue)));
			value = BinarySerializer.deserializeSimpleObject(reader);
		}
	}
}