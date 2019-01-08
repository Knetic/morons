using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	public class WhenBoolChoice : MoronChoice
	{
		[MoronStateReadAttribute]
		public bool val;

		[MoronManualWriteAttribute]
		public bool not;

		protected override bool evaluate()
		{
			return val != not;
		}
	}
}