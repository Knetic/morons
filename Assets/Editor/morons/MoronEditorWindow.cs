using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public partial class MoronEditorWindow : EditorWindow
	{
		protected static readonly Color INTENT_COLOR = new Color(.9f, .38f, 0f);
		protected static readonly Color CHOICE_COLOR = new Color(0f, .38f, .9f);
		protected static readonly Color INTENT_ROOT_COLOR = new Color(.23f, .77f, .13f);

		protected static readonly Color YES_TRANSITION_COLOR = new Color(.28f, 1f, .42f);
		protected static readonly Color NO_TRANSITION_COLOR = new Color(1f, .28f, .28f);
		protected static readonly Color NEXT_TRANSITION_COLOR = new Color(1f, .48f, .28f);
		protected static readonly Color SELECTED_TRANSITION_COLOR = new Color(.83f, .28f, 1f);

		protected static readonly Color NODE_BACKGROUND_COLOR = Color.white;
		protected static readonly Color INSPECTOR_BACKGROUND_COLOR = new Color(.76f, .76f, .76f);
		
		protected const int NODE_FLASHTIME = 300;
		protected const float NAME_WIDTH = 90f;

		// statemap stuff
		protected Dictionary<String, Type> definitionTypes;
		protected String[] definitionTypeNames;
		protected String definitionName;
		protected int selectedDefinitionIndex;

		protected float statemapInspectorWidth = 225f;

		// currently edited graph
		protected static MoronGraph graph;
		
		// ingame current selected gameObject guid map
		Dictionary<String, MoronIONode> guidMap;

		[MenuItem("Window/Morons")]
		public static void Init()
		{
			EditorWindow.GetWindow<MoronEditorWindow>().Show();
		}
		
		public MoronEditorWindow()
		{
			this.titleContent = new GUIContent("Morons");
			this.autoRepaintOnSceneChange = false;
			this.wantsMouseMove = true;
			
			definitionTypes = new Dictionary<String, Type>
			{
				{"bool", typeof(bool)},
				{"int", typeof(int)},
				{"float", typeof(float)},
				{"string", typeof(String)},
				{"Vector3", typeof(Vector3)},
				{"GameObject", typeof(GameObject)}
			};
			definitionTypeNames = definitionTypes.Keys.ToArray();	
		}

		[UnityEditor.Callbacks.OnOpenAsset(1)]
		public static bool OnOpenAsset(int instanceID, int line)
		{
			MoronGraph graph;
			String path;

			path = AssetDatabase.GetAssetPath(instanceID);

			graph = AssetDatabase.LoadAssetAtPath<MoronGraph>(path);
			if (graph == null) 
				return false;

			MoronEditorWindow.graph = graph;

			EditorWindow.GetWindow<MoronEditorWindow>().Show();
			return true;
		}

		public void OnSelectionChange()
		{
			MoronThinker thinker;

			if(!Application.isPlaying)
				return;
			
			if(Selection.activeGameObject == null)
				return;
			
			thinker = Selection.activeGameObject.GetComponent<MoronThinker>();
			if(thinker == null)
				return;
			
			graph = thinker.logic;
			buildGuidMap(thinker);
			Repaint();
		}

		public void OnEnable()
		{
			reset();
		}

		public MoronGraph getGraph()
		{
			return graph;
		}

		public void OnGUI()
		{
			if(graph == null)
				return;

			updateInput(Event.current);
			
			switch(Event.current.type)
			{
				case EventType.MouseDrag:
				case EventType.MouseMove:
					if(!m0)
						return;
					break;
				
				// TODO: patched-redraw based on focused control and keyboard event
			}

			redraw();
		}

		public void OnInspectorUpdate() 
		{
			Repaint();
		}

		protected void redraw()
		{
			Rect realPosition;
			clear(Color.gray);

			realPosition = new Rect(0f, 0f, position.width, position.height);

			// draw main area
			scrollPosition = GUI.BeginScrollView(realPosition, scrollPosition, findMaxArea());
			
			if(transitionLine && selected != null)
				drawTransitionLine();
			
			foreach(Pair<int, int> edge in graph.edges)
				drawTransitionEdge(edge);

			foreach(MoronIONode node in graph.nodes)
				drawNode(node);

			GUI.EndScrollView();

			// draw statemap definitions
			GUI.BeginGroup(new Rect(0f, 0f, statemapInspectorWidth, realPosition.height));

			clear(INSPECTOR_BACKGROUND_COLOR);
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;

			drawStatemap();
			drawStaticStatemapControls();
			drawNodeComments();

			GUI.EndGroup();
		}

		protected void clear(Color clearColor)
		{
			GUI.color = clearColor;
			GUI.DrawTexture(new Rect(0f, 0f, position.width, position.height), EditorGUIUtility.whiteTexture);
		}

		protected void drawNode(MoronIONode node)
		{
			Rect nodeRect;

			nodeRect = makeNodeRect(node);

			drawBody(node, nodeRect);
			drawTitleLabel(node, nodeRect);
			drawFields(node, nodeRect);
		}

		protected void drawBody(MoronIONode node, Rect nodeRect)
		{
			MoronThinker thinker;
			MoronIONode thinkerNode;
			TimeSpan age;
			Rect borderRect;
			Color desiredColor;

			// border
			borderRect = nodeRect;
			borderRect.x -= 1;
			borderRect.y -= 1;
			borderRect.width += 2;
			borderRect.height += 2;

			if(node == selected)
				GUI.color = Color.yellow;
			else
				GUI.color = Color.black;

			GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture);

			// body
			if(node is MoronIntent)
			{
				if(node == graph.nodes[0])
					desiredColor = INTENT_ROOT_COLOR;
				else
					desiredColor = INTENT_COLOR;
			}
			else
				desiredColor = CHOICE_COLOR;

			if(Application.isPlaying && Selection.gameObjects.Length > 0)
			{
				thinker = Selection.gameObjects[0].GetComponent<MoronThinker>();
				if(thinker != null)
				{
					if(thinker.getCurrentNode().guid == node.guid)
						desiredColor = Color.white;
					else
					{
						if(guidMap.ContainsKey(node.guid))
						{
							thinkerNode = guidMap[node.guid];

							age = DateTime.Now.AddMilliseconds(-NODE_FLASHTIME).Subtract(thinkerNode.lastActiveTime);
							if(age.TotalMilliseconds < NODE_FLASHTIME)
								desiredColor = Color.LerpUnclamped(Color.white, desiredColor, (float)age.TotalMilliseconds / (float)NODE_FLASHTIME);
						}
					}
				} 
			}

			GUI.color = desiredColor;
			GUI.DrawTexture(nodeRect, EditorGUIUtility.whiteTexture);
		}

		protected void drawTitleLabel(MoronIONode node, Rect nodeRect)
		{			
			String name;
			Rect labelRect;

			name = findNodeTypeName(node.GetType());

			labelRect = nodeRect;
			labelRect.x += labelRect.width / 2;
			labelRect.x -= GUI.skin.label.CalcSize(new GUIContent(name)).x / 2;
			labelRect.xMax = nodeRect.xMax;
			labelRect.height = 15;
			
			GUI.color = Color.white;
			GUI.Label(labelRect, name, GUIStyle.none);

			labelRect.xMin = nodeRect.xMin;
			labelRect.y += labelRect.height;
			labelRect.height = 1;
			GUI.color = Color.black;
			GUI.DrawTexture(labelRect, EditorGUIUtility.whiteTexture);
		}

		protected void drawFields(MoronIONode node, Rect nodeRect)
		{
			System.Object value, original;
			Rect current, label;
			float labelWidth;

			current = new Rect(nodeRect.x, nodeRect.y + 20, nodeRect.width, 20);
			label = current;

			GUI.color = NODE_BACKGROUND_COLOR;
			GUI.contentColor = NODE_BACKGROUND_COLOR;
			GUI.backgroundColor = NODE_BACKGROUND_COLOR;
			
			// manual
			foreach(FieldInfo field in node.getFieldsWithAttribute<MoronManualWriteAttribute>())
			{
				label.y = current.y;
				label.width = GUI.skin.label.CalcSize(new GUIContent(field.Name)).x;
				
				labelWidth = label.width + 5;
				current.x = nodeRect.x + labelWidth;
				current.width = nodeRect.width - labelWidth - 2;
				
				original = node.getManualJoist(field.Name);
				value = EditorPropertyHelper.propertyField(current, label, field.Name, field.FieldType, original);
				node.setManualJoist(field.Name, value);
				
				if(value != original)
					EditorUtility.SetDirty(graph);

				current.y += 20;
			}

			drawJoistList<MoronStateReadAttribute>(node, nodeRect, ref current, ref label, node.getReadJoist, node.setReadJoist, "    Reads:");
			drawJoistList<MoronStateWriteAttribute>(node, nodeRect, ref current, ref label, node.getWriteJoist, node.setWriteJoist, "    Writes:");
		}

		protected void drawJoistList<T>(MoronIONode node, Rect nodeRect, ref Rect current, ref Rect label, Func<String, String> getCall, Action<String, String> setCall, String labelText) where T: Attribute
		{
			IEnumerable<FieldInfo> fields;
			String[] matchingArray;
			float labelWidth;
			int index, original, adj;

			fields = node.getFieldsWithAttribute<T>();
			if(fields.Count() <= 0)
				return;

			label.y = current.y;
			label.width = GUI.skin.label.CalcSize(new GUIContent(labelText)).x;
			current.y += 20;

			EditorGUI.LabelField(label, labelText);

			// reads
			foreach(FieldInfo field in fields)
			{
				label.y = current.y;
				label.width = GUI.skin.label.CalcSize(new GUIContent(field.Name)).x;
				
				labelWidth = label.width + 5;
				current.x = nodeRect.x + labelWidth;
				current.width = nodeRect.width - labelWidth - 2;
				
				EditorGUI.LabelField(label, field.Name);

				matchingArray = graph.statemapDefinition.getFieldsForType(field.FieldType).Select(k => k.name).ToArray();
				original = Array.IndexOf(matchingArray, getCall(field.Name));

				adj = original;
				if(original < 0)
					adj = 0;

				index = EditorGUI.Popup(current, adj, matchingArray);
				
				if(index >= 0 && index < matchingArray.Length && index != original)
				{
					setCall(field.Name, matchingArray[index]);
					EditorUtility.SetDirty(graph);
				}
				
				current.y += 20;
			}
		}

		protected void drawTransitionEdge(Pair<int, int> indices)
		{			
			Rect toRect;
			Vector2 from, to;
			Vector2 arrowPosition;
			bool fromIntent, toIntent;
			int metaIndex;

			metaIndex = graph.edges.IndexOf(indices);
			fromIntent = graph.nodes[indices.key] is MoronIntent;
			toIntent = graph.nodes[indices.value] is MoronIntent;

			Handles.color = Color.white;
			
			// if it's intent-to-intent, this is a default "next" transition and should be called out that way.
			if(fromIntent && toIntent)
				Handles.color = NEXT_TRANSITION_COLOR;
						
			if(!fromIntent)
			{
				// "yes" choices are green, "no" choices are red.
				// if this index is less than all other neighbors, it's a "yes". Otherwise it's a "no".
				Handles.color = YES_TRANSITION_COLOR;
				
				foreach(int index in findNeighborsFrom(indices.key))
					if(metaIndex > index)
					{
						Handles.color = NO_TRANSITION_COLOR;
						break;
					}
			}

			// finally, if this is the selected edge, color it purple.
			if(selectedEdge >= 0 && indices.Equals(graph.edges[selectedEdge]))
				Handles.color = SELECTED_TRANSITION_COLOR;

			from = graph.nodes[indices.key].position;
			to = graph.nodes[indices.value].position;
			
			toRect = makeNodeRect(graph.nodes[indices.value]);
			arrowPosition = VectorHandling.findPointAroundRect(from, toRect);

			Handles.DrawLine(from, to);
			drawArrow(arrowPosition, Quaternion.Euler(0f, 0f, VectorHandling.angleBetween(from, to)), 5, 12);
		}

		protected void drawTransitionLine()
		{
			GUI.color = Color.white;
			Handles.DrawLine(selected.position, Event.current.mousePosition);
		}

		protected void drawArrow(Vector2 position, Quaternion rotation, float width, float length)
		{
			Vector2 a, b, c;

			a = (Vector2)(rotation * new Vector2(-width, -length)) + position;
			b = (Vector2)(rotation * new Vector2(width, -length)) + position;
			c =  position;

			Handles.DrawAAConvexPolygon(a, b, c);
		}

		protected void drawStatemap()
		{
			System.Object value;

			foreach(StatemapField field in graph.statemapDefinition.definitions)
			{
				if(!field.editable)
				{
					EditorPropertyHelper.readonlyField(field.name, NAME_WIDTH);
					continue;
				}

				value = EditorPropertyHelper.propertyField(field.name, NAME_WIDTH, field.type, field.value, statemapInspectorWidth - NAME_WIDTH - 10f);

				if(!System.Object.Equals(value, field.value))
				{
					field.set(field.type, value);
					EditorUtility.SetDirty(graph);
				}
			}
		}

		protected void drawStaticStatemapControls()
		{
			EditorGUILayout.BeginHorizontal();

			definitionName = EditorGUILayout.TextField(definitionName, GUILayout.Width(NAME_WIDTH));
			selectedDefinitionIndex = EditorGUILayout.Popup(selectedDefinitionIndex, definitionTypeNames, GUILayout.Width(NAME_WIDTH));

			if(GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(20), GUILayout.Height(15)))
				createDefinition();

			EditorGUILayout.EndHorizontal();
		}

		protected void drawNodeComments()
		{
			if(selected == null)
				return;
			
			try
			{
				EditorGUILayout.LabelField("Comments");
				selected.comment = EditorGUILayout.TextArea(selected.comment, GUILayout.Width(statemapInspectorWidth - 10), GUILayout.MinHeight(50));
			}
			catch(ArgumentException e)
			{
				// suppress. I know it's hacky.
			}
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

			graph.statemapDefinition.set(definitionName, t, value);
			EditorUtility.SetDirty(graph);
		}

		protected void startTransition(System.Object userdata)
		{
			transitionLine = true;
			transitionReplacement = (MoronTransitionCondition)userdata;
		}

		protected void deleteTransition(System.Object userdata)
		{
			graph.edges.RemoveAt(selectedEdge);
			reset();
			EditorUtility.SetDirty(graph);
		}

		protected void toggleInterrupting(System.Object userdata)
		{
			selected.interruptsIntent = !selected.interruptsIntent;
			EditorUtility.SetDirty(graph);
		}

		protected void setRootIntent(System.Object userdata)
		{
			Pair<int, int> edge;
			int nodeIndex;

			// swap
			nodeIndex = graph.nodes.IndexOf(selected);
			graph.nodes[nodeIndex] = graph.nodes[0];
			graph.nodes[0] = selected;

			// modify edges that referenced either one
			for(int i = 0; i < graph.edges.Count; i++)
			{
				edge = graph.edges[i];

				if(edge.key == nodeIndex)
					edge.key = 0;
				else
					if(edge.key == 0)
						edge.key = nodeIndex;

				if(edge.value == nodeIndex)
					edge.value = 0;
				else
					if(edge.value == 0)
						edge.value = nodeIndex;
				
				graph.edges[i] = edge;
			}

			EditorUtility.SetDirty(graph);
		}

		protected void deleteNode(System.Object userdata)
		{
			Pair<int, int> modifiedPair, originalPair;
			int index, from, to;

			index = graph.nodes.IndexOf(selected);
			graph.nodes.Remove(selected);

			// make sure edge indices are properly updated/removed
			graph.edges.RemoveAll(e => e.key == index || e.value == index);

			for(int i = 0; i < graph.edges.Count; i++)
			{
				originalPair = graph.edges[i];
				
				from = originalPair.key;
				if(from > index)
					from--;
				to = originalPair.value;
				if(to > index)
					to--;
				
				modifiedPair = new Pair<int, int>(from, to);
				graph.edges[i] = modifiedPair;
			}

			EditorUtility.SetDirty(graph);
		}

		protected MoronIONode findNodeForPosition(Vector2 position)
		{
			foreach(MoronIONode node in graph.nodes)
				if(makeNodeRect(node).Contains(position))
					return node;

			return null;
		}

		protected int findEdgeForPosition(Vector2 position)
		{
			Pair<int, int> edge;
			float distance;

			for(int i = 0; i < graph.edges.Count; i++)
			{
				edge = graph.edges[i];
				distance = VectorHandling.lineSegmentDistance(position, graph.nodes[edge.key].position, graph.nodes[edge.value].position);

				if(distance < 5)
					return i;
			}

			return -1;
		}

		protected bool checkStatemapWidth(float x)
		{
			return x > statemapInspectorWidth;
		}

		protected void rightClick()
		{
			if(selected == null)
			{				
				if(selectedEdge < 0)
				{
					showAddMenu();
					return;
				}

				showTransitionMenu();
				return;
			}
			
			populateNodeRightClick();
		}

		protected void showAddMenu()
		{
			GenericMenu menu;
			
			menu  = new GenericMenu();

			makeTypeCreators(typeof(MoronIntent), menu, "Create Intent/");
			makeTypeCreators(typeof(MoronChoice), menu, "Create Choice/");

			menu.ShowAsContext();
		}

		protected void showTransitionMenu()
		{
			GenericMenu menu;
			
			menu  = new GenericMenu();
			menu.AddItem(new GUIContent("Delete Transition"), false, deleteTransition, null);
			menu.ShowAsContext();
		}

		protected void populateNodeRightClick()
		{
			GenericMenu menu;
			
			menu  = new GenericMenu();

			if(selected is MoronIntent)
			{
				menu.AddItem(new GUIContent("Make Transition"), false, startTransition, MoronTransitionCondition.INTENT);

				if(graph.nodes[0] != selected)
					menu.AddItem(new GUIContent("Set as Root"), false, setRootIntent, null);
				else
					menu.AddDisabledItem(new GUIContent("Set as Root"));
			}

			if(selected is MoronChoice)
			{
				menu.AddItem(new GUIContent("Set 'yes' Transition"), false, startTransition, MoronTransitionCondition.YES);	

				if(findNeighborsFrom(graph.nodes.IndexOf(selected)).Count() > 0)
					menu.AddItem(new GUIContent("Set 'no' Transition"), false, startTransition, MoronTransitionCondition.NO);	
				else
					menu.AddDisabledItem(new GUIContent("Set 'no' Transition"));	

				if(((MoronChoice)selected).interruptsIntent)
					menu.AddItem(new GUIContent("Set as non-interrupting"), false, toggleInterrupting, null);
				else
					menu.AddItem(new GUIContent("Set as interrupting"), false, toggleInterrupting, null);
			}
			
			menu.AddItem(new GUIContent("Delete"), false, deleteNode, null);
			menu.ShowAsContext();
		}

		protected void makeTransition(MoronIONode from, MoronIONode to)
		{
			Pair<int, int> edge;
			int metaIndex, previousEdgeMetaIndex, count;

			if(from == to)
				return;

			metaIndex = graph.nodes.IndexOf(from);
			previousEdgeMetaIndex = -1;

			switch(transitionReplacement)
			{
				case MoronTransitionCondition.INTENT:
					
					// if both are intents, we make sure that [from] has no other intent edges - an intent can only have one other intent as an edge.
					if(from is MoronIntent && to is MoronIntent)
						graph.edges.RemoveAll(e => e.key == metaIndex && graph.nodes[e.value] is MoronIntent);
					break;
				
				case MoronTransitionCondition.YES:

					// remove the lowest-index transition, and insert into its old spot.
					for(int i = 0; i < graph.edges.Count; i++)
						if(graph.edges[i].key == metaIndex)
						{
							previousEdgeMetaIndex = i;
							break;
						}
					
					if(previousEdgeMetaIndex >= 0)
						graph.edges.RemoveAt(previousEdgeMetaIndex);
					break;

				case MoronTransitionCondition.NO:

					// remove the highest-index transition, and insert into its old spot.
					count = 0;

					for(int i = 0; i < graph.edges.Count; i++)
						if(graph.edges[i].key == metaIndex && ++count == 2)
						{
							previousEdgeMetaIndex = i;
							break;
						}
					
					if(previousEdgeMetaIndex >= 0)
						graph.edges.RemoveAt(previousEdgeMetaIndex);
					break;
			}

			edge = new Pair<int, int>(graph.nodes.IndexOf(from), graph.nodes.IndexOf(to));

			if(previousEdgeMetaIndex < 0)
				graph.edges.Add(edge);
			else
				graph.edges.Insert(previousEdgeMetaIndex, edge);

			EditorUtility.SetDirty(graph);
		}

		protected void deleteSelected()
		{
			EditorUtility.SetDirty(graph);

			if(selectedEdge >= 0)
			{
				deleteTransition(null);
				return;
			}

			if(selected != null)
			{
				deleteNode(null);
				return;
			}
		}

		protected Rect makeNodeRect(MoronIONode node)
		{
			IEnumerable<FieldInfo> attributes;
			float width, height;

			width =  180;
			height = 25;

			attributes = node.getFieldsWithAttribute<MoronManualWriteAttribute>();
			if(attributes.Count() > 0)
				height += attributes.Count() * 20f;

			attributes = node.getFieldsWithAttribute<MoronStateWriteAttribute>();
			if(attributes.Count() > 0)
				height += (attributes.Count() + 1) * 20f;

			attributes =  node.getFieldsWithAttribute<MoronStateReadAttribute>();
			if(attributes.Count() > 0)
				height += (attributes.Count() + 1) * 20f;			

			return new Rect(node.position.x - width / 2, node.position.y - height / 2, width, height);
		}

		protected void makeTypeCreators(Type t, GenericMenu menu, String prefix)
		{
			IEnumerable<Type> matchingTypes;
			MoronPathAttribute relativePath;
			List<Pair<Type, String>> paths;
			IEnumerable<Pair<Type, String>> orderedPaths;
			String name, path;

			paths = new List<Pair<Type, String>>();

			// create a set of all the types and paths we want
			matchingTypes = 
				AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => t.IsAssignableFrom(p) && p.IsClass && p != t)
				.OrderBy(ty => ty.Name);
			
			foreach(Type matched in matchingTypes)
			{
				name = findNodeTypeName(matched);

				path = prefix;

				relativePath = matched.GetCustomAttributes(true)
					.Where(a => a.GetType() == typeof(MoronPathAttribute))
					.Cast<MoronPathAttribute>()
					.FirstOrDefault();

				if(relativePath != null)
					path += relativePath.path;
				
				path += name;
				paths.Add(new Pair<Type, String>(matched, path));
			}

			// organize and create menu items
			orderedPaths = paths
				.OrderByDescending(p => p.value.Length - p.value.Replace("/", "").Length)
				.ThenBy(p => p.value);

			foreach(Pair<Type, String> pair in orderedPaths)
				menu.AddItem(new GUIContent(pair.value), false, (_) => createNode(pair.key), null);
		}

		protected void createNode(Type t)
		{
			MoronIONode node;

			node = (MoronIONode)Activator.CreateInstance(t);
			node.position = lastClicked + scrollPosition;
			graph.nodes.Add(node);

			EditorUtility.SetDirty(graph);
		}

		protected Rect findMaxArea()
		{
			Vector2 max;

			max = Vector2.zero;

			foreach(MoronIONode node in graph.nodes)
				max = VectorHandling.findAllMax(node.position, max);
			
			max = VectorHandling.add(max, 200f);
			return new Rect(0f, 0f, max.x, max.y);
		}

		protected String findNodeTypeName(Type t)
		{
			return 
				t.Name
				.Replace("Moron", "")
				.Replace("Intent", "")
				.Replace("Choice", "");
		}

		protected IEnumerable<int> findNeighborsFrom(int index)
		{
			for(int i = 0; i < graph.edges.Count; i++)
				if(graph.edges[i].key == index)
					yield return i;
		}

		protected void buildGuidMap(MoronThinker thinker)
		{
			List<MoronIONode> visited, horizon;
			MoronIONode node;

			horizon = new List<MoronIONode>();
			visited = new List<MoronIONode>();

			horizon.Add(thinker.getCurrentNode());
			guidMap = new Dictionary<String, MoronIONode>();

			while(horizon.Count > 0)
			{
				node = horizon[0];
				horizon.RemoveAt(0);
				visited.Add(node);

				guidMap[node.guid] = node;

				foreach(GraphEdge edge in node.edges)
					if(!visited.Contains((MoronIONode)edge.to))
						horizon.Add((MoronIONode)edge.to);
			}
		}

		protected void markDirty()
		{
			EditorUtility.SetDirty(graph);
		}
	}
}