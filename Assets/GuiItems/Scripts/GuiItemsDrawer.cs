using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("GuiItems/GuiItemsDrawer")]
public class GuiItemsDrawer : MonoBehaviour
{
	/// <summary>
	/// Le GUISkin appliqué par défaut aux éléments de ce GuiItemsDrawer.
	/// </summary>
	public GUISkin thisGuiSkin;


	/// <summary>
	/// Résolution idéale de l'interface.
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
	/// Cette valeur sert, entre autres, à positionner un élément d'interface indépendant
	/// à partir d'une position obtenue via Camera.main.WorldToScreenPoint() par exemple.
	/// 
	/// Par exemple, j'ai la position — sur laquelle je veux positionner un Label en GUI — d'un objet ;
	/// position étant obtenue grâce à Camera.main.WorldToScreenPoint().
	/// Ensuite, je divise x et y du Vector3 obtenu afin d'avoir un vecteur aux valeurs relatives (0 -> 1).
	/// Enfin, je multiplie x par ScreenBaseWidth et y par ScreenBaseHeight => cela me donne la position
	/// corrigée et utilisable dans le cadre d'un affichage rectifié avec GuiItemsDrawer.
	/// 
	/// Il est également possible d'utiliser RelativeToPixels() pour convertir directement un Vector2.
	/// </summary>
	public static int GuiAreaWidth
	{
		get
		{
			return (int)(DesignResolution.x * FittingRatio.x);
		}
	}


	/// <summary>
	/// Cette valeur sert, entre autres, à positionner un élément d'interface indépendant
	/// à partir d'une position obtenue via Camera.main.WorldToScreenPoint() par exemple.
	/// 
	/// Par exemple, j'ai la position d'un objet obtenue grâce à Camera.main.WorldToScreenPoint().
	/// Ensuite, je divise x et y du Vector3 obtenu afin d'avoir un vecteur aux valeurs relatives (0 -> 1).
	/// Enfin, je multiplie x par ScreenBaseWidth et y par ScreenBaseHeight => cela me donne la position
	/// corrigée et utilisable dans le cadre d'un affichage rectifié avec GuiItemsDrawer.
	/// 
	/// Il est également possible d'utiliser RelativeToPixels() pour convertir directement un Vector2.
	/// </summary>
	public static int GuiAreaHeight
	{
		get
		{
			return (int)(DesignResolution.y * FittingRatio.y);
		}
	}


	/// <summary>
	/// Indique si la souris est sur n'importe quel morceau d'interface.
	/// </summary>
	public bool MouseOverGuiItems
	{
		get
		{
			return mouseOverGuiItems;
		}
	}


	/// <summary>
	/// Indique si la souris est sur une partie interactive de l'interface.
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
	/// Liste des GuiItems qui doivent s'afficher.
	/// </summary>
	public List<GuiItemsCollection> guiItemsOrder;


	/// <summary>
	/// Instance unique de cet objet.
	/// </summary>
	static public GuiItemsDrawer instance;

	void Awake()
	{
		mouseOverGuiItems = false;
		mouseOverInteractiveGuiItem = false;


		// Initialiser la liste
		guiItemsOrder = new List<GuiItemsCollection>();

		DontDestroyOnLoad(gameObject);
	}

	void OnEnable()
	{
		instance = this;
	}

	void OnGUI()
	{
		// Nettoyer la liste
		guiItemsOrder.RemoveAll(item => item == null);


		// Sauvegarder la matrice du GUI
		Matrix4x4 matrixBackup = GUI.matrix;


		// Utiliser le skin s'il est défini dans thisGuiSkin
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


		// Remettre la matrice comme elle était avant
		GUI.matrix = matrixBackup;


		// Chercher si la souris est sur un élément d'interface
		mouseOverGuiItems = false;
		mouseOverInteractiveGuiItem = false;


		// S'il n'y a aucun élément, alors la souris ne peut pas être sur un élément d'interface
		if(guiItemsOrder.Count != 0)
		{
			foreach(GuiItemsCollection g in guiItemsOrder)
			{
				// Si l'objet est désactivé, ne pas le prendre en compte
				if(!g.draw)
					continue;

				foreach(GuiItemsCollection.GuiItem item in g.items)
				{
					// Si l'élément est désactivé, ne pas le prendre en compte
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
			
			
			// Chercher si la souris est sur un objet interactif
			foreach(GuiItemsCollection g in guiItemsOrder)
			{
				// Si l'objet est désactivé, ne pas le prendre en compte
				if(!g.draw)
					continue;
				
				
				foreach(GuiItemsCollection.GuiItem item in g.items)
				{
					// Si l'élément est désactivé, ne pas le prendre en compte
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


		// Remettre le skin par défaut si thisGuiSkin a été défini
		if(defaultSkin)
		{
			GUI.skin = defaultSkin;
		}
	}

	void Update()
	{
		// Nettoyer la liste
		guiItemsOrder.RemoveAll(item => item == null);


		foreach(GuiItemsCollection g in guiItemsOrder)
		{
			if(g.enabled)
			{
				foreach(GuiItemsCollection.GuiItem gi in g.items)
				{
					if(gi.loop)
					{
						// Anime les labels qui bouclent
						gi.loopOffset += gi.loopScrollSpeed * Time.deltaTime;


						// Limiter loopOffset pour limiter le nombre de copies
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
			// Référencer ce GuiItems dans la liste, du plus grand depth au plus petit
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
	/// Valeurs permettant de mettre à l'échelle les éléments (positions et tailles)
	/// de l'espace de la résolution design vers l'espace de la résolution actuelle.
	/// </summary>
	static public Vector2 FittingRatio
	{
		get
		{
			// Ratio permet de savoir si l'écran est "horizontally-driven" ou "vertically-driven"
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
	/// Convertit un Rect avec des valeurs relatives en valeurs absolues (pixels)
	/// exploitables dans le mode d'affichage de GuiItemsDrawer.
	/// </summary>
	/// <param name="_relativeRect">Rect relatif</param>
	/// <returns>Rect absolu</returns>
	static public Rect RelativeToPixels(Rect _relativeRect)
	{
		_relativeRect.width *= GuiItemsDrawer.GuiAreaWidth;
		_relativeRect.height *= GuiItemsDrawer.GuiAreaHeight;
		_relativeRect.x *= GuiItemsDrawer.GuiAreaWidth;
		_relativeRect.y *= GuiItemsDrawer.GuiAreaHeight;

		return _relativeRect;
	}


	/// <summary>
	/// Convertit un Vector2 avec des valeurs relatives en valeurs absolues (pixels)
	/// exploitables dans le mode d'affichage de GuiItemsDrawer.
	/// </summary>
	/// <param name="relativeValue">Vector2 relatif (valeurs entre 0 et 1, généralement)</param>
	/// <returns>Vector2 absolu prêt pour utilisation dans le mode d'affichage de GuiItemsDrawer.</returns>
	static public Vector2 RelativeToPixels(Vector2 _relativeVector2)
	{
		_relativeVector2.x *= GuiItemsDrawer.GuiAreaWidth;
		_relativeVector2.y *= GuiItemsDrawer.GuiAreaHeight;

		return _relativeVector2;
	}
}
