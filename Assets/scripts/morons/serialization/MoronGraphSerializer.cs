using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gridsnap.Morons
{
	public partial class MoronGraph
	{
		protected String serialize()
		{
			MemoryStream stream;
			BinaryWriter writer;
			List<Pair<String, String>> joists;
			List<Pair<String, System.Object>> manualJoists;

			stream = new MemoryStream();
			writer = new BinaryWriter(stream);

			writer.Write(nodes.Count);
			foreach(MoronIONode node in nodes)
			{
				writer.Write(node.GetType().FullName);
				writer.Write(node.guid);
				BinarySerializer.writeVector3(writer, node.position);
				writer.Write(node.interruptsIntent);

				// joists
				writeJoists(writer, node.getReadJoists());
				writeJoists(writer, node.getWriteJoists());

				// manual.
				manualJoists = node.getManualJoists();
				writer.Write(manualJoists.Count);
				foreach(Pair<String, System.Object> joist in manualJoists)
				{
					writer.Write(joist.key);
					BinarySerializer.serializeSimpleObject(writer, joist.value);
				}
			}

			writer.Write(edges.Count);
			foreach(Pair<int, int> edge in edges)
			{
				writer.Write(edge.key);
				writer.Write(edge.value);
			}

			return Convert.ToBase64String(stream.ToArray());
		}

		protected void deserialize(String serialized)
		{
			Type t;
			BinaryReader reader;
			MoronIONode node;
			int count, topCount;

			reader = new BinaryReader(new MemoryStream(Convert.FromBase64String(serialized)));

			topCount = reader.ReadInt32();
			nodes = new List<MoronIONode>(topCount);

			for(int i = 0; i < topCount; i++)
			{
				t = BinarySerializer.findType(reader.ReadString());
				node = (MoronIONode)Activator.CreateInstance(t);
				node.guid = reader.ReadString();
				node.position = BinarySerializer.readVector3(reader);
				node.interruptsIntent = reader.ReadBoolean();

				count = reader.ReadInt32();
				for(int z = 0; z < count; z++)
					node.setReadJoist(reader.ReadString(), reader.ReadString());

				count = reader.ReadInt32();
				for(int z = 0; z < count; z++)
					node.setWriteJoist(reader.ReadString(), reader.ReadString());

				count = reader.ReadInt32();
				for(int z = 0; z < count; z++)
					node.setManualJoist(reader.ReadString(), BinarySerializer.deserializeSimpleObject(reader));
				
				nodes.Add(node);
			}

			topCount = reader.ReadInt32();
			for(int i = 0; i < topCount; i++)
				edges.Add(new Pair<int, int>(reader.ReadInt32(), reader.ReadInt32()));
		}

		protected void writeJoists(BinaryWriter writer, List<Pair<String, String>> joists)
		{
			writer.Write(joists.Count);
				
			foreach(Pair<String, String> joist in joists)
			{
				writer.Write(joist.key);
				writer.Write(joist.value);
			}
		}
	}
}