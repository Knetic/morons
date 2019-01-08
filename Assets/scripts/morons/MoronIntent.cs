using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		A MoronThinker holds exactly one Intent at any given time, and it handles what the Moron should be doing until it changes its mind.
	///		Intents are atomic, they never reference other Intents, and they are the "leaf nodes" in the Moron thought process.
	///		Examples include idling for a specified time period, performing an attack, walking to a point, taking an item from the ground, etc.
	///		Similar to a Behavior in IAI.
	/// </summary>
	public abstract class MoronIntent : MoronIONode
	{
		protected abstract bool evaluate();

		public sealed override GraphEdge update()
		{
			GraphEdge transition;

			// if any choice wants to interrupt us, return before we do anything.
			transition = getTransitionForInterruptors(true);
			if(transition != null)
				return transition;

			// otherwise, evaluate and see if we're done.
			if(!evaluate())
				return null;

			// we're done. Go through all non-interrupting choices and find one that fits.
			transition = getTransitionForInterruptors(false);
			if(transition != null)
				return transition;

			// no choices indicate they want us to do anything, find an edge to another intent.
			transition = 
				edges.Where(e => e.to is MoronIntent)
				.FirstOrDefault();
			
			if(transition != null)
				return transition;

			// nothing to transition out of, restart this.
			activate();
			return null;
		}

		// "nearby" isn't a concept that intents care about.
		public sealed override void notifyNearby()
		{}
		public sealed override void notifyNotNearby()
		{}

		protected GraphEdge getTransitionForInterruptors(bool areInterruptor)
		{
			GraphEdge transition;

			foreach(MoronChoice choice in getInterruptors(true))
			{
				transition = choice.update();
				if(transition != null)
					return transition;
			}

			return null;
		}

		protected IEnumerable<MoronChoice> getInterruptors(bool areInterruptor)
		{
			return 
				getNeighbors<MoronChoice>()
				.Where(c => c.interruptsIntent == areInterruptor);
		}
	}
}