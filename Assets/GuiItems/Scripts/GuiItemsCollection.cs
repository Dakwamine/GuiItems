using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NS_GuiItems
{
	[AddComponentMenu("GuiItems/GuiItemsCollection")]
	public class GuiItemsCollection : MonoBehaviour, ISerializationCallbackReceiver
	{
		/// <summary>
		/// Is this collection actually drawn?
		/// </summary>
		public bool draw = true;


		/// <summary>
		/// Do we want to use the area property to define the render area?
		/// </summary>
		public bool useArea = true;


		/// <summary>
		/// Defines the render area if useArea is true.
		/// </summary>
		public GuiItem.RectExtension area = new GuiItem.RectExtension();


		/// <summary>
		/// If useArea = true, is this GuiItemsCollection use the transform's position instead of the area (x,y)?
		/// </summary>
		public bool useTransformPosition = false;


		/// <summary>
		/// Defines the display order between this collection and another.
		/// </summary>
		public int depth = 0;


		/// <summary>
		/// Contains the GuiItem elements to draw.
		/// </summary>
		public List<GuiItemsCollection.GuiItem> items = new List<GuiItem>();


		/// <summary>
		/// Editor value which indicates if we want to show the GuiItem list in the inspector.
		/// </summary>
		public bool editor_showItemsList = false;


		/// <summary>
		/// GuiItemsInterface object for event handling of this collection.
		/// </summary>
		public GuiItemsInterfaceBase guiItemsInterface
		{
			get
			{
				return GetComponent<GuiItemsInterfaceBase>();
			}
		}


		void Start()
		{
			GuiItemsDrawer.ReferenceCollection(this);
		}

		public void Draw()
		{
			if(!draw)
				return;


			// GUI matrix update
			// The "+2" covers the transparent borders on some screen resolutions.
			float resX = (Screen.width + 2) / (GuiItemsDrawer.DesignResolution.x * GuiItemsDrawer.FittingRatio.x);
			float resY = (Screen.height + 2) / (GuiItemsDrawer.DesignResolution.y * GuiItemsDrawer.FittingRatio.y);

			GUI.matrix = Matrix4x4.TRS(new Vector3(-1f, -1f, 0f), Quaternion.identity, new Vector3(resX, resY, 1f));


			// Area computing
			if(useArea)
			{
				Rect realArea;

				if(useTransformPosition)
				{
					realArea = new Rect(transform.localPosition.x * GuiItemsDrawer.GuiAreaWidth,
										transform.localPosition.y * GuiItemsDrawer.GuiAreaHeight,
										area.StandardSpaceValue.width,
										area.StandardSpaceValue.height);
				}
				else
				{
					realArea = new Rect(area.StandardSpaceValue);
				}

				GUILayout.BeginArea(realArea);
			}

			foreach(GuiItemsCollection.GuiItem i in items)
			{
				if(i.enabled)
					i.Display();
			}

			if(useArea)
				GUILayout.EndArea();


			// Call the GuiItemsScriptedInterface child components
			GuiItemsScriptedInterface[] gisi = GetComponents<GuiItemsScriptedInterface>();

			foreach(GuiItemsScriptedInterface g in gisi)
			{
				g.Draw();
			}
		}


		/// <summary>
		/// Get an item of this collection by its tag.
		/// </summary>
		/// <param name="_itemTag">Tag of the item.</param>
		/// <param name="_logWhenNotFound">Log an error if the item is not found.</param>
		/// <returns>The found item, null if not.</returns>
		public GuiItemsCollection.GuiItem GetItem(string _itemTag, bool _logWhenNotFound = true)
		{
			foreach(GuiItemsCollection.GuiItem g in items)
			{
				if(g.tag == _itemTag)
				{
					return g;
				}
			}

			if(_logWhenNotFound)
				Debug.LogError("Item not found by this tag " + _itemTag + " (length=" + _itemTag.Length + "). Please check the tag in the first parameter.", gameObject);
			return null;
		}


		[System.Serializable]
		public class GuiItem
		{
			/// <summary>
			/// GuiItemsCollection which created this GuiItem.
			/// </summary>
			public GuiItemsCollection guiItemsCollection;

			[System.Obsolete("Please use guiItemsCollection instead of guiItems.")]
			public GuiItemsCollection guiItems;


			/// <summary>
			/// GUI item type.
			/// </summary>
			public enum itemType
			{
				LABEL,
				BOX,
				BUTTON,
				REPEAT_BUTTON,
				TEXT_FIELD,
				PASSWORD_FIELD,
				TEXT_AREA,
				TOGGLE,
				TOOLBAR,
				SELECTION_GRID,
				SPACE,
				FLEXIBLE_SPACE,
				BEGIN_HORIZONTAL,
				END_HORIZONTAL,
				BEGIN_VERTICAL,
				END_VERTICAL,
				BEGIN_SCROLL_VIEW,
				END_SCROLL_VIEW
			}


			/// <summary>
			/// This item type.
			/// </summary>
			public itemType thisItemType = itemType.LABEL;


			/// <summary>
			/// Is this GuiItem drawn?
			/// </summary>
			public bool enabled = true;


			/// <summary>
			/// Is this GuiItem activated / is it interactive? (it is still drawn)
			/// </summary>
			public bool activated = true;


			/// <summary>
			/// This GuiItem name. This is needed for GuiItemsInterfaceBase children for event handling. Can be empty.
			/// </summary>
			public string tag = "";


			/// <summary>
			/// Color of this GuiItem.
			/// </summary>
			public Color color = Color.white;


			/// <summary>
			/// Name of the event to launch when this button is activated.
			/// </summary>
			public string eventToLaunch = "";


			/// <summary>
			/// Parameter to send.
			/// </summary>
			public string parameter = "";


			/// <summary>
			/// Rotation to apply on this GuiItem and subsequent ones.
			/// </summary>
			public float rotationAngle = 0f;
			public GuiItem.Vector2Extension pivotPoint = new Vector2Extension();


			/// <summary>
			/// Does this GuiItem needs to loop? Implemented only for LABEL items for horizontal loops.
			/// </summary>
			public bool loop = false;


			/// <summary>
			/// Loop offset in pixels.
			/// </summary>
			public float loopOffset = 0f;


			/// <summary>
			/// Loop scroll speed in pixels / second.
			/// </summary>
			public float loopScrollSpeed = 0f;


			/// <summary>
			/// The real rect area of the label to loop.
			/// </summary>
			public Rect LoopLabelRect
			{
				get
				{
					return loopLabelRect;
				}
			}
			private Rect loopLabelRect = new Rect();


			/// <summary>
			/// Indicates if loopLabelRect has been defined.
			/// </summary>
			private bool loopLabelRectDefined = false;


			/// <summary>
			/// Last rectangle which contains the element.
			/// </summary>
			public Rect LastRect
			{
				get
				{
					return lastRect;
				}
			}
			private Rect lastRect = new Rect();


			/// <summary>
			/// Contenu(s) de ce GuiItem. Seul la première entrée est utilisée par les éléments, à part Toolbar et SelectionGrid.
			/// Content(s) of this GuiItem. Only the first entry is used, TOOLBAR and SELECTION_GRID apart.
			/// </summary>
			public List<GUIContent> contents = new List<GUIContent>();


			/// <summary>
			/// Read-only contents list in an Array flavour.
			/// </summary>
			public GUIContent[] contentsArray
			{
				get
				{
					return contents.ToArray();
				}
			}


			/// <summary>
			/// First content shortcut of this GuiItem. Used by this GuiItem if it uses a single GUIContent.
			/// </summary>
			public GUIContent content
			{
				get
				{
					return contents[0];
				}
				set
				{
					contents[0] = value;
				}
			}


			/// <summary>
			/// Mask character for passwords. The char value is used in play mode; the string value is used in the editor.
			/// </summary>
			private char _maskChar
			{
				get
				{
					return maskChar[0] != new char() ? maskChar[0] : "*"[0];
				}
			}
			public string maskChar = "*";


			/// <summary>
			/// Max text length for TEXT_FIELD and others.
			/// </summary>
			public int maxLength = -1;


			/// <summary>
			/// TOGGLE value.
			/// </summary>
			public bool toggle = false;


			/// <summary>
			/// Returned value of TOOLBAR and others.
			/// </summary>
			public int selected = 0;


			/// <summary>
			/// Column count in a SELECTION_GRID.
			/// </summary>
			public int xCount = 3;


			/// <summary>
			/// Space used by GuiLayout.Space().
			/// </summary>
			public float pixels = 0f;


			/// <summary>
			/// SCROLL_VIEW position.
			/// </summary>
			public Vector2 scrollViewPosition = new Vector2();


			/// <summary>
			/// Always show the SCROLL_VIEW horizontal bar?
			/// </summary>
			public bool alwaysShowHorizontal = false;


			/// <summary>
			/// Always show the SCROLL_VIEW vertical bar?
			/// </summary>
			public bool alwaysShowVertical = false;


			/// <summary>
			/// SCROLL_VIEW horizontal bar style.
			/// </summary>
			public GuiStyleElement horizontalScrollbarStyle = null;


			/// <summary>
			/// SCROLL_VIEW vertical bar style.
			/// </summary>
			public GuiStyleElement verticalScrollbarStyle = null;


			/// <summary>
			/// SCROLL_VIEW background style.
			/// </summary>
			public GuiStyleElement backgroundStyle = null;


			/// <summary>
			/// GuiItem style mode enum.
			/// </summary>
			public enum guiStyleMode
			{
				/// <summary>
				/// Use the default style.
				/// </summary>
				DEFAULT,


				/// <summary>
				/// Use the same style as the previous GuiItem.
				/// </summary>
				NO_STYLE,


				/// <summary>
				/// Use the style defined by the GuiStyleElement reference.
				/// </summary>
				GUI_STYLE_ELEMENT,


				/// <summary>
				/// Special style used only on this GuiItem.
				/// </summary>
				CUSTOM_STYLE
			}


			/// <summary>
			/// Current style mode of this GuiItem.
			/// </summary>
			public guiStyleMode thisGuiStyleMode = guiStyleMode.DEFAULT;


			/// <summary>
			/// GuiStyleElement object reference for GUI_STYLE_ELEMENT mode.
			/// </summary>
			public GuiStyleElement guiStyleElement = null;


			/// <summary>
			/// Custom style for CUSTOM_STYLE mode.
			/// </summary>
			public GUIStyleExtension customStyle = new GUIStyleExtension();


			/// <summary>
			/// Direct access to this GuiItem's customStyle style.
			/// </summary>
			public GUIStyle CustomStyle
			{
				get
				{
					return customStyle.guiStyle;
				}
				set
				{
					customStyle.guiStyle = value;
				}
			}


			/// <summary>
			/// GUISkin for SCROLL_VIEW (is the GUIStyle not straightforwardingly working on BeginScrollView()?)
			/// </summary>
			public GUISkin guiSkin;


			/// <summary>
			/// Is the mouse cursor on this GuiItem?
			/// </summary>
			private bool _hover = false;
			public bool hover
			{
				get
				{
					return _hover;
				}

				set
				{
					if(!_hover && value)
					{
						if(guiItemsCollection.guiItemsInterface)
							guiItemsCollection.guiItemsInterface.OnHover(this);
					}

					_hover = value;
				}
			}


			//#if UNITY_EDITOR
			/// <summary>
			/// Inspector variable. Indicates if this GuiItem is folded.
			/// </summary>
			public bool editor_folded = false;


			/// <summary>
			/// Inspector variable. Indicates if the contents of this GuiItem are folded.
			/// </summary>
			public bool editor_contentsFolded = false;


			/// <summary>
			/// Inspector variable. Indicates if the style values are folded.
			/// </summary>
			public bool editor_styleFolded = false;
			//#endif


			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="_guiItems">Collection which created this GuiItem.</param>
			public GuiItem(GuiItemsCollection _guiItems)
			{
				contents.Add(new GUIContent());
				guiItemsCollection = _guiItems;
			}


			/// <summary>
			/// Creates by copying an existing GuiItem.
			/// </summary>
			/// <param name="original">The original GuiItem.</param>
			public GuiItem(GuiItemsCollection.GuiItem original)
			{
				this.guiItemsCollection = original.guiItemsCollection;
				this.thisItemType = original.thisItemType;
				this.enabled = original.enabled;
				this.activated = original.activated;
				this.tag = original.tag;
				this.color = Color.white;
				this.eventToLaunch = original.eventToLaunch;
				this.parameter = original.parameter;
				this.rotationAngle = original.rotationAngle;
				this.pivotPoint = new GuiItem.Vector2Extension(original.pivotPoint);
				this.loop = original.loop;
				this.loopOffset = original.loopOffset;
				this.loopScrollSpeed = original.loopScrollSpeed;
				this.loopLabelRect = new Rect(original.loopLabelRect);
				this.loopLabelRectDefined = false; // Keep to false to force rect redefinition
				this.lastRect = new Rect();
				this.contents = new List<GUIContent>();
				foreach(GUIContent gc in original.contents)
				{
					this.contents.Add(new GUIContent(gc));
				}
				this.maskChar = original.maskChar;
				this.maxLength = original.maxLength;
				this.toggle = original.toggle;
				this.selected = original.selected;
				this.xCount = original.xCount;
				this.pixels = original.pixels;
				this.scrollViewPosition = original.scrollViewPosition;
				this.thisGuiStyleMode = original.thisGuiStyleMode;
				this.guiStyleElement = original.guiStyleElement;
				this.customStyle = new GUIStyleExtension(original.customStyle);
				this._hover = original._hover;

				//#if UNITY_EDITOR
				this.editor_folded = false;
				this.editor_contentsFolded = false;
				this.editor_styleFolded = false;
				//#endif
			}

			public void Display()
			{
				if(rotationAngle != 0f)
				{
					// Rotation is always around the pivot point in current screen space, not the standard screen space
					GUIUtility.RotateAroundPivot(rotationAngle, pivotPoint.CurrentSpaceValue);
				}


				// Disable GUI if this element is not activated
				if(!activated)
					GUI.enabled = false;


				// Save the current GUI color
				Color colorSave = GUI.color;


				// Change the GUI color
				GUI.color = color;


				switch(thisItemType)
				{
					case itemType.LABEL:
						if(loop)
						{
							// Make an empty label to obtain the GuiItem limits
							if(thisGuiStyleMode == guiStyleMode.DEFAULT)
							{
								// Obtain the real GuiItem limits
								if(!loopLabelRectDefined)
								{
									loopLabelRect = GUILayoutUtility.GetRect(content, GUI.skin.label);
								}


								GUIStyle g = new GUIStyle(GUI.skin.label);
								g.stretchWidth = true;

								GUILayout.Label("", g, GUILayout.MinWidth(1f));
							}
							else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
							{
								if(guiStyleElement != null)
								{
									// Obtain the real GuiItem limits
									if(!loopLabelRectDefined)
									{
										loopLabelRect = GUILayoutUtility.GetRect(content, guiStyleElement.guiStyle);
									}


									GUIStyle g = new GUIStyle(guiStyleElement.guiStyle);
									g.stretchWidth = true;
									GUILayout.Label("", g, GUILayout.MinWidth(1f));
								}
								else
								{
									// Obtain the real GuiItem limits
									if(!loopLabelRectDefined)
									{
										loopLabelRect = GUILayoutUtility.GetRect(content, GUI.skin.label);
									}


									GUIStyle g = new GUIStyle(GUI.skin.label);
									g.stretchWidth = true;
									Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);
									GUILayout.Label("", g, GUILayout.MinWidth(1f));
								}
							}
							else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
							{
								// Obtain the real GuiItem limits
								if(!loopLabelRectDefined)
								{
									loopLabelRect = GUILayoutUtility.GetRect(content, CustomStyle);
								}

								GUIStyle g = new GUIStyle(CustomStyle);
								g.stretchWidth = true;
								GUILayout.Label("", g, GUILayout.MinWidth(1f));
							}
							else
							{
								// Obtain the real GuiItem limits
								if(!loopLabelRectDefined)
								{
									loopLabelRect = GUILayoutUtility.GetRect(content, GUI.skin.label);
								}


								GUIStyle g = new GUIStyle(GUI.skin.label);
								g.stretchWidth = true;
								GUILayout.Label("", g, GUILayout.MinWidth(1f));
							}
						}
						else
						{
							if(thisGuiStyleMode == guiStyleMode.DEFAULT)
								GUILayout.Label(content, GUI.skin.label);
							else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
							{
								if(guiStyleElement != null)
									GUILayout.Label(content, guiStyleElement.guiStyle);
								else
								{
									Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);
									GUILayout.Label(content);
								}
							}
							else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
								GUILayout.Label(content, CustomStyle);
							else
								GUILayout.Label(content);
						}
						break;

					case itemType.BOX:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
							GUILayout.Box(content, GUI.skin.box);
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
								GUILayout.Box(content, guiStyleElement.guiStyle);
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);
								GUILayout.Box(content);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
							GUILayout.Box(content, CustomStyle);
						else
							GUILayout.Box(content);

						break;

					case itemType.BUTTON:
						if(DisplayButton())
						{
							if(guiItemsCollection.guiItemsInterface == null)
								Debug.LogError("Cannot send event. No interface found. You have to add a GuiItemsInterface script on this game object.");
							else
								guiItemsCollection.guiItemsInterface.ReceiveEvent(this);
						}
						break;

					case itemType.REPEAT_BUTTON:
						if(DisplayRepeatButton())
						{
							if(guiItemsCollection.guiItemsInterface == null)
								Debug.LogError("Cannot send event. No interface found. You have to add a GuiItemsInterface script on this game object.");
							else
								guiItemsCollection.guiItemsInterface.ReceiveEvent(this);
						}
						break;

					case itemType.TEXT_FIELD:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							if(maxLength < 0)
								content.text = GUILayout.TextField(content.text, GUI.skin.textField);
							else
								content.text = GUILayout.TextField(content.text, maxLength, GUI.skin.textField);
						}
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
							{
								if(maxLength < 0)
									content.text = GUILayout.TextField(content.text, guiStyleElement.guiStyle);
								else
									content.text = GUILayout.TextField(content.text, maxLength, guiStyleElement.guiStyle);
							}
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								if(maxLength < 0)
									content.text = GUILayout.TextField(content.text);
								else
									content.text = GUILayout.TextField(content.text, maxLength);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							if(maxLength < 0)
								content.text = GUILayout.TextField(content.text, CustomStyle);
							else
								content.text = GUILayout.TextField(content.text, maxLength, CustomStyle);
						}
						else
						{
							if(maxLength < 0)
								content.text = GUILayout.TextField(content.text);
							else
								content.text = GUILayout.TextField(content.text, maxLength);
						}

						break;

					case itemType.PASSWORD_FIELD:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							if(maxLength < 0)
								content.text = GUILayout.PasswordField(content.text, _maskChar, GUI.skin.textField);
							else
								content.text = GUILayout.PasswordField(content.text, _maskChar, maxLength, GUI.skin.textField);
						}
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
							{
								if(maxLength < 0)
									content.text = GUILayout.PasswordField(content.text, _maskChar, guiStyleElement.guiStyle);
								else
									content.text = GUILayout.PasswordField(content.text, _maskChar, maxLength, guiStyleElement.guiStyle);
							}
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								if(maxLength < 0)
									content.text = GUILayout.PasswordField(content.text, _maskChar);
								else
									content.text = GUILayout.PasswordField(content.text, _maskChar, maxLength);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							if(maxLength < 0)
								content.text = GUILayout.PasswordField(content.text, _maskChar, CustomStyle);
							else
								content.text = GUILayout.PasswordField(content.text, _maskChar, maxLength, CustomStyle);
						}
						else
						{
							if(maxLength < 0)
								content.text = GUILayout.PasswordField(content.text, _maskChar);
							else
								content.text = GUILayout.PasswordField(content.text, _maskChar, maxLength);
						}

						break;

					case itemType.TEXT_AREA:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							if(maxLength < 0)
								content.text = GUILayout.TextArea(content.text, GUI.skin.textArea);
							else
								content.text = GUILayout.TextArea(content.text, maxLength, GUI.skin.textArea);
						}
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
							{
								if(maxLength < 0)
									content.text = GUILayout.TextArea(content.text, guiStyleElement.guiStyle);
								else
									content.text = GUILayout.TextArea(content.text, maxLength, guiStyleElement.guiStyle);
							}
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								if(maxLength < 0)
									content.text = GUILayout.TextField(content.text);
								else
									content.text = GUILayout.TextField(content.text, maxLength);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							if(maxLength < 0)
								content.text = GUILayout.TextArea(content.text, CustomStyle);
							else
								content.text = GUILayout.TextArea(content.text, maxLength, CustomStyle);
						}
						else
						{
							if(maxLength < 0)
								content.text = GUILayout.TextField(content.text);
							else
								content.text = GUILayout.TextField(content.text, maxLength);
						}

						break;

					case itemType.TOGGLE:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
							toggle = GUILayout.Toggle(toggle, content, GUI.skin.toggle);
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
								toggle = GUILayout.Toggle(toggle, content, guiStyleElement.guiStyle);
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								toggle = GUILayout.Toggle(toggle, content);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
							toggle = GUILayout.Toggle(toggle, content, CustomStyle);
						else
							toggle = GUILayout.Toggle(toggle, content);

						break;

					case itemType.TOOLBAR:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
							selected = GUILayout.Toolbar(selected, contentsArray, GUI.skin.button);
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
								selected = GUILayout.Toolbar(selected, contentsArray, guiStyleElement.guiStyle);
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								selected = GUILayout.Toolbar(selected, contentsArray);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
							selected = GUILayout.Toolbar(selected, contentsArray, CustomStyle);
						else
							selected = GUILayout.Toolbar(selected, contentsArray);

						break;

					case itemType.SELECTION_GRID:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
							selected = GUILayout.SelectionGrid(selected, contentsArray, xCount, GUI.skin.button);
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
								selected = GUILayout.SelectionGrid(selected, contentsArray, xCount, guiStyleElement.guiStyle);
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								selected = GUILayout.SelectionGrid(selected, contentsArray, xCount);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
							selected = GUILayout.SelectionGrid(selected, contentsArray, xCount, CustomStyle);
						else
							selected = GUILayout.SelectionGrid(selected, contentsArray, xCount);

						break;

					case itemType.SPACE:
						GUILayout.Space(pixels);
						break;

					case itemType.FLEXIBLE_SPACE:
						GUILayout.FlexibleSpace();
						break;

					case itemType.BEGIN_HORIZONTAL:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							if(content.image)
							{
								GUILayout.BeginHorizontal(content.image, GUI.skin.box);
							}
							else
							{
								GUILayout.BeginHorizontal(content.text, GUI.skin.box);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
							{
								if(content.image)
								{
									GUILayout.BeginHorizontal(content.image, guiStyleElement.guiStyle);
								}
								else
								{
									GUILayout.BeginHorizontal(content.text, guiStyleElement.guiStyle);
								}
							}
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								GUILayout.BeginHorizontal();
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							if(content.image)
							{
								GUILayout.BeginHorizontal(content.image, CustomStyle);
							}
							else
							{
								GUILayout.BeginHorizontal(content.text, CustomStyle);
							}
						}
						else
							GUILayout.BeginHorizontal();

						break;

					case itemType.END_HORIZONTAL:
						GUILayout.EndHorizontal();
						break;

					case itemType.BEGIN_VERTICAL:
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							if(content.image)
							{
								GUILayout.BeginVertical(content.image, GUI.skin.box);
							}
							else
							{
								GUILayout.BeginVertical(content.text, GUI.skin.box);
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
						{
							if(guiStyleElement != null)
							{
								if(content.image)
								{
									GUILayout.BeginVertical(content.image, guiStyleElement.guiStyle);
								}
								else
								{
									GUILayout.BeginVertical(content.text, guiStyleElement.guiStyle);
								}
							}
							else
							{
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);

								GUILayout.BeginVertical();
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							if(content.image)
							{
								GUILayout.BeginVertical(content.image, CustomStyle);
							}
							else
							{
								GUILayout.BeginVertical(content.text, CustomStyle);
							}
						}
						else
							GUILayout.BeginVertical();

						break;

					case itemType.END_VERTICAL:
						GUILayout.EndVertical();
						break;

					case itemType.BEGIN_SCROLL_VIEW:
						GUISkin gs = GUI.skin;

						if(guiSkin)
						{
							GUI.skin = guiSkin;
						}

						scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical);

						if(guiSkin)
						{
							GUI.skin = gs;
						}

						break;

					#region BEGIN_SCROLL_VIEW does not support styles (Unity Engine bug)
					/*
					if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbarStyle == null ? GUI.skin.horizontalScrollbar : horizontalScrollbarStyle.guiStyle, verticalScrollbarStyle == null ? GUI.skin.verticalScrollbar : verticalScrollbarStyle.guiStyle, GUI.skin.scrollView);
					else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
					{
						if(guiStyleElement != null)
							scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbarStyle == null ? GUI.skin.horizontalScrollbar : horizontalScrollbarStyle.guiStyle, verticalScrollbarStyle == null ? GUI.skin.verticalScrollbar : verticalScrollbarStyle.guiStyle, guiStyleElement.guiStyle);
						else
						{
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

							scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbarStyle == null ? GUI.skin.horizontalScrollbar : horizontalScrollbarStyle.guiStyle, verticalScrollbarStyle == null ? GUI.skin.verticalScrollbar : verticalScrollbarStyle.guiStyle);
						}
					}
					else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbarStyle == null ? GUI.skin.horizontalScrollbar : horizontalScrollbarStyle.guiStyle, verticalScrollbarStyle == null ? GUI.skin.verticalScrollbar : verticalScrollbarStyle.guiStyle, CustomStyle);
					else
						scrollViewPosition = GUILayout.BeginScrollView(scrollViewPosition, alwaysShowHorizontal, alwaysShowVertical, horizontalScrollbarStyle == null ? GUI.skin.horizontalScrollbar : horizontalScrollbarStyle.guiStyle, verticalScrollbarStyle == null ? GUI.skin.verticalScrollbar : verticalScrollbarStyle.guiStyle);

					break;
				     */
					#endregion

					case itemType.END_SCROLL_VIEW:
						GUILayout.EndScrollView();
						break;
				}

				if((thisItemType != itemType.BEGIN_HORIZONTAL) && (thisItemType != itemType.BEGIN_VERTICAL) && (thisItemType != itemType.BEGIN_SCROLL_VIEW))
				{
					// Wait for Repaint to check positionings
					if(Event.current.type == EventType.Repaint)
					{
						lastRect = GUILayoutUtility.GetLastRect();

						hover = lastRect.Contains(Event.current.mousePosition);


						// If this GuiItem is a non-empty looping LABEL
						if(thisItemType == itemType.LABEL && loop && lastRect.width != 0f)
						{
							loopLabelRectDefined = true;


							#region Add copies to the left

							Rect r = new Rect(lastRect);

							GUI.BeginGroup(r);

							r.x = 0f;
							r.y = 0f;
							r.width = loopLabelRect.width;
							r.height = loopLabelRect.height;


							// Shift the rect by loopOffset
							r.x += loopOffset;

							while(r.xMax > lastRect.xMin)
							{
								if(thisGuiStyleMode == guiStyleMode.DEFAULT)
								{
									GUI.Label(r, content, GUI.skin.label);
								}
								else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
								{
									if(guiStyleElement != null)
									{
										GUI.Label(r, content, guiStyleElement.guiStyle);
									}
									else
									{
										GUI.Label(r, content, GUI.skin.label);
									}
								}
								else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
								{
									GUI.Label(r, content, CustomStyle);
								}
								else
								{
									GUI.Label(r, content, GUI.skin.label);
								}


								// Move the copy by its width
								r.x -= loopLabelRect.width;
							}

							#endregion

							#region Add copies to the right

							r = new Rect(lastRect);

							r.x = loopLabelRect.width;
							r.y = 0f;
							r.width = loopLabelRect.width;
							r.height = loopLabelRect.height;


							// Shift the rect by loopOffset
							r.x += loopOffset;

							while(r.xMin < lastRect.xMax)
							{
								if(thisGuiStyleMode == guiStyleMode.DEFAULT)
								{
									GUI.Label(r, content, GUI.skin.label);
								}
								else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
								{
									if(guiStyleElement != null)
									{
										GUI.Label(r, content, guiStyleElement.guiStyle);
									}
									else
									{
										GUI.Label(r, content, GUI.skin.label);
									}
								}
								else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
								{
									GUI.Label(r, content, CustomStyle);
								}
								else
								{
									GUI.Label(r, content, GUI.skin.label);
								}


								// Move the copy by its width
								r.x += loopLabelRect.width;
							}

							#endregion


							GUI.EndGroup();
						}
					}
				}


				// Undo any GUI.color changes
				GUI.color = colorSave;


				// Activate the GUI again if it was deactivated
				if(!GUI.enabled)
					GUI.enabled = true;
			}


			/// <summary>
			/// Displays a normal button which sends event when unpressed.
			/// </summary>
			/// <returns>Button activation. true if clicked (push and release).</returns>
			private bool DisplayButton()
			{
				if(thisGuiStyleMode == guiStyleMode.DEFAULT)
					return GUILayout.Button(content, GUI.skin.button);
				else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
					if(guiStyleElement != null)
						return GUILayout.Button(content, guiStyleElement.guiStyle);
					else
					{
						Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);
						return GUILayout.Button(content);
					}
				else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
					return GUILayout.Button(content, CustomStyle);
				else
					return GUILayout.Button(content);
			}


			/// <summary>
			/// Displays a button which sends continuous events while pressed.
			/// </summary>
			/// <returns>Button activation. true if pushed.</returns>
			private bool DisplayRepeatButton()
			{
				if(thisGuiStyleMode == guiStyleMode.DEFAULT)
					return GUILayout.RepeatButton(content, GUI.skin.button);
				else if(thisGuiStyleMode == guiStyleMode.GUI_STYLE_ELEMENT)
					if(guiStyleElement != null)
						return GUILayout.RepeatButton(content, guiStyleElement.guiStyle);
					else
					{
						Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItemsCollection);
						return GUILayout.RepeatButton(content);
					}
				else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
					return GUILayout.RepeatButton(content, CustomStyle);
				else
					return GUILayout.RepeatButton(content);
			}





			/// <summary>
			/// A struct use to embedded two FloatExtension as a Vector2.
			/// </summary>
			[System.Serializable]
			public class Vector2Extension
			{
				public FloatExtension x;
				public FloatExtension y;

				public Vector2Extension()
				{
					x = new FloatExtension();
					y = new FloatExtension(true);
				}


				public Vector2Extension(Vector2Extension toCopy)
				{
					x = new FloatExtension(toCopy.x);
					y = new FloatExtension(toCopy.y);
				}


				/// <summary>
				/// The real Value to use when rendering the GUI.
				/// </summary>
				public Vector2 StandardSpaceValue
				{
					get
					{
						return new Vector2(x.StandardSpaceValue, y.StandardSpaceValue);
					}
				}


				/// <summary>
				/// (Not used anymore!) The real Value to use when rotating the GUI.
				/// </summary>
				public Vector2 CurrentSpaceValue
				{
					get
					{
						return new Vector2(x.CurrentSpaceValue, y.CurrentSpaceValue);
					}
				}
			}


			/// <summary>
			/// A struct use to embedded two FloatExtension as a Vector2.
			/// </summary>
			[System.Serializable]
			public class RectExtension
			{
				public FloatExtension x;
				public FloatExtension y;
				public FloatExtension width;
				public FloatExtension height;

				public RectExtension()
				{
					x = new FloatExtension();
					y = new FloatExtension(true);
					width = new FloatExtension();
					height = new FloatExtension(true);
				}


				public RectExtension(RectExtension toCopy)
				{
					x = new FloatExtension(toCopy.x);
					y = new FloatExtension(toCopy.y);
					width = new FloatExtension(toCopy.width);
					height = new FloatExtension(toCopy.height);
				}


				/// <summary>
				/// The real Value to use when rendering the GUI.
				/// </summary>
				public Rect StandardSpaceValue
				{
					get
					{
						return new Rect(x.StandardSpaceValue, y.StandardSpaceValue, width.StandardSpaceValue, height.StandardSpaceValue);
					}
				}
			}


			/// <summary>
			/// A class used to define more precisely a float Value.
			/// </summary>
			[System.Serializable]
			public class FloatExtension
			{
				/// <summary>
				/// Value in standard screen space.
				/// </summary>
				public float value;


				/// <summary>
				/// Is the Value in percentage (0,1) or in pixels?
				/// </summary>
				public bool relative;


				/// <summary>
				/// If the Value is relative, is it relative to the height or the width?
				/// </summary>
				private bool relativeToHeight;

				public FloatExtension(bool _relativeToHeight = false)
				{
					value = 0f;
					relative = true;
					relativeToHeight = _relativeToHeight;
				}

				public FloatExtension(FloatExtension toCopy)
				{
					value = toCopy.value;
					relative = toCopy.relative;
					relativeToHeight = toCopy.relativeToHeight;
				}

				public FloatExtension(float _value, bool _relative = false, bool _relativeToHeight = false)
				{
					value = _value;
					relative = _relative;
					relativeToHeight = _relativeToHeight;
				}

				public float StandardSpaceValue
				{
					get
					{
						if(relative)
						{
							if(relativeToHeight)
							{
								return value * (float)GuiItemsDrawer.GuiAreaHeight;
							}
							else
							{
								return value * (float)GuiItemsDrawer.GuiAreaWidth;
							}
						}
						else
						{
							return value;
						}
					}
				}


				public float CurrentSpaceValue
				{
					get
					{
						if(relative)
						{
							if(relativeToHeight)
							{
								return value * (float)Screen.height;
							}
							else
							{
								return value * (float)Screen.width;
							}
						}
						else
						{
							return value;
						}
					}
				}
			}
		}


		public class ErrMess
		{
			/// <summary>
			/// Message: A GuiStyleElement has not been defined for a GuiItem from this collection which needed it; no style will be applied on this GuiItem. Please set the guiStyleElement variable or use NO_STYLE mode instead of GUI_STYLE_ELEMENT.
			/// </summary>
			static public string noGuiStyleElementDefined = "A GuiStyleElement has not been defined for a GuiItem from this collection which needed it; no style will be applied on this GuiItem. Please set the guiStyleElement variable or use NO_STYLE mode instead of GUI_STYLE_ELEMENT.";
		}

		#region ISerializationCallbackReceiver Members

		public void OnAfterDeserialize(){}

		public void OnBeforeSerialize()
		{
			// Transition code: renamed GuiItemsCollection.GuiItem.guiItems to GuiItemsCollection.GuiItem.guiItemsCollection
			foreach(GuiItem i in items)
			{
				#pragma warning disable 618
				i.guiItemsCollection = i.guiItems;
			}
		}

		#endregion
	}
}
