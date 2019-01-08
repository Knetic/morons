using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public class MoronEditorWindow : EditorWindow
	{
		protected static readonly Color INTENT_COLOR = new Color(.9f, .38f, 0f);
		protected static readonly Color CHOICE_COLOR = new Color(0f, .38f, .9f);
		protected static readonly Color INTENT_ROOT_COLOR = new Color(.9f, .20f, 0f);

		protected static readonly Color YES_TRANSITION_COLOR = new Color(.28f, 1f, .42f);
		protected static readonly Color NO_TRANSITION_COLOR = new Color(1f, .28f, .28f);
		protected static readonly Color NEXT_TRANSITION_COLOR = new Color(1f, .48f, .28f);
		protected static readonly Color SELECTED_TRANSITION_COLOR = new Color(.83f, .28f, 1f);

		protected static readonly Color NODE_BACKGROUND_COLOR = new Color(.9f, .9f, .9f);

		protected static MoronGraph graph;

		protected MoronEditorInput input;
		
		protected GUIStyle labelStyle;
		protected Texture2D nodeGradient;

		protected Vector2 scrollPosition;

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

			input = new MoronEditorInput();
			input.selectionCallback = findNodeForPosition;
			input.edgeSelectionCallback = findEdgeForPosition;
			input.rightClickCallback = rightClick;
			input.transitionCallback = makeTransition;			
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
			EditorWindow.GetWindow<MoronStatemapEditorWindow>().Show();
			return true;
		}

		public void OnEnable()
		{
			input.reset();
		}

		public MoronGraph getGraph()
		{
			return graph;
		}

		public void OnGUI()
		{
			if(graph == null)
				return;

			input.updateInput(Event.current);
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
			input.scrollPosition = GUI.BeginScrollView(realPosition, input.scrollPosition, findMaxArea());
			
			if(input.transitionLine)
				drawTransitionLine();
			
			foreach(Pair<int, int> edge in graph.edges)
				drawTransitionEdge(edge);

			foreach(MoronIONode node in graph.nodes)
				drawNode(node);

			GUI.EndScrollView();
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
			Rect borderRect;
			
			// border
			borderRect = nodeRect;
			borderRect.x -= 1;
			borderRect.y -= 1;
			borderRect.width += 2;
			borderRect.height += 2;

			if(node == input.selected)
				GUI.color = Color.yellow;
			else
				GUI.color = Color.black;

			GUI.DrawTexture(borderRect, EditorGUIUtility.whiteTexture);

			// body
			if(node is MoronIntent)
			{
				if(node == graph.nodes[0])
					GUI.color = INTENT_ROOT_COLOR;
				else
					GUI.color = INTENT_COLOR;
			}
			if(node is MoronChoice)
				GUI.color = CHOICE_COLOR;

			if(Application.isPlaying && Selection.gameObjects.Length > 0)
			{
				thinker = Selection.gameObjects[0].GetComponent<MoronThinker>();
				if(thinker != null && thinker.getCurrentNode().guid == node.guid)
					GUI.color = Color.white;
			}

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
			System.Object value;
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
				
				value = EditorPropertyHelper.propertyField(current, label, field.Name, field.FieldType, node.getManualJoist(field.Name));
				node.setManualJoist(field.Name, value);
				
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
			int index;

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
				index = Array.IndexOf(matchingArray, getCall(field.Name));
				if(index < 0)
					index = 0;

				index = EditorGUI.Popup(current, index, matchingArray);
				
				if(index > 0 && index < matchingArray.Length)
					setCall(field.Name, matchingArray[index]);
				
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
			if(input.selectedEdge >= 0 && indices.Equals(graph.edges[input.selectedEdge]))
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
			Handles.DrawLine(input.selected.position, Event.current.mousePosition);
		}

		protected void drawArrow(Vector2 position, Quaternion rotation, float width, float length)
		{
			Vector2 a, b, c;

			a = (Vector2)(rotation * new Vector2(-width, -length)) + position;
			b = (Vector2)(rotation * new Vector2(width, -length)) + position;
			c =  position;

			Handles.DrawAAConvexPolygon(a, b, c);
		}

		protected void startTransition(System.Object userdata)
		{
			input.transitionLine = true;
			input.transitionReplacement = (MoronTransitionCondition)userdata;
		}

		protected void deleteTransition(System.Object userdata)
		{
			graph.edges.RemoveAt(input.selectedEdge);
			input.reset();
		}

		protected void toggleInterrupting(System.Object userdata)
		{
			MoronChoice selected;

			selected = ((MoronChoice)input.selected);
			selected.interruptsIntent = !selected.interruptsIntent;
		}

		protected void setRootIntent(System.Object userdata)
		{
			Pair<int, int> edge;
			int nodeIndex;

			// swap
			nodeIndex = graph.nodes.IndexOf(input.selected);
			graph.nodes[nodeIndex] = graph.nodes[0];
			graph.nodes[0] = input.selected;

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
		}

		protected void deleteNode(System.Object userdata)
		{
			Pair<int, int> modifiedPair, originalPair;
			int index, from, to;

			index = graph.nodes.IndexOf(input.selected);
			graph.nodes.Remove(input.selected);

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

		protected void rightClick()
		{
			GenericMenu menu;
			
			menu  = new GenericMenu();

			if(input.selected == null)
			{				
				if(input.selectedEdge < 0)
				{
					makeTypeCreators(typeof(MoronIntent), menu, "Create Intent/");
					makeTypeCreators(typeof(MoronChoice), menu, "Create Choice/");
				}
				else
					menu.AddItem(new GUIContent("Delete Transition"), false, deleteTransition, null);
			}
			else
				populateNodeRightClick(menu);
			
			menu.ShowAsContext();
		}

		protected void populateNodeRightClick(GenericMenu menu)
		{
			if(input.selected is MoronIntent)
			{
				menu.AddItem(new GUIContent("Make Transition"), false, startTransition, MoronTransitionCondition.INTENT);

				if(graph.nodes[0] != input.selected)
					menu.AddItem(new GUIContent("Set as Root"), false, setRootIntent, null);
				else
					menu.AddDisabledItem(new GUIContent("Set as Root"));
			}

			if(input.selected is MoronChoice)
			{
				if(((MoronChoice)input.selected).interruptsIntent)
					menu.AddItem(new GUIContent("Set as non-interrupting"), false, toggleInterrupting, null);
				else
					menu.AddItem(new GUIContent("Set as interrupting"), false, toggleInterrupting, null);

				menu.AddItem(new GUIContent("Set 'yes' Transition"), false, startTransition, MoronTransitionCondition.YES);	

				if(findNeighborsFrom(graph.nodes.IndexOf(input.selected)).Count() > 0)
					menu.AddItem(new GUIContent("Set 'no' Transition"), false, startTransition, MoronTransitionCondition.NO);	
				else
					menu.AddDisabledItem(new GUIContent("Set 'no' Transition"));	
			}
			
			menu.AddItem(new GUIContent("Delete"), false, deleteNode, null);
		}

		protected void makeTransition(MoronIONode from, MoronIONode to)
		{
			Pair<int, int> edge;
			int metaIndex, previousEdgeMetaIndex, count;

			if(from == to)
				return;

			metaIndex = graph.nodes.IndexOf(from);
			previousEdgeMetaIndex = -1;

			switch(input.transitionReplacement)
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
			String name;

			matchingTypes = 
				AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => t.IsAssignableFrom(p) && p.IsClass && p != t);
			
			foreach(Type matched in matchingTypes)
			{
				name = findNodeTypeName(matched);
				menu.AddItem(new GUIContent(prefix + name), false, (_) => createNode(matched), null);
			}
		}

		protected void createNode(Type t)
		{
			MoronIONode node;

			node = (MoronIONode)Activator.CreateInstance(t);
			node.position = input.lastClicked + input.scrollPosition;
			graph.nodes.Add(node);
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
	}
}