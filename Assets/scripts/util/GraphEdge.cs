using UnityEngine;
using System.Collections;

namespace Gridsnap.Morons
{
	public class GraphEdge
	{
	    public GraphNode to, from;
	    public float cost;
	
	    public GraphEdge(GraphNode from, GraphNode to) : this(from, to, 1f)
	    {}
	
	    public GraphEdge(GraphNode from, GraphNode to, float cost)
	    {
	        this.to = to;
	        this.from = from;
	        this.cost = cost;
	    }
	
	    public bool equals(GraphEdge other)
	    {
	        return to == other.to && from == other.from;
	    }
	}
}
