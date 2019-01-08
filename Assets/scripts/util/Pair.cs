using System;
using UnityEngine;

namespace Gridsnap.Morons
{
	[Serializable]
	public struct Pair<A, B>
	{
		[SerializeField]
		public A key;

		[SerializeField]
		public B value;

		public Pair(A key, B value)
		{
			this.key = key;
			this.value = value;
		}

		public override String ToString()
		{
			return key.ToString() + ":" + value.ToString();
		}
	}
}