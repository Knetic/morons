using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridsnap.Morons
{
	/// <summary>
	///		Marks a variable which can be written to the Moron's statemap after an IONode is done evaluating.
	/// </summary>
	public class MoronStateWriteAttribute : Attribute
	{}
}