using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public abstract class EditorPropertyHelper
	{
		public static System.Object propertyField(Rect position, Rect labelPosition, String label, Type type, System.Object value)
		{
			EditorGUILayout.BeginHorizontal();

			if(type == null)
				throw new ArgumentNullException("No type specified");
				
			try
			{
				EditorGUI.LabelField(labelPosition, label);
				
				if(type == typeof(bool))
					return EditorGUI.Toggle(position, (bool)value);
				if(type == typeof(int))
					return EditorGUI.IntField(position, (int)value);
				if(type == typeof(float))
					return EditorGUI.FloatField(position, (float)value);
				if(type == typeof(String))
					return EditorGUI.TextField(position, (String)value);
				if(type == typeof(Vector3))
					return EditorGUI.Vector3Field(position, "", (Vector3)value);
				if(type == typeof(GameObject))
					return EditorGUI.ObjectField(position, (UnityEngine.GameObject)value, type, true);
				
				throw new ArgumentException("Cannot support fields of type " + type.Name);
			}
			finally
			{
				EditorGUILayout.EndHorizontal();
			}
		}

		public static System.Object propertyField(String label, float labelWidth, Type type, System.Object value, float width)
		{
			EditorGUILayout.BeginHorizontal();

			if(type == null)
				throw new ArgumentNullException("No type specified");

			try
			{
				EditorGUILayout.LabelField(label, GUILayout.MaxWidth(labelWidth));
				
				if(type == typeof(bool))
					return EditorGUILayout.Toggle((bool)value);
				if(type == typeof(int))
					return EditorGUILayout.IntField((int)value);
				if(type == typeof(float))
					return EditorGUILayout.FloatField((float)value);
				if(type == typeof(String))
					return EditorGUILayout.TextField((String)value);
				if(type == typeof(Vector3))
					return EditorGUILayout.Vector3Field("", (Vector3)value, GUILayout.MaxWidth(width));
				if(type == typeof(GameObject))
					return EditorGUILayout.ObjectField((UnityEngine.GameObject)value, type, true);
				
				throw new ArgumentException("Cannot support fields of type " + type.Name);
			}
			finally
			{
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}