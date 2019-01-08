using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Denotes a path in the editor for which an IONode should appear.
	/// </summary>
	public class MoronPathAttribute : Attribute
	{
		public String path;
		
		public MoronPathAttribute(String path)
		{
			this.path = path;
		}
	}
}