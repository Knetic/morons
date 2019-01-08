using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Calculates a position that is offset from a gameobject by a set amount.
	///		Runs for one frame (calculating the offset) and ends.
	/// </summary>
	[MoronPathAttribute("math/")]
	public class RandomVectorIntent : MoronIntent
	{
		[MoronManualWriteAttribute]
		public Vector3 min;

		[MoronManualWriteAttribute]
		public Vector3 max;

		[MoronStateWriteAttribute]
		public Vector3 result;

		protected override bool evaluate()
		{
			result.x = UnityEngine.Random.Range(min.x, max.x);
			result.y = UnityEngine.Random.Range(min.y, max.y);
			result.z = UnityEngine.Random.Range(min.z, max.z);
			return true;
		}
	}
}