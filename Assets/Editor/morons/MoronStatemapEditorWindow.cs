using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public class MoronStatemapEditorWindow : EditorWindow
	{
		protected static readonly Color INSPECTOR_BACKGROUND_COLOR = new Color(.76f, .76f, .76f);
		protected const float NAME_WIDTH = 90f;

		protected Dictionary<String, Type> definitionTypes;
		protected String[] definitionTypeNames;
		protected String definitionName;
		protected int selectedDefinitionIndex;

		protected MoronEditorWindow primaryEditor;

		[MenuItem("Window/Moron statemap")]
		public static void Init()
		{
			EditorWindow.GetWindow<MoronStatemapEditorWindow>().Show();
		}
		
		public MoronStatemapEditorWindow()
		{
			this.titleContent = new GUIContent("Moron statemap");
			this.autoRepaintOnSceneChange = false;
			this.wantsMouseMove = false;
			this.minSize = new Vector2(255, 300);

			definitionTypes = new Dictionary<String, Type>
			{
				{"int", typeof(int)},
				{"float", typeof(float)},
				{"string", typeof(String)},
				{"Vector3", typeof(Vector3)},
				{"GameObject", typeof(GameObject)}
			};
			definitionTypeNames = definitionTypes.Keys.ToArray();
		}

		public void OnEnable()
		{
			primaryEditor = EditorWindow.GetWindow<MoronEditorWindow>();
		}

		public void OnGUI()
		{
			clear(INSPECTOR_BACKGROUND_COLOR);
			
			if(primaryEditor.getGraph() == null)
				return;

			GUI.color = Color.black;
			GUI.DrawTexture(new Rect(position.x, position.y, 1f, position.height), EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;
			
			drawStatemap();
			drawStaticControls();
		}

		protected void drawStatemap()
		{
			MoronGraph graph;

			graph = primaryEditor.getGraph();
			
			foreach(StatemapField field in graph.statemapDefinition.definitions)
				field.value = EditorPropertyHelper.propertyField(field.name, NAME_WIDTH, field.getType(), field.value, position.width - 80f);
		}

		protected void drawStaticControls()
		{
			EditorGUILayout.BeginHorizontal();

			definitionName = EditorGUILayout.TextField(definitionName, GUILayout.Width(NAME_WIDTH));
			selectedDefinitionIndex = EditorGUILayout.Popup(selectedDefinitionIndex, definitionTypeNames, GUILayout.Width(80f));

			if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(15)))
				createDefinition();

			EditorGUILayout.EndHorizontal();
		}

		protected void createDefinition()
		{
			Type t;
			System.Object value;

			t = definitionTypes[definitionTypeNames[selectedDefinitionIndex]];
			
			if(t.IsValueType)
				value = (System.Object)Activator.CreateInstance(t);
			else
				value = null;

			primaryEditor.getGraph().statemapDefinition.set(definitionName, t, value);
		}

		protected void clear(Color clearColor)
		{
			GUI.color = clearColor;
			GUI.DrawTexture(new Rect(0f, 0f, position.width, position.height), EditorGUIUtility.whiteTexture);
		}
	}
}