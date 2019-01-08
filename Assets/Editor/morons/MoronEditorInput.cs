using System;
using UnityEngine;
using UnityEditor;

namespace Gridsnap.Morons
{
	public delegate MoronIONode MoronSelectionRequestEvent(Vector2 position);
	public delegate int MoronEdgeSelectionRequestEvent(Vector2 position);
	public delegate void MoronRightClick();
	public delegate void MoronTransition(MoronIONode from, MoronIONode to);

	public class MoronEditorInput
	{
		public const float GRIDSNAP_SIZE = 20;

		public MoronSelectionRequestEvent selectionCallback;
		public MoronRightClick rightClickCallback;
		public MoronTransition transitionCallback;
		public MoronEdgeSelectionRequestEvent edgeSelectionCallback;

		public MoronIONode selected;
		public int selectedEdge;

		public Vector2 lastClicked, lastDragged;
		public Vector2 scrollPosition;

		public MoronTransitionCondition transitionReplacement;
		public bool transitionLine;

		protected Vector2 selectionOffset;
		protected bool m0;

		public MoronEditorInput()
		{
			reset();
		}

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

					lastClicked = current.mousePosition;

					switch(current.button)
					{
						case 0:

							selectedEdge = -1;
							currentNode = selectionCallback(position);

							if(transitionLine)
							{
								m0 = false;

								if(currentNode != null)
									transitionCallback(selected, currentNode);
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
								selectionOffset = Vector3.zero;
								selectedEdge = edgeSelectionCallback(position);
							}
							
							break;
						case 1:

							selected = selectionCallback(position);
							if(selected == null)
								selectedEdge = edgeSelectionCallback(position);

							rightClickCallback();
							break;
					}

					transitionLine = false;
					GUI.FocusControl("");
					break;

				case EventType.MouseUp:

					transitionLine = false;

					if(current.button != 0)
						break;
					
					if(selectionCallback(position) != selected)
						selected = null;
					m0 = false;
					break;
				
				case EventType.MouseDrag:

					lastDragged = position;
					if(!m0 || selected == null)
						break;
					
					selected.position = VectorHandling.gridsnap(lastDragged + selectionOffset, GRIDSNAP_SIZE);
					break;
			}
		}
	}
}