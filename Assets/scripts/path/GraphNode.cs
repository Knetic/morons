using System;
using UnityEngine;
using System.Collections.Generic;

namespace Gridsnap.Morons
{
	public class GraphNode
	{	
	    public System.Object representative;
	
	    public Vector3 position;
	    public List<GraphEdge> edges;
	    
	    public GraphNode(Vector3 position)
	    {
	        this.position = position;
	        edges = new List<GraphEdge>();
	    }
	
	    public void addBidirectionalEdge(GraphNode node)
	    {
	        addBidirectionalEdge(node, Vector3.Distance(position, node.position));
	    }
	
	    public void addBidirectionalEdge(GraphNode node, float cost)
	    {
	        addEdge(node, cost);
	        node.addEdge(this, cost);
	    }
	
	    public void addEdge(GraphNode node)
	    {
	        addEdge(node, Vector3.Distance(position, node.position));
	    }
	
	    public virtual void addEdge(GraphNode node, float cost)
	    {
	        edges.Add(new GraphEdge(this, node, cost));
	    }
	
	    /// <summary>
	    ///     If this node contains an edge to the given [other] node,
	    ///     this returns the edge to it.
	    ///     Otherwise, null is returned.
	    /// </summary>
	    public GraphEdge getEdgeTo(GraphNode other)
	    {
	        foreach (GraphEdge edge in edges)
	            if (edge.to == other)
	                return edge;
	        return null;
	    }
		
		public GraphEdge removeEdgeTo(GraphNode other)
		{
			GraphEdge edge;
			
			edge = getEdgeTo(other);
			if(edge != null)
				edges.Remove(edge);
	        return edge;
		}
	
	    public T getRepresentative<T>() where T : class
	    {
	        return representative as T;
	    }
	}
}
