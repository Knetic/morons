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
			IdentifiedGameObject stored, identified;
			GameObject obj;

			EditorGUILayout.BeginHorizontal();

			if(type == null)
				throw new ArgumentNullException("No type specified");

			try
			{
				EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
				
				if(type == typeof(bool))
					return EditorGUILayout.Toggle((bool)value, GUILayout.Width(width));
				if(type == typeof(int))
					return EditorGUILayout.IntField((int)value, GUILayout.Width(width));
				if(type == typeof(float))
					return EditorGUILayout.FloatField((float)value, GUILayout.Width(width));
				if(type == typeof(String))
					return EditorGUILayout.TextField((String)value, GUILayout.Width(width));
				if(type == typeof(Vector3))
					return EditorGUILayout.Vector3Field("", (Vector3)value, GUILayout.Width(width));
				if(type == typeof(IdentifiedGameObject))
				{
					stored = (IdentifiedGameObject)value;					
						
					// empty (never-assigned) guid?
					if(stored == null || String.IsNullOrEmpty(stored.guid))
					{
						obj = (GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), true, GUILayout.Width(width));
						if(stored == null && obj != null)
							return new IdentifiedGameObject(null, obj);

						if(stored != null && obj != stored.gameObject)
							return new IdentifiedGameObject(null, obj);
						
						return stored;
					}

					identified = StatemapDefinition.getHeldObject(stored.guid);					
					stored.gameObject = (GameObject)EditorGUILayout.ObjectField((GameObject)identified.gameObject, typeof(GameObject), true, GUILayout.Width(width));
					return stored;
				}				
				throw new ArgumentException("Cannot support fields of type " + type.Name);
			}
			finally
			{
				EditorGUILayout.EndHorizontal();
			}
		}

		public static void readonlyField(String label, float labelWidth)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
			EditorGUILayout.EndHorizontal();
		}
	}
}