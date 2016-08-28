using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NS_GuiItems
{
	[AddComponentMenu("GuiItems/GuiItemsDrawer")]
	public class GuiItemsDrawer : MonoBehaviour
	{
		/// <summary>
		/// The default GUISkin applied on the GuiItem elements to draw.
		/// </summary>
		public GUISkin thisGuiSkin;


		/// <summary>
		/// Ideal resolution of the interface.
		/// </summary>
		static public Vector2 DesignResolution
		{
			get
			{
				return instance.designResolution;
			}
		}
		[SerializeField]
		private Vector2 designResolution = new Vector2(1920f, 1080f);


		/// <summary>
		/// This value is used when you want to horizontally position a single interface item
		/// using a position obtained from Camera.main.WorldToScreenPoint().
		/// 
		/// For example, we obtain a position on screen with Camera.main.WorldToScreenPoint().
		/// Then we divide x and y of the position to get values relative to the screen size, in [0.0-1.0] range.
		/// Then we multiply x by GuiAreaWidth and y by GuiAreaHeight.
		/// Then we have the correct position usable within a GuiItemsDrawer screen drawing.
		/// 
		/// It is recommended to use RelativeToPixels() instead.
		/// </summary>
		public static int GuiAreaWidth
		{
			get
			{
				return (int)(DesignResolution.x * FittingRatio.x);
			}
		}


		/// <summary>
		/// This value is used when you want to horizontally position a single interface item
		/// using a position obtained from Camera.main.WorldToScreenPoint() or by other means.
		/// 
		/// For example, we obtain a position on screen with Camera.main.WorldToScreenPoint().
		/// Then we divide x and y of the position to get values relative to the screen size, in [0.0-1.0] range.
		/// Then we multiply x by GuiAreaWidth and y by GuiAreaHeight.
		/// Then we have the correct position usable within a GuiItemsDrawer screen drawing.
		/// 
		/// It is recommended to use RelativeToPixels() instead.
		/// </summary>
		public static int GuiAreaHeight
		{
			get
			{
				return (int)(DesignResolution.y * FittingRatio.y);
			}
		}


		/// <summary>
		/// Indicates if the mouse is over any GuiItem from the associated GuiItemsCollection.
		/// </summary>
		public bool MouseOverGuiItems
		{
			get
			{
				return mouseOverGuiItems;
			}
		}


		/// <summary>
		/// Indicates if the mouse is over any interactive GuiItem from the associated GuiItemsCollection.
		/// </summary>
		public bool MouseOverInteractiveGuiItem
		{
			get
			{
				return mouseOverInteractiveGuiItem;
			}
		}

		private bool mouseOverGuiItems;
		private bool mouseOverInteractiveGuiItem;


		/// <summary>
		/// GuiItemsCollections to draw.
		/// </summary>
		public List<GuiItemsCollection> guiItemsOrder;


		/// <summary>
		/// Unique instance.
		/// </summary>
		static public GuiItemsDrawer instance;

		void Awake()
		{
			mouseOverGuiItems = false;
			mouseOverInteractiveGuiItem = false;


			// Initialize the list
			guiItemsOrder = new List<GuiItemsCollection>();

			DontDestroyOnLoad(gameObject);
		}

		void OnEnable()
		{
			instance = this;
		}

		void OnGUI()
		{
			// Clean the list
			guiItemsOrder.RemoveAll(item => item == null);


			// Save the original GUI matrix
			Matrix4x4 matrixBackup = GUI.matrix;


			// If defined, use thisGuiSkin
			GUISkin defaultSkin = null;
			if(thisGuiSkin)
			{
				defaultSkin = GUI.skin;
				GUI.skin = thisGuiSkin;
			}

			foreach(GuiItemsCollection g in guiItemsOrder)
			{
				if(g.enabled)
				{
					g.Draw();
				}
			}


			// Restore matrix from backup
			GUI.matrix = matrixBackup;


			// Look if mouse cursor is over GuiItemsCollection and a GuiItem
			mouseOverGuiItems = false;
			mouseOverInteractiveGuiItem = false;


			// No need to continue if guiItemsOrder list is empty
			if(guiItemsOrder.Count != 0)
			{
				foreach(GuiItemsCollection g in guiItemsOrder)
				{
					// No need to check is this GuiItemsCollection has draw = false
					if(!g.draw)
						continue;

					foreach(GuiItemsCollection.GuiItem item in g.items)
					{
						// If the GuiItem is disabled, do not check it
						if(!item.enabled)
							continue;

						if((item.thisItemType != GuiItemsCollection.GuiItem.itemType.BEGIN_HORIZONTAL) && (item.thisItemType != GuiItemsCollection.GuiItem.itemType.BEGIN_VERTICAL) && (item.thisItemType != GuiItemsCollection.GuiItem.itemType.BEGIN_SCROLL_VIEW))
						{
							if(item.hover)
							{
								mouseOverGuiItems = true;
								break;
							}
						}
					}

					if(mouseOverGuiItems)
					{
						break;
					}
				}


				// Look if the mouse cursor is over an interactive GuiItem
				foreach(GuiItemsCollection g in guiItemsOrder)
				{
					// No need to check is this GuiItemsCollection has draw = false
					if(!g.draw)
						continue;


					foreach(GuiItemsCollection.GuiItem item in g.items)
					{
						// If the GuiItem is disabled, do not check it
						if(!item.enabled)
							continue;

						switch(item.thisItemType)
						{
							case GuiItemsCollection.GuiItem.itemType.BUTTON:
							case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
							case GuiItemsCollection.GuiItem.itemType.REPEAT_BUTTON:
							case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
							case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
							case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
							case GuiItemsCollection.GuiItem.itemType.TOGGLE:
							case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
								if(item.hover)
								{
									mouseOverInteractiveGuiItem = true;
								}
								break;
						}

						if(mouseOverInteractiveGuiItem)
							break;
					}

					if(mouseOverInteractiveGuiItem)
						break;
				}
			}


			// Reset the default GUI skin if thisGuiSkin has been defined
			if(defaultSkin)
			{
				GUI.skin = defaultSkin;
			}
		}

		void Update()
		{
			// Clean the list
			guiItemsOrder.RemoveAll(item => item == null);


			foreach(GuiItemsCollection g in guiItemsOrder)
			{
				if(g.enabled)
				{
					foreach(GuiItemsCollection.GuiItem gi in g.items)
					{
						if(gi.loop)
						{
							// Animate looping labels
							gi.loopOffset += gi.loopScrollSpeed * Time.deltaTime;


							// Limit loopOffset to limit copies count
							if(gi.loopOffset < -gi.LoopLabelRect.width)
								gi.loopOffset += gi.LoopLabelRect.width;
							if(gi.loopOffset > gi.LoopLabelRect.width)
								gi.loopOffset -= gi.LoopLabelRect.width;
						}
					}
				}
			}
		}

		static public void ReferenceCollection(GuiItemsCollection collection)
		{
			if(!instance)
			{
				Debug.LogError("No GuiItemsDrawer object instantiated. Please instantiate a single GuiItemsDrawer object like the one in the GuiItems/GameObjects folder in your scene.");
				return;
			}

			if(!instance.guiItemsOrder.Contains(collection))
			{
				// Reference this GuiItemsCollection in the list, from greater depth to lesser
				if(instance.guiItemsOrder.Count == 0)
				{
					instance.guiItemsOrder.Add(collection);
				}
				else
				{
					for(int i = 0; i < instance.guiItemsOrder.Count; i++)
					{
						if(instance.guiItemsOrder[i].depth <= collection.depth)
						{
							instance.guiItemsOrder.Insert(i, collection);
							break;
						}
						if(i == instance.guiItemsOrder.Count - 1)
						{
							instance.guiItemsOrder.Add(collection);
							break;
						}
					}
				}
			}
		}


		/// <summary>
		/// Values used for GuiItem scaling (position and size) from design resolution
		/// to current resolution.
		/// </summary>
		static public Vector2 FittingRatio
		{
			get
			{
				// Ratio lets us know if the screen is "horizontally-driven" or "vertically-driven"
				Vector2 ratio = new Vector2((float)Screen.width / DesignResolution.x, (float)Screen.height / DesignResolution.y);
				Vector2 correction = Vector2.one;

				if(ratio.x < ratio.y)
				{
					// Horizontally-driven
					correction.y = ratio.y / ratio.x;
				}
				else if(ratio.x > ratio.y)
				{
					// Vertically-driven
					correction.x = ratio.x / ratio.y;
				}

				return correction;
			}
		}


		/// <summary>
		/// Converts a Rect with relative values to absolute values (in pixels) which can
		/// be used in GuiItemsDrawer display mode.
		/// </summary>
		/// <param name="_relativeRect">Relative rect in percentage (values between 0 and 1, usually).</param>
		/// <returns>Absolute rect in pixels.</returns>
		static public Rect RelativeToPixels(Rect _relativeRect)
		{
			_relativeRect.width *= GuiItemsDrawer.GuiAreaWidth;
			_relativeRect.height *= GuiItemsDrawer.GuiAreaHeight;
			_relativeRect.x *= GuiItemsDrawer.GuiAreaWidth;
			_relativeRect.y *= GuiItemsDrawer.GuiAreaHeight;

			return _relativeRect;
		}


		/// <summary>
		/// Converts a Vector2 with relative values to absolute values (in pixels) which can
		/// be used in GuiItemsDrawer display mode.
		/// </summary>
		/// <param name="relativeValue">Relative Vector2 (values between 0 and 1, usually).</param>
		/// <returns>Absolute Vector2 in pixels.</returns>
		static public Vector2 RelativeToPixels(Vector2 _relativeVector2)
		{
			_relativeVector2.x *= GuiItemsDrawer.GuiAreaWidth;
			_relativeVector2.y *= GuiItemsDrawer.GuiAreaHeight;

			return _relativeVector2;
		}
	}
}
