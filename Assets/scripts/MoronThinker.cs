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
		protected Dictionary<String, System.Object> statemap;
		
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
			statemap["self"] = gameObject;
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

		public System.Object getStateValue(String key)
		{
			IdentifiedGameObject identified;
			System.Object obj;

			if(!statemap.ContainsKey(key))
				return null;
			
			obj = statemap[key];
			identified = obj as IdentifiedGameObject;

			if(identified != null)
				return identified.gameObject;
			
			return obj;
		}

		public void setStateValue(String key, System.Object value)
		{
			statemap[key] = value;

			// any nodes in the current neighborhood who read that value should have their statemaps updated.
			// this includes choices that may be more than just in the immediate vicinity.
			recurseReadUpdates(currentNode);
		}

		protected void recurseReadUpdates(MoronIONode node)
		{
			MoronChoice choice;

			node.updateReadFields();
			
			foreach(GraphEdge edge in node.edges)
			{
				choice = edge.to as MoronChoice;
				if(choice != null)
				{
					recurseReadUpdates(choice);
					continue;
				}

				((MoronIONode)edge.to).updateReadFields();
			}
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