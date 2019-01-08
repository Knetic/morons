using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Adds two vectors. Saves result.
	/// </summary>
	[MoronPathAttribute("math/")]
	public class AddVectorIntent : MoronIntent
	{
		[MoronStateReadAttribute]
		public Vector3 offset;

		[MoronStateReadAttribute]
		public Vector3 original;

		[MoronManualWriteAttribute]
		public bool subtract;

		[MoronStateWriteAttribute]
		public Vector3 result;

		protected override bool evaluate()
		{
			if(subtract)
				result = original - offset;
			else
				result = original + offset;
			
			return true;
		}
	}
}