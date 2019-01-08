using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Calculates a position that is offset from a vector by a set amount.
	///		Runs for one frame (calculating the offset) and ends.
	/// </summary>
	[MoronPathAttribute("math/")]
	public class OffsetVectorIntent : MoronIntent
	{
		[MoronManualWriteAttribute]
		public Vector3 offset;

		[MoronStateReadAttribute]
		public Vector3 original;

		[MoronStateWriteAttribute]
		public Vector3 result;

		protected override bool evaluate()
		{
			result = original + offset;		
			return true;
		}
	}
}