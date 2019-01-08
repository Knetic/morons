using System;
using System.Collections;
using UnityEngine;

namespace Gridsnap.Morons
{
	public class SwitchTimerIntent : MoronIntent
	{
		[MoronManualWriteAttribute]
		public float time;

		[MoronStateWriteAttribute]
		public bool target;

		protected Coroutine current;

		protected override bool evaluate()
		{
			if(current != null)
				thinker.StopCoroutine(current);

			target = false;
			current = thinker.StartCoroutine(startTimer(time)); 
			
			return true;
		}

		protected IEnumerator startTimer(float length)
		{
			yield return new WaitForSeconds(length);
			current = null;
			setStatemapValue("target", true);
		}
	}
}