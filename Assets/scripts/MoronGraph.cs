using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Holds all information required to create connected IONodes for a Moron.
	///		Also conveniently serializes and deserializes and validity-checks the graph.
	///		Generally used by the EditorWindow as the main data source, and loaded once per graph during gameplay.
	/// </summary>
	[Serializable]
	public partial class MoronGraph : ScriptableObject, ISerializationCallbackReceiver
	{
		[NonSerialized]
		public List<MoronIONode> nodes = new List<MoronIONode>();

		[NonSerialized]
		public List<Pair<int, int>> edges = new List<Pair<int, int>>();

		[SerializeField]
		public StatemapDefinition statemapDefinition = new StatemapDefinition();

		[SerializeField, HideInInspector]
		private String serialized;

		/// <summary>
		///		Creates a real statemap from the definitions in this graph.
		/// </summary>
		public Dictionary<String, System.Object> createStatemap()
		{
			Dictionary<String, System.Object> statemap;

			statemap = new Dictionary<String, System.Object>();
			statemapDefinition.resolveFields();

			foreach(StatemapField field in statemapDefinition.getDefinitions())
				statemap.Add(field.name, field.value);
			
			return statemap;
		}

		/// <summary>
		///		Creates and returns the root of a fresh set of nodes and edges for the given [thinker].
		/// </summary>
		public MoronIONode createResolvedGraph(MoronThinker thinker)
		{
			MoronIONode[] resolvedNodes;
			MoronIONode node;

			resolvedNodes = new MoronIONode[nodes.Count];

			for(int i = 0; i < nodes.Count; i++)
			{
				node = (MoronIONode)Activator.CreateInstance(nodes[i].GetType());
				node.thinker = thinker;
				node.guid = nodes[i].guid;
				node.interruptsIntent = nodes[i].interruptsIntent;
				node.setJoistsTo(nodes[i]);

				resolvedNodes[i] = node;
			}

			foreach(Pair<int, int> edge in edges)
				resolvedNodes[edge.key].addEdge(resolvedNodes[edge.value]);

			for(int i = 0; i < resolvedNodes.Length; i++)
				resolvedNodes[i].init();

			return resolvedNodes[0];
		}

		public void OnBeforeSerialize()
		{
			serialized = serialize();
		}

		public void OnAfterDeserialize()
		{
			nodes.Clear();
			edges.Clear();

			deserialize(serialized);
		}
	}
}