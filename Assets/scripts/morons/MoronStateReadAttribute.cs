using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Marks a variable which can be read from the Moron's statemap before an IONode starts evaluating.
	/// </summary>
	public class MoronStateReadAttribute : Attribute
	{}
}