using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public class MoronAssetMenuItem
	{
		[MenuItem("Assets/Create/Moron Graph")]
		private static void createGraph()
		{
			String path;

			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			path = AssetDatabase.GenerateUniqueAssetPath(path + "/New Moron.asset");

			AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(typeof(MoronGraph)), path);
		}
	}
}