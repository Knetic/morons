using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Calculates a position that is offset from a gameobject by a set amount.
	///		Runs for one frame (calculating the offset) and ends.
	/// </summary>
	[MoronPathAttribute("math/")]
	public class OffsetObjectIntent : MoronIntent
	{
		[MoronManualWriteAttribute]
		public Vector3 offset;

		[MoronManualWriteAttribute]
		public bool relative;

		[MoronStateReadAttribute]
		public GameObject center;

		[MoronStateWriteAttribute]
		public Vector3 result;

		protected override bool evaluate()
		{
			Vector3 difference;

			if(relative)
			{
				difference = (thinker.gameObject.transform.position - center.transform.position).normalized;
				result = VectorHandling.multiply(difference, offset) + center.transform.position;
			}
			else
				result = center.transform.position + offset;
			
			return true;
		}
	}
}