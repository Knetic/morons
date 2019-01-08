using System;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public partial class MoronEditorWindow : EditorWindow
	{
		public const float GRIDSNAP_SIZE = 10;

		public MoronIONode selected;
		public int selectedEdge;

		public Vector2 lastClicked, lastDragged;
		public Vector2 scrollPosition;

		public MoronTransitionCondition transitionReplacement;
		public bool transitionLine;
		public bool m0;

		protected Vector2 selectionOffset;

		public void reset()
		{
			selectedEdge = -1;
			selected = null;
			m0 = false;
			transitionLine = false;
		}

		public void updateInput(Event current)
		{
			MoronIONode currentNode;
			Vector2 position;

			position =  current.mousePosition + scrollPosition;

			switch(current.type)
			{
				case EventType.MouseDown:

					if(!checkStatemapWidth(position.x))
						break;

					lastClicked = position;

					switch(current.button)
					{
						case 0:

							selectedEdge = -1;
							currentNode = findNodeForPosition(position);

							if(transitionLine)
							{
								m0 = false;

								if(currentNode != null)
									makeTransition(selected, currentNode);
								break;
							}

							selected = currentNode;

							if(selected != null)
							{
								selectionOffset = VectorHandling.convXY(selected.position) - position;
								m0 = true;
							}
							else
							{
								deselectNode();
								selectionOffset = Vector3.zero;
								selectedEdge = findEdgeForPosition(position);
							}
							
							break;
						case 1:

							selected = findNodeForPosition(position);
							if(selected == null)
								selectedEdge = findEdgeForPosition(position);

							rightClick();
							break;
					}

					transitionLine = false;
					GUI.FocusControl("");
					break;

				case EventType.MouseUp:

					transitionLine = false;

					if(current.button != 0)
						break;
					
					m0 = false;

					if(!checkStatemapWidth(position.x))
						break;
					
					if(findNodeForPosition(position) != selected)
						deselectNode();
					break;
				
				case EventType.MouseDrag:

					lastDragged = position;
					if(!m0 || selected == null)
						break;
					
					selected.position = VectorHandling.gridsnap(lastDragged + selectionOffset, GRIDSNAP_SIZE);
					markDirty();					
					break;
				
				case EventType.KeyDown:

					if(!checkStatemapWidth(current.mousePosition.x))
						break;

					switch(current.keyCode)
					{
						case KeyCode.X:
						case KeyCode.Delete:

							deleteSelected();					
							break;
						case KeyCode.T:

							MoronTransitionCondition condition;

							if(selected == null || transitionLine)
								break;
								
							condition = MoronTransitionCondition.INTENT;

							if(selected is MoronChoice)
								condition = MoronTransitionCondition.YES;

							startTransition(condition);
							break;
						
						case KeyCode.A:

							showAddMenu();
							break;
					}
					break;
			}
		}

		protected void deselectNode()
		{
			selected = null;
			GUI.FocusControl("");
		}
	}
}