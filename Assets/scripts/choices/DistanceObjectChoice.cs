using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	public class DistanceObjectChoice : MoronChoice
	{
		[MoronStateReadAttribute]
		public GameObject target;

		[MoronManualWriteAttribute]
		public float threshold;

		[MoronManualWriteAttribute]
		public bool greater;

		protected override bool evaluate()
		{
			float distance;

			distance = Vector3.Distance(target.transform.position, thinker.gameObject.transform.position);
			return (distance > threshold) == greater;
		}
	}
}