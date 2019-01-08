using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		A Choice represents a binary condition which splits the flow of logic during Moron evaluation.
	///		Choices have a boolean-satisfiable condition and exactly two edges - one for each possible outcome of the condition.
	///		Choices are also capable of writing to the statemap.
	/// </summary>
	public abstract class MoronChoice : MoronIONode
	{
		protected GraphEdge trueEdge, falseEdge;
		
		protected abstract bool evaluate();

		public override void init()
		{
			base.init();
			
			if(edges.Count <= 0 || edges.Count > 2)
				throw new Exception("MoronChoice must contain either one or two edges, never more or less");

			trueEdge = edges[0];
			if(edges.Count == 2)
				falseEdge = edges[1];
		}

		public sealed override GraphEdge update()
		{
			if(evaluate())
				return trueEdge;
			return falseEdge;
		}
	}
}