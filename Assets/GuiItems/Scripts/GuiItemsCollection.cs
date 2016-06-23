using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("GuiItems/GuiItemsCollection")]
public class GuiItemsCollection : MonoBehaviour
{
	/// <summary>
	/// Est-ce que cette collection est affichée ?
	/// </summary>
	public bool draw = true;


	/// <summary>
	/// Est-ce que l'on veut utiliser la propriété area pour définir la zone de rendu ?
	/// </summary>
	public bool useArea = true;


	/// <summary>
	/// Zone d'affichage des éléments.
	/// </summary>
	public GuiItem.RectExtension area = new GuiItem.RectExtension();


	/// <summary>
	/// Est-ce que ce GuiITtems utilise la position du transform pour se positionner ?
	/// </summary>
	public bool useTransformPosition = false;


	/// <summary>
	/// Ordre d'affichage de ce GuiItems.
	/// </summary>
	public int depth = 0;


	/// <summary>
	/// Liste des objets contenus dans ce GuiItems.
	/// </summary>
	public List<GuiItemsCollection.GuiItem> items = new List<GuiItem>();


	/// <summary>
	/// Variable pour l'inspecteur pour indiquer si la liste des objets doit être affichée.
	/// </summary>
	public bool editor_showItemsList = false;


	/// <summary>
	/// Objet GuiItemsInterface qui va réceptionner les events activés par les éléments de ce GuiItems.
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
		// Ne pas afficher si la collection est désactivée
		if(!draw)
			return;


		// Mise à jour de la matrice GUI
		Vector2 correction = GuiItemsDrawer.FittingRatio;


		// Le "+2" permet d'augmenter la zone de rendu GUI pour couvrir les bords de l'écran qui sont transparents sur certaines résolutions
		float resX = (Screen.width + 2) / (GuiItemsDrawer.DesignResolution.x * correction.x);
		float resY = (Screen.height + 2) / (GuiItemsDrawer.DesignResolution.y * correction.y);

		GUI.matrix = Matrix4x4.TRS(new Vector3(-1f, -1f, 0f), Quaternion.identity, new Vector3(resX, resY, 1f));


		// Calcul de la zone
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


		// Appeler GuiItemsScriptedInterface si l'objet contient le component
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
		/// GuiItems qui a créé ce GuiItem.
		/// </summary>
		public GuiItemsCollection guiItems;


		/// <summary>
		/// Type d'objet GUI.
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
		/// Type de cet objet.
		/// </summary>
		public itemType thisItemType = itemType.LABEL;


		/// <summary>
		/// Est-ce que ce GuiItem est affiché ?
		/// </summary>
		public bool enabled = true;


		/// <summary>
		/// Est-ce que ce GuiItem est activé ? En gros, définit si cet élément est interactif (il reste affiché)
		/// </summary>
		public bool activated = true;


		/// <summary>
		/// Tag de cet objet, pour que le GuiItemsInterfaceBase puisse connaître associer un nom aux valeurs de sa liste de GuiItemReturnValue.
		/// </summary>
		public string tag = "";


		/// <summary>
		/// Couleur de cet objet.
		/// </summary>
		public Color color = Color.white;


		/// <summary>
		/// Nom de l'évènement à traiter lorsque le bouton s'active.
		/// </summary>
		public string eventToLaunch = "";


		/// <summary>
		/// Paramètre à envoyer.
		/// </summary>
		public string parameter = "";


		/// <summary>
		/// Rotation à appliquer sur cet élément ainsi que les suivants.
		/// </summary>
		public float rotationAngle = 0f;
		public GuiItem.Vector2Extension pivotPoint = new Vector2Extension();


		/// <summary>
		/// Est-ce que ce GuiItem doit boucler ?
		/// </summary>
		public bool loop = false;


		/// <summary>
		/// Décalage du bouclage en pixels.
		/// </summary>
		public float loopOffset = 0f;


		/// <summary>
		/// Vitesse de déplacement.
		/// </summary>
		public float loopScrollSpeed = 0f;


		/// <summary>
		/// Rect réel du contenu du label à boucler.
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
		/// Indique si la variable loopLabelRect a été définie.
		/// </summary>
		private bool loopLabelRectDefined = false;


		/// <summary>
		/// Dernier rectangle contenant l'élément.
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
		/// </summary>
		public List<GUIContent> contents = new List<GUIContent>();


		/// <summary>
		/// Version Array de contents
		/// </summary>
		public GUIContent[] contentsArray
		{
			get
			{
				return contents.ToArray();
			}
		}


		/// <summary>
		/// Premier contenu de ce GuiItem, utilisé par les éléments qui n'utilisent qu'un seul GUIContent.
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
		/// Caractère de masque pour le mot de passe. La version char est celle utilisée par l'élement final, la version string est utilisée par l'éditeur.
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
		/// Longueur maximale du texte du textField et autres.
		/// </summary>
		public int maxLength = -1;


		/// <summary>
		/// Valeur du Toggle.
		/// </summary>
		public bool toggle = false;


		/// <summary>
		/// Valeur retournée par Toolbar, entre autres.
		/// </summary>
		public int selected = 0;


		/// <summary>
		/// Nombre d'objets dans une ligne de SelectionGrid.
		/// </summary>
		public int xCount = 3;


		/// <summary>
		/// Espacement utilisé par GuiLayout.Space().
		/// </summary>
		public float pixels = 0f;


		/// <summary>
		/// Position du scroll view.
		/// </summary>
		public Vector2 scrollViewPosition = new Vector2();


		/// <summary>
		/// Toujours montrer la barre horizontale du scrollview ?
		/// </summary>
		public bool alwaysShowHorizontal = false;


		/// <summary>
		/// Toujours montrer la barre verticale du scrollview ?
		/// </summary>
		public bool alwaysShowVertical = false;


		/// <summary>
		/// Style de la barre horizontale du scrollview.
		/// </summary>
		public GuiStyleElement horizontalScrollbarStyle = null;


		/// <summary>
		/// Style de la barre verticale du scrollview.
		/// </summary>
		public GuiStyleElement verticalScrollbarStyle = null;


		/// <summary>
		/// Style du fond du scrollview.
		/// </summary>
		public GuiStyleElement backgroundStyle = null;


		/// <summary>
		/// Modes de style de ce GuiItem.
		/// </summary>
		public enum guiStyleMode
		{
			/// <summary>
			/// Force le style sur le style par défaut.
			/// </summary>
			DEFAULT,


			/// <summary>
			/// Reprend le fil des styles utilisés précédemment.
			/// </summary>
			NO_STYLE,


			/// <summary>
			/// Utilise le style spécifié par un GuiStyleElement.
			/// </summary>
			GUI_STYLE_ELEMENT,


			/// <summary>
			/// Permet de copier et personnaliser le style utilisé actuellement.
			/// </summary>
			CUSTOM_STYLE
		}


		/// <summary>
		/// Mode de style utilisé actuellement.
		/// </summary>
		public guiStyleMode thisGuiStyleMode = guiStyleMode.DEFAULT;


		/// <summary>
		/// Référence à un objet guiStyleElement qui stocke un GUIStyle séparément de ce GuiItems.
		/// </summary>
		public GuiStyleElement guiStyleElement = null;


		/// <summary>
		/// Style personnalisé de cet objet (GUIStyleExtension).
		/// </summary>
		public GuiItems.GUIStyleExtension customStyle = new GuiItems.GUIStyleExtension();


		/// <summary>
		/// Accès direct au style personnalisé de cet objet.
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
		/// GUISkin pour ScrollView (le GUIStyle ne fonctionne pas directement sur les BeginScrollView())?
		/// </summary>
		public GUISkin guiSkin;


		/// <summary>
		/// Est-ce que la souris est sur cet élément ?
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
					if(guiItems.guiItemsInterface)
						guiItems.guiItemsInterface.OnHover(this);
				}

				_hover = value;
			}
		}


		//#if UNITY_EDITOR
		/// <summary>
		/// Variable pour l'inspecteur pour indiquer si cet élément doit être affiché.
		/// </summary>
		public bool editor_folded = false;


		/// <summary>
		/// Variable pour l'inspecteur pour indiquer si les contenus de cet élément sont repliés.
		/// </summary>
		public bool editor_contentsFolded = false;


		/// <summary>
		/// Indique si cet objet affiche les paramètres de style ou pas.
		/// </summary>
		public bool editor_styleFolded = false;
		//#endif


		/// <summary>
		/// Constructeur de ce GuiItem. Indiquer le GuiItems qui a créé cet objet
		/// </summary>
		public GuiItem(GuiItemsCollection _guiItems)
		{
			contents.Add(new GUIContent());
			guiItems = _guiItems;
		}


		/// <summary>
		/// Copie du GuiItem indiqué.
		/// </summary>
		/// <param name="original"></param>
		public GuiItem(GuiItemsCollection.GuiItem original)
		{
			this.guiItems = original.guiItems;
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
			this.loopLabelRectDefined = false; // Garder à false pour forcer à se régénérer
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
			this.customStyle = new GuiItems.GUIStyleExtension(original.customStyle);
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
				// La rotation se fait toujours autour du point dans le Current Screen Space, pas le Standard
				GUIUtility.RotateAroundPivot(rotationAngle, pivotPoint.CurrentSpaceValue);
			}


			// Désactiver le GUI si l'élément n'est pas activé
			if(!activated)
				GUI.enabled = false;

			// Sauvegarder la couleur du GUI actuelle
			Color colorSave = GUI.color;


			// Changer la couleur du GUI
			GUI.color = color;


			switch(thisItemType)
			{
				case itemType.LABEL:
					if(loop)
					{
						// Faire un label vide pour obtenir les limites de l'objet
						if(thisGuiStyleMode == guiStyleMode.DEFAULT)
						{
							// Obtenir la taille de l'élément réel
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
								// Obtenir la taille de l'élément réel
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
								// Obtenir la taille de l'élément réel
								if(!loopLabelRectDefined)
								{
									loopLabelRect = GUILayoutUtility.GetRect(content, GUI.skin.label);
								}


								GUIStyle g = new GUIStyle(GUI.skin.label);
								g.stretchWidth = true;
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);
								GUILayout.Label("", g, GUILayout.MinWidth(1f));
							}
						}
						else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
						{
							// Obtenir la taille de l'élément réel
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
							// Obtenir la taille de l'élément réel
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
								Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);
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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);
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
						if(guiItems.guiItemsInterface == null)
							Debug.LogError("Cannot send event. No interface found. You have to add a GuiItemsInterface script on this game object.");
						else
							guiItems.guiItemsInterface.ReceiveEvent(this);
					}
					break;

				case itemType.REPEAT_BUTTON:
					if(DisplayRepeatButton())
					{
						if(guiItems.guiItemsInterface == null)
							Debug.LogError("Cannot send event. No interface found. You have to add a GuiItemsInterface script on this game object.");
						else
							guiItems.guiItemsInterface.ReceiveEvent(this);
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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
							Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);

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
				/*
				case itemType.BEGIN_SCROLL_VIEW:
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

				case itemType.END_SCROLL_VIEW:
					GUILayout.EndScrollView();
					break;
			}

			if((thisItemType != itemType.BEGIN_HORIZONTAL) && (thisItemType != itemType.BEGIN_VERTICAL) && (thisItemType != itemType.BEGIN_SCROLL_VIEW))
			{
				// Attendre Repaint pour vérifier les positionnements
				if(Event.current.type == EventType.Repaint)
				{
					lastRect = GUILayoutUtility.GetLastRect();

					hover = lastRect.Contains(Event.current.mousePosition);


					// Si cet élément est un label qui boucle
					if(thisItemType == itemType.LABEL && loop && lastRect.width != 0f)
					{
						loopLabelRectDefined = true;


						// Ajouter des éléments à gauche
						Rect r = new Rect(lastRect);

						GUI.BeginGroup(r);

						r.x = 0f;
						r.y = 0f;
						r.width = loopLabelRect.width;
						r.height = loopLabelRect.height;


						// Décaler le Rect en fonction de offset
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


							// Déplacer la copie de sa longueur
							r.x -= loopLabelRect.width;
						}


						// Ajouter des éléments à droite
						r = new Rect(lastRect);

						r.x = loopLabelRect.width;
						r.y = 0f;
						r.width = loopLabelRect.width;
						r.height = loopLabelRect.height;


						// Décaler le Rect en fonction de offset
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


							// Déplacer la copie de sa longueur
							r.x += loopLabelRect.width;
						}


						GUI.EndGroup();
					}
				}
			}


			// Redéfinir la couleur du GUI comme c'était avant
			GUI.color = colorSave;


			// Réactiver le GUI pour les éléments suivants s'il était désactivé
			if(!GUI.enabled)
				GUI.enabled = true;
		}


		/// <summary>
		/// Affiche un bouton normal dans le jeu.
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
					Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);
					return GUILayout.Button(content);
				}
			else if(thisGuiStyleMode == guiStyleMode.CUSTOM_STYLE)
				return GUILayout.Button(content, CustomStyle);
			else
				return GUILayout.Button(content);
		}


		/// <summary>
		/// Affiche un bouton répéteur dans le jeu.
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
					Debug.LogWarning(ErrMess.noGuiStyleElementDefined, guiItems);
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
		/// Message : Un GuiStyleElement n'a pas été défini pour un GuiItem dans cet objet alors qu'il en a besoin ; aucun style n'est donc appliqué. Pour ne plus voir ce message, veuillez définir Gui Style Element ou bien passer le mode de style à NO_STYLE (pour ce dernier, l'apparence ne changera pas par rapport à ce que vous voyez actuellement).
		/// </summary>
		static public string noGuiStyleElementDefined = "Un GuiStyleElement n'a pas été défini pour un GuiItem dans cet objet alors qu'il en a besoin ; aucun style n'est donc appliqué. Pour ne plus voir ce message, veuillez définir Gui Style Element ou bien passer le mode de style à NO_STYLE (pour ce dernier, l'apparence ne changera pas par rapport à ce que vous voyez actuellement).";
	}
}
