using System;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	[CustomEditor(typeof(MoronGraph))]
	public class MoronGraphEditor : Editor
	{
		/*protected static Texture2D ICON;

		public override Texture2D RenderStaticPreview(String assetPath, UnityEngine.Object[] subAssets, int width, int height)
		{
			if(ICON == null)
				ICON = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/morons/icon.png", typeof(Texture2D));			
			
			return ICON;
		}*/
	}
}