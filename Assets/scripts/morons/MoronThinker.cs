using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Controls an NPC using the Moron stateful-AI framework.
	/// </summary>
	public class MoronThinker : MonoBehaviour
	{
		public MoronGraph logic;
		public Dictionary<String, System.Object> statemap;

		protected MoronIONode currentNode;

		public void Start()
		{
			if(logic == null)
			{
				Destroy(this);
				Debug.LogError("MoronThinker had no logic attached.");
				return;
			}

			statemap = logic.createStatemap();
			setCurrentNode(logic.createResolvedGraph(this));
		}

		public void Update()
		{
			GraphEdge transition;

			transition = currentNode.update();
			if(transition == null)
				return;
			
			setCurrentNode((MoronIONode)transition.to);
		}

		public MoronIONode getCurrentNode()
		{
			return currentNode;
		}

		protected void setCurrentNode(MoronIONode node)
		{
			if(currentNode != null)
			{
				currentNode.deactivate();
				notifyNearby(currentNode, false);
			}

			currentNode = node;
			notifyNearby(currentNode, true);
			currentNode.activate();
		}

		protected void notifyNearby(MoronIONode node, bool active)
		{
			MoronChoice choice;

			foreach(GraphEdge edge in node.edges)
			{
				choice = edge.to as MoronChoice;
				if(choice == null || !choice.interruptsIntent)
					continue;

				if(active)
					choice.notifyNearby();
				else
					choice.notifyNotNearby();
			}
		}
	}
}