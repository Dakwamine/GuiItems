using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GuiItemsCollection))]
public class GuiItemsCollectionEditor : Editor
{
	/// <summary>
	/// Name of the control that will unsets the control focus.
	/// </summary>
	public string focusUnset = "focusUnset";


	/// <summary>
	/// Accès rapide à l'objet.
	/// </summary>
	GuiItemsCollection guiItems
	{
		get
		{
			return (GuiItemsCollection)target;
		}
	}


	public override void OnInspectorGUI()
	{
		GUI.SetNextControlName(focusUnset);
		GUI.TextField(new Rect(-1f, -1f, 0f, 0f), "");

		bool guiChangedSave = GUI.changed;

		serializedObject.Update();

		SerializedProperty sp;


		// Permet à l'éditeur de savoir s'il faut enregistrer le guiItems sur le disque après un changement
		GUI.changed = false;


		// Indiquer à l'utilisateur qu'aucun eventReceiver n'a été associé à ce GuiItems
		if(!guiItems.GetComponent<GuiItemsInterfaceBase>())
		{
			EditorGUILayout.HelpBox("Aucun récepteur d'évènements détecté pour ce GuiItems. Si vous n'ajoutez pas un component dont le type dérive de GuiItemsInterfaceBase, aucun évènement ne sera transmis hors de ce GuiItems.", MessageType.Info);
		}


		// Booléen pour savoir s'il faut afficher la collection dans le jeu
		EditorGUILayout.PropertyField(serializedObject.FindProperty("draw"), new GUIContent("Draw", "Does this collection have to show up ingame?"));


		// Rectangle de zone de ce GuiItems
		sp = serializedObject.FindProperty("useArea");
		EditorGUILayout.PropertyField(sp, new GUIContent("Use Area", "Begin a GUILayout block of GUI controls in a fixed screen area."));
		bool guiEnabledSave = GUI.enabled;

		if(!sp.boolValue)
			GUI.enabled = false;
		else
			GUI.enabled = guiEnabledSave;


		// Positionnement par le transform de guiItems ?
		sp = serializedObject.FindProperty("useTransformPosition");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(sp, new GUIContent("Use Transform.position", "Does this collection uses Transform.position to position on screen? If so, values have to be % to the current screen : x=0f means left border, x=1f means right border..."));
		EditorGUI.indentLevel--;


		// Booléen pour empêcher l'utilisation des champs X et Y si on utilise Transform.position
		bool useTransformPosition = sp.boolValue;

		GuiItemsGUILayoutExtension.RectExtensionField(serializedObject.FindProperty("area"), GUI.enabled ? !useTransformPosition : false, GUI.enabled ? !useTransformPosition : false, GUI.enabled, GUI.enabled);

		GUI.enabled = guiEnabledSave;


		// Z depth
		EditorGUILayout.PropertyField(serializedObject.FindProperty("depth"), new GUIContent("Depth", "Set this to determine ordering when you have different scripts running simultaneously. GUI elements drawn with lower depth values will appear on top of elements with higher values (ie, you can think of the depth as \"distance\" from the camera)."));


		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical(GUI.skin.box);

		if(GuiItemsGUILayoutExtension.FoldoutButtonProperty(serializedObject.FindProperty("editor_showItemsList"), new GUIContent("Items of this collection")))
		{
			SerializedProperty itemsProperty = serializedObject.FindProperty("items");


			// Boutons pour ajouter un élément
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();

			GUI.enabled = false;
			GUILayout.Button(new GUIContent("Copy above", "Copier l'élément au dessus"), EditorStyles.miniButtonLeft);
			GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("+", "Ajouter un élément ici"), EditorStyles.miniButtonMid))
			{
				AddRequest.RequestId(0);
			}

			if(guiItems.items.Count == 0)
				GUI.enabled = false;
			if(GUILayout.Button(new GUIContent("Copy below", "Copier l'élément en dessous"), EditorStyles.miniButtonRight))
			{
				CopyRequest.RequestId(0, 0);
			}

			GUI.enabled = guiEnabledSave;

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();


			// Afficher les sous-éléments GuiItem
			int prevIdentLevel = EditorGUI.indentLevel;


			// Iterate over all child properties of array
			SerializedProperty property = itemsProperty.FindPropertyRelative("Array.size");

			int arraySize = property.intValue;

			int propertyStartingDepth = property.depth;
			int index = 0;

			while(property.NextVisible(false) && property.depth == propertyStartingDepth)
			{
				// Dessiner l'inspecteur du GuiItem
				GuiItemInspector.Draw(property, guiItems.items[index], index);


				// Boutons pour ajouter un élément
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.Space();

					if(GUILayout.Button(new GUIContent("Copy above", "Copier l'élément au dessus"), EditorStyles.miniButtonLeft))
					{
						CopyRequest.RequestId(index + 1, index);
					}

					if(GUILayout.Button(new GUIContent("+", "Ajouter un élément ici"), EditorStyles.miniButtonMid))
					{
						AddRequest.RequestId(index + 1);
					}

					if(index == arraySize - 1)
						GUI.enabled = false;
					if(GUILayout.Button(new GUIContent("Copy below", "Copier l'élément en dessous"), EditorStyles.miniButtonRight))
					{
						CopyRequest.RequestId(index + 1, index + 1);
					}

					GUI.enabled = guiEnabledSave;

					EditorGUILayout.Space();
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.Space();

				index++;
			}

			EditorGUI.indentLevel = prevIdentLevel;

			{
				// Ajouter un GuiItemsCollection.GuiItem dans ce GuiItems si requis
				int id = AddRequest.GetId();
				if(id != -1)
				{
					//guiItems.items.Insert(id, new GuiItemsCollection.GuiItem(guiItems));

					if(id < itemsProperty.arraySize)
						itemsProperty.InsertArrayElementAtIndex(id);
					else
						itemsProperty.arraySize++;
					ResetGuiItem(itemsProperty.GetArrayElementAtIndex(id));

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Retirer un GuiItemsCollection.GuiItem dans ce GuiItems si requis
				int id = RemoveRequest.GetId();
				if(id != -1)
				{
					//guiItems.items.Remove(guiItems.items[id]);
					itemsProperty.DeleteArrayElementAtIndex(id);

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Intervertir des GuiItemsCollection.GuiItem dans ce GuiItems si requis
				int[] id = MoveRequest.GetIds();
				if(id.Length != 0)
				{
					//GuiItemsCollection.GuiItem originalItem = guiItems.items[id[0]];

					//guiItems.items.RemoveAt(id[0]);
					//guiItems.items.Insert(id[1], originalItem);

					itemsProperty.MoveArrayElement(id[0], id[1]);

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Copier un GuiItemsCollection.GuiItem dans ce GuiItems si requis
				int idToCopy;
				int destinationId = CopyRequest.GetId(out idToCopy);
				if(destinationId != -1)
				{
					//guiItems.items.Insert(id, new GuiItemsCollection.GuiItem(guiItems.items[idToCopy]));
					itemsProperty.InsertArrayElementAtIndex(idToCopy);
					//GuiItemsCollectionEditor.ResetGuiItemSystemOnly(itemsProperty.GetArrayElementAtIndex(idToCopy));
					itemsProperty.MoveArrayElement(idToCopy, destinationId);
					GUI.FocusControl(focusUnset);
				}
			}
		}

		EditorGUILayout.EndVertical();


		// Si un seul élément d'interface a été modifié
		if(GUI.changed)
		{
			// Mettre à jour toutes les références de GuiItems dans tous les GuiItem
			/*foreach(GuiItemsCollection.GuiItem g in guiItems.items)
			{
				g.guiItems = guiItems;
			}*/


			// Mettre à jour / Enregistrer sur le disque dur ce guiItems
			//EditorUtility.SetDirty(guiItems);
			//Debug.Log("changed");
		}
		/*foreach(GuiItemsCollection.GuiItem g in guiItems.items)
		{
			g.color = Color.white;
		}*/

		serializedObject.ApplyModifiedProperties();

		GUI.changed = guiChangedSave;
	}



	/*void OnEnable()
	{
		//serializedObject.Update();
		//Debug.Log("Collection enable");
		//foreach(GuiItemsCollection.GuiItem g in guiItems.items)
		//{
		//serializedObject.FindProperty(
		//g.newPivotPoint = ScriptableObject.CreateInstance<GuiItems.Vector2Extension>();
		//g.newPivotPoint = new GuiItems.Vector2Extension();
		//	g.newPivotPoint = new GuiItemsCollection.GuiItem.Vector2Extension2();
		//	g.newPivotPoint.x.Value = g.newPivotPointLOZ.x.Value;
		//}

		foreach(GuiItemsCollection.GuiItem g in guiItems.items)
		{
			if(g.contents.Count == 0)
			{
				g.contents.Add(new GUIContent());
			}
		}

		EditorUtility.SetDirty(guiItems);
		//serializedObject.ApplyModifiedProperties();
	}*/


	/// <summary>
	/// Resets GuiItem's properties.
	/// </summary>
	/// <param name="_guiItem"></param>
	void ResetGuiItem(SerializedProperty _guiItemProperty)
	{
		_guiItemProperty.FindPropertyRelative("guiItems").objectReferenceValue = guiItems;
		_guiItemProperty.FindPropertyRelative("thisItemType").enumValueIndex = 0;
		_guiItemProperty.FindPropertyRelative("enabled").boolValue = true;
		_guiItemProperty.FindPropertyRelative("activated").boolValue = true;
		_guiItemProperty.FindPropertyRelative("tag").stringValue = "";
		_guiItemProperty.FindPropertyRelative("color").colorValue = Color.white;
		_guiItemProperty.FindPropertyRelative("eventToLaunch").stringValue = "";
		_guiItemProperty.FindPropertyRelative("parameter").stringValue = "";
		_guiItemProperty.FindPropertyRelative("rotationAngle").floatValue = 0f;
		_guiItemProperty.FindPropertyRelative("pivotPoint.x.value").floatValue = 0f;
		_guiItemProperty.FindPropertyRelative("pivotPoint.y.value").floatValue = 0f;
		_guiItemProperty.FindPropertyRelative("loop").boolValue = false;
		_guiItemProperty.FindPropertyRelative("loopOffset").floatValue = 0f;
		_guiItemProperty.FindPropertyRelative("loopScrollSpeed").floatValue = 0f;
		//_guiItemProperty.FindPropertyRelative("loopLabelRect").rectValue = new Rect();
		//_guiItemProperty.FindPropertyRelative("loopLabelRectDefined").boolValue = false;
		//_guiItemProperty.FindPropertyRelative("lastRect").rectValue = new Rect();
		_guiItemProperty.FindPropertyRelative("contents").ClearArray();
		_guiItemProperty.FindPropertyRelative("contents").arraySize++;
		_guiItemProperty.FindPropertyRelative("maskChar").stringValue = "*";
		_guiItemProperty.FindPropertyRelative("maxLength").intValue = -1;
		_guiItemProperty.FindPropertyRelative("toggle").boolValue = false;
		_guiItemProperty.FindPropertyRelative("selected").intValue = 0;
		_guiItemProperty.FindPropertyRelative("xCount").intValue = 3;
		_guiItemProperty.FindPropertyRelative("pixels").floatValue = 0f;
		_guiItemProperty.FindPropertyRelative("thisGuiStyleMode").enumValueIndex = 0;
		_guiItemProperty.FindPropertyRelative("guiStyleElement").objectReferenceValue = null;
		GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _guiItemProperty.FindPropertyRelative("customStyle.guiStyle"));
		//_guiItemProperty.FindPropertyRelative("_hover").boolValue = false;

		_guiItemProperty.FindPropertyRelative("editor_folded").boolValue = false;
		_guiItemProperty.FindPropertyRelative("editor_contentsFolded").boolValue = false;
		_guiItemProperty.FindPropertyRelative("editor_styleFolded").boolValue = false;
	}
}


/// <summary>
/// Inspecteur d'un GuiItemsCollection.GuiItem.
/// </summary>
static public class GuiItemInspector
{
	/// <summary>
	/// Indique s'il est possible de référencer des objets de la scène.
	/// Les GuiItemsCollection.GuiItem ne peuvent référencer des objets de scène que s'ils sont dans la scène.
	/// </summary>
	/// <param name="guiItems">L'objet GuiItems auquel cet inspecteur appartient.</param>
	/// <returns></returns>
	static private bool AllowSceneObjects(GuiItemsCollection guiItems)
	{
		return EditorUtility.IsPersistent(guiItems) ? false : true;
	}


	/// <summary>
	/// Dessine un GuiItemsCollection.GuiItem dans l'inspecteur.
	/// </summary>
	/// <param name="_itemProperty">Le property de l'élément que représente cet inspecteur.</param>
	/// <param name="guiItem">L'élément que représente cet inspecteur.</param>
	/// <param name="id">L'id de cet élément dans son conteneur GuiItems.</param>
	static public void Draw(SerializedProperty _itemProperty, GuiItemsCollection.GuiItem guiItem, int _id)
	{
		bool guiEnabledSave = GUI.enabled;

		Color c = GUI.color;
		GUI.color = CustomStyles.BACKGROUND_COLOR_BLUE;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		GUI.color = c;


		// Barre avec ses boutons
		EditorGUILayout.BeginHorizontal();
		{
			// Property de la variable qui indique si cet élément doit être déplié (visible)
			SerializedProperty editor_foldedProperty = _itemProperty.FindPropertyRelative("editor_folded");


			// On utilise un Foldout personnalisé parce qu'il est géré par un booléen
			bool foldTemp = GuiItemsGUILayoutExtension.FoldoutButton(guiItem.editor_folded, new GUIContent(guiItem.thisItemType.ToString() + (guiItem.tag != "" ? ":" + guiItem.tag : "")));
			//bool foldTemp = EditorGUILayout.Foldout(guiItem.editor_folded, new GUIContent(guiItem.thisItemType.ToString() + (guiItem.tag != "" ? ":" + guiItem.tag : "")));


			// Enregistrer la valeur editor_folded
			if(foldTemp != guiItem.editor_folded)
				editor_foldedProperty.boolValue = foldTemp;


			// Property de la liste qui contient les éléments
			SerializedProperty itemsArrayProperty = _itemProperty.serializedObject.FindProperty("items");


			// Boutons pour déplacer ou supprimer cet élément
			if(_id == 0)
				GUI.enabled = false;
			else
				GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("▲", "Monter cet élément"), CustomStyles.miniButtonLeft))
			{
				MoveRequest.RequestIds(_id, _id - 1);
			}

			if(_id == itemsArrayProperty.arraySize - 1)
				GUI.enabled = false;
			else
				GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("▼", "Descendre cet élément"), CustomStyles.miniButtonMid))
			{
				MoveRequest.RequestIds(_id, _id + 1);
			}

			GUI.enabled = guiEnabledSave;


			if(GUILayout.Button(new GUIContent("X", "Supprimer"), CustomStyles.miniButtonRight))
			{
				RemoveRequest.RequestId(_id);
			}
		}
		EditorGUILayout.EndHorizontal();


		if(guiItem.editor_folded)
		{
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("thisItemType"), new GUIContent("Element Type"));
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("enabled"), new GUIContent("Enabled", "Est-ce que ce GuiItemsCollection.GuiItem est affiché ?"));
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("activated"), new GUIContent("Activated", "Est-ce que ce GuiItemsCollection.GuiItem est activé ? En gros, définit si la valeur de cet élément peut être modifié ingame (il reste affiché)."));


			// Rotation // Désactivé pour le moment car pas robuste comme système
			//guiItem.rotationAngle = EditorGUILayout.FloatField(new GUIContent("Rotation Angle", "Rotation applying on this element and all next elements. You don't need to reset it at the end."), guiItem.rotationAngle);
			//guiItem.pivotPoint = EditorGUILayout.Vector2Field("Rotation pivot point", guiItem.pivotPoint);


			// Tag de ce GuiItemsCollection.GuiItem afin que GuiItemsInterfaceBase puisse retrouver les valeurs de cet élément
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("tag"), new GUIContent("Tag", "Tag de ce GuiItemsCollection.GuiItem afin que GuiItemsInterfaceBase puisse retrouver les valeurs de cet élément."));
			
			
			// Couleur
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("color"), new GUIContent("Color", "Couleur utilisé par ce GuiItemsCollection.GuiItem. La couleur appliquée est relative à celle qui est employée globalement dans la collection."));


			switch((GuiItemsCollection.GuiItem.itemType)_itemProperty.FindPropertyRelative("thisItemType").enumValueIndex)
			{
				case GuiItemsCollection.GuiItem.itemType.LABEL:
					// Loop
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("loop"), new GUIContent("Loop", "Does this label loop?"));
					if(guiItem.loop)
					{
						EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("loopOffset"), new GUIContent("Loop Offset", "Offset of the loop."));
						EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("loopScrollSpeed"), new GUIContent("Scroll Speed", "Scroll speed of this loop element."));
					}


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BOX:
					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BUTTON:
					// Nom de l'évènement à lancer si cet élément est activé
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("eventToLaunch"), new GUIContent("Event To Launch", "Nom de l'évènement à lancer si cet élément est activé."));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("parameter"), new GUIContent("Parameter", "Paramètre de l'évènement à utiliser si cet élément est activé."));


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.REPEAT_BUTTON:
					// Nom de l'évènement à lancer si cet élément est activé
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("eventToLaunch"), new GUIContent("Event To Launch", "Nom de l'évènement à lancer si cet élément est activé."));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("parameter"), new GUIContent("Parameter", "Paramètre de l'évènement à utiliser si cet élément est activé."));


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
					// Nombre de caractères maximum
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Nombre de caractères maximum. Mettre un chiffre négatif pour ne pas limiter la longueur."));


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
					// Caractère de masque
					/*if(_itemProperty.FindPropertyRelative("maskChar").stringValue == "")
						_itemProperty.FindPropertyRelative("maskChar").stringValue = "*";
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent("Mask Char", "Character to mask the password with."));
					_itemProperty.FindPropertyRelative("maskChar").stringValue = GUILayout.TextField(_itemProperty.FindPropertyRelative("maskChar").stringValue, 1);
					EditorGUILayout.EndHorizontal();*/
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maskChar"));


					// Nombre de caractères maximum
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Nombre de caractères maximum. Mettre un chiffre négatif pour ne pas limiter la longueur."));


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
					// Nombre de caractères maximum
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Nombre de caractères maximum. Mettre un chiffre négatif pour ne pas limiter la longueur."));


					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TOGGLE:
					// Dessiner l'inspecteur du GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
					// Dessiner l'inspecteur des GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
					// Nombre de colonnes dans le selection grid
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("xCount"), new GUIContent("xCount", "How many elements to fit in the horizontal direction. The elements will be scaled to fit unless the style defines a fixedWidth to use. The height of the control will be determined from the number of elements."));

					if(_itemProperty.FindPropertyRelative("xCount").intValue < 1)
						_itemProperty.FindPropertyRelative("xCount").intValue = 1;


					// Dessiner l'inspecteur des GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.SPACE:
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("pixels"), new GUIContent("Pixels", "Size of the space. Can be negative."));


					EditorGUILayout.HelpBox("The direction of the space is dependent on the layout group you're currently in when issuing the command. If in a vertical group, the space will be vertical. Note: This will override the GUILayout.ExpandWidth and GUILayout.ExpandHeight.", MessageType.Info);
					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_HORIZONTAL:
					EditorGUILayout.HelpBox("Le contenu ne peut être affiché que si le mode de style de cet élément est différent de NO_STYLE. De plus, les images et les textes ne pouvant s'afficher mutuellement, l'image est prioritaire sur le texte lorsqu'elle est définie.", MessageType.Info);


					// Dessiner l'inspecteur des GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_VERTICAL:
					EditorGUILayout.HelpBox("Le contenu ne peut être affiché que si le mode de style de cet élément est différent de NO_STYLE. De plus, les images et les textes ne pouvant s'afficher mutuellement, l'image est prioritaire sur le texte lorsqu'elle est définie.", MessageType.Info);


					// Dessiner l'inspecteur des GUIContent
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_SCROLL_VIEW:
					EditorGUILayout.HelpBox("Le BeginScrollView est un peu bugué (bug Unity non corrigé). On ne peut donc pas utiliser de GuiStyleElement pour le style des barres. Il faut définir le style des barres directement dans le skin (voir le skin indiqué dans GuiItemsDrawer).", MessageType.Info);

					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("alwaysShowHorizontal"));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("alwaysShowVertical"));


					// GUISkin
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("guiSkin"));
					// GUIStyle
					//DrawGuiStyleBlock(_itemProperty);

					break;
			}
		}

		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// Draws the block containing the style settings.
	/// </summary>
	/// <param name="_itemProperty">The serialized property of the element.</param>
	static private void DrawGuiStyleBlock(SerializedProperty _itemProperty)
	{
		Color c = GUI.color;
		GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		GUI.color = c;
		{
			//EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("editor_styleFolded"), new GUIContent("Style"));
			SerializedProperty editor_styleFoldedProperty = _itemProperty.FindPropertyRelative("editor_styleFolded");
			bool tmp = GuiItemsGUILayoutExtension.FoldoutButton(editor_styleFoldedProperty.boolValue, new GUIContent("Style"));

			if(tmp != editor_styleFoldedProperty.boolValue)
				editor_styleFoldedProperty.boolValue = tmp;

			if(tmp)
			{
				EditorGUILayout.Space();


				// Sélecteur de thisGuiStyleMode
				DrawStyleModeSelector(_itemProperty);


				// Inspecteur de style
				DrawGuiStyleInspector(_itemProperty);

				EditorGUILayout.Space();
			}
		}
		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// Dessine le sélecteur de thisGuiStyleMode avec les boutons Edit et Customize.
	/// </summary>
	/// <param name="guiItem">Le GuiItemsCollection.GuiItem auquel ce sélecteur appartient.</param>
	static private void DrawStyleModeSelector(SerializedProperty _itemProperty)
	{
		EditorGUILayout.BeginHorizontal();

		SerializedProperty thisGuiStyleModeProperty = _itemProperty.FindPropertyRelative("thisGuiStyleMode");

		EditorGUILayout.PropertyField(thisGuiStyleModeProperty);
		//guiItem.thisGuiStyleMode = (GuiItemsCollection.GuiItem.guiStyleMode)EditorGUILayout.EnumPopup(guiItem.thisGuiStyleMode);
		GUIStyle gs = GUI.skin.button;
		GUI.skin.button.font = EditorStyles.miniFont;


		// Bouton pour éditer le style actuel
		switch((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex)
		{
			case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
				if(GUILayout.Button(new GUIContent("Edit", "Éditer ce style"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false)))
				{
					// Sélectionner cet objet
					Selection.activeObject = _itemProperty.FindPropertyRelative("guiStyleElement").objectReferenceValue;
				}
				break;

			default:
				GUI.enabled = false;
				GUILayout.Button(new GUIContent("Edit", "Éditer ce style"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false));
				GUI.enabled = true;
				break;
		}


		// Bouton pour customiser le style actuel
		bool guiEnabledSave = GUI.enabled;

		if((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex == GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE
			|| (GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex == GuiItemsCollection.GuiItem.guiStyleMode.DEFAULT)
			GUI.enabled = false;
		else
			GUI.enabled = guiEnabledSave;


		if(GUILayout.Button(new GUIContent("Customize", "Personnaliser le style actuel. Seul ce GuiItemsCollection.GuiItem aura ce style."), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
		{
			switch((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex)
			{
				case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
					// Copier le style du guiStyleElement vers le CustomStyle
					GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(((GuiStyleElement)_itemProperty.FindPropertyRelative("guiStyleElement").objectReferenceValue).guiStyleExtension.guiStyle, _itemProperty.FindPropertyRelative("customStyle.guiStyle"));

					thisGuiStyleModeProperty.enumValueIndex = (int)GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE;

					break;

				case GuiItemsCollection.GuiItem.guiStyleMode.NO_STYLE:
					switch((GuiItemsCollection.GuiItem.itemType)_itemProperty.FindPropertyRelative("thisItemType").enumValueIndex)
					{
						case GuiItemsCollection.GuiItem.itemType.LABEL:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}

						case GuiItemsCollection.GuiItem.itemType.BOX:
						case GuiItemsCollection.GuiItem.itemType.BEGIN_HORIZONTAL:
						case GuiItemsCollection.GuiItem.itemType.BEGIN_VERTICAL:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}

						case GuiItemsCollection.GuiItem.itemType.BUTTON:
						case GuiItemsCollection.GuiItem.itemType.REPEAT_BUTTON:
						case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
						case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}

						case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
						case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}

						case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}

						case GuiItemsCollection.GuiItem.itemType.TOGGLE:
							{
								GuiItemsTools.GUIStyleExtensionEditor.CopyStyleTo(new GUIStyle(), _itemProperty.FindPropertyRelative("customStyle.guiStyle"));
								break;
							}
					}

					_itemProperty.FindPropertyRelative("thisGuiStyleMode").enumValueIndex = (int)GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE;


					break;
			}
		}
		GUI.enabled = guiEnabledSave;

		GUI.skin.button = gs;
		EditorGUILayout.EndHorizontal();
	}


	/// <summary>
	/// Dessine l'inspecteur de style du GuiItemsCollection.GuiItem en paramètre en fonction du mode de style appliqué sur ce dernier.
	/// </summary>
	/// <param name="guiItem">Le GuiItemsCollection.GuiItem auquel ce sélecteur appartient.</param>
	static private void DrawGuiStyleInspector(SerializedProperty _itemProperty)
	{
		switch((GuiItemsCollection.GuiItem.guiStyleMode)_itemProperty.FindPropertyRelative("thisGuiStyleMode").enumValueIndex)
		{
			case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
				EditorGUILayout.Space();
				//GameObject go = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Gui Style Element", "Game Object référençant le style à utiliser."), guiItem.guiStyleElement != null ? guiItem.guiStyleElement.gameObject : null, typeof(GameObject), AllowSceneObjects(guiItem.guiItems));
				EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("guiStyleElement"), new GUIContent("Gui Style Element", "Game Object référençant le style à utiliser."));

				break;

			case GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE:
				EditorGUILayout.Space();


				// Dessiner l'inspecteur de customStyle
				GuiStyleInspector.Draw(_itemProperty.FindPropertyRelative("customStyle"));
				break;
		}
	}
}


/// <summary>
/// Classe qui affiche le GUIContent dans l'inspecteur.
/// </summary>
public class GuiContentInspector
{
	/// <summary>
	/// Affiche un GUIContent dans l'inspecteur.
	/// </summary>
	/// <param name="_guiContentsArrayProperty">Le property de la liste des GUIContent</param>
	/// <param name="_guiItem">Le GuiItemsCollection.GuiItem dont ce GUIContent en est le contenu.</param>
	/// <param name="_drawTooltip">Est-ce qu'il faut dessiner le champ du tooltip ? (pour BEGIN_HORIZONTAL et BEGIN_VERTICAL).</param>
	static public void Draw(SerializedProperty _guiItemProperty, bool _drawTooltip = true)
	{
		// Liste des contenus de ce GuiItemsCollection.GuiItem à afficher
		SerializedProperty guiContentsArrayProperty = _guiItemProperty.FindPropertyRelative("contents");
		GuiItemsCollection.GuiItem.itemType itemType = (GuiItemsCollection.GuiItem.itemType)_guiItemProperty.FindPropertyRelative("thisItemType").enumValueIndex; //_guiItem.thisItemType;


		// Conteneur principal
		Color c = GUI.color;
		GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		GUI.color = c;
		{
			// Fold de tout les GUIContent ( != fold des éléments)
			SerializedProperty editor_contentsFoldedProperty = _guiItemProperty.FindPropertyRelative("editor_contentsFolded");

			bool fold = GuiItemsGUILayoutExtension.FoldoutButton(editor_contentsFoldedProperty.boolValue, new GUIContent("Content(s)", "Content(s) of this element."));


			// Enregistrer la valeur si besoin
			if(fold != editor_contentsFoldedProperty.boolValue)
				editor_contentsFoldedProperty.boolValue = fold;


			if(fold)
			{
				// Ne pas permettre l'ajout ou le déplacement si le type de ce GuiItemsCollection.GuiItem ne permet pas le multicontenus
				if((itemType == GuiItemsCollection.GuiItem.itemType.TOOLBAR) || (itemType == GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
				{
					// Bouton pour ajouter un GUIContent
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button(new GUIContent("+", "Ajouter un contenu ici"), CustomStyles.littleButton))
					{
						ContentAddRequest.RequestId(0);
					}
					EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
				}


				SerializedProperty contentProperty = guiContentsArrayProperty.FindPropertyRelative("Array.size");
				int startingDepth = contentProperty.depth;
				int index = 0;


				while(contentProperty.NextVisible(false) && contentProperty.depth == startingDepth)
				{
					// Horizontal de la barre de titre + boutons à droite
					c = GUI.color;
					GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
					EditorGUILayout.BeginVertical(GUI.skin.box);
					GUI.color = c;
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Content #" + index);


						// Ne pas permettre l'ajout ou le déplacement si le type de ce GuiItemsCollection.GuiItem ne permet pas le multicontenus
						if((itemType == GuiItemsCollection.GuiItem.itemType.TOOLBAR) || (itemType == GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
						{
							bool guiEnabledSave = GUI.enabled;

							if(index == 0)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("▲", "Monter ce contenu"), CustomStyles.miniButtonLeft))
							{
								ContentMoveRequest.RequestIds(index, index - 1);
							}


							if(index == guiContentsArrayProperty.arraySize - 1)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("▼", "Descendre ce contenu"), CustomStyles.miniButtonMid))
							{
								ContentMoveRequest.RequestIds(index, index + 1);
							}


							GUI.enabled = guiEnabledSave;
							if(GUILayout.Button(new GUIContent("Reset", "Reset this content"), CustomStyles.miniButtonMid))
							{
								GuiItemsTools.ResetPropertyValues(contentProperty);
							}


							// Il ne peut pas y avoir 0 contenu
							if(guiContentsArrayProperty.arraySize <= 1)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("X", "Supprimer"), CustomStyles.miniButtonRight))
							{
								ContentRemoveRequest.RequestId(index);
							}

							GUI.enabled = guiEnabledSave;
						}

						EditorGUILayout.EndHorizontal();


						// Texte du GUIContent
						EditorGUILayout.BeginHorizontal();
						{
							SerializedProperty text = contentProperty.FindPropertyRelative("m_Text");

							EditorGUILayout.PrefixLabel(new GUIContent("Text"));

							string tempInput = EditorGUILayout.TextArea(text.stringValue, GUILayout.MinHeight(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.MinWidth(0f));

							if(tempInput != text.stringValue)
								text.stringValue = tempInput;
						}
						EditorGUILayout.EndHorizontal();


						SerializedProperty image = contentProperty.FindPropertyRelative("m_Image");
						EditorGUILayout.PropertyField(image, new GUIContent("Image"));


						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.PrefixLabel(" ");
							if(image.objectReferenceValue == null)
							{
								EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
							}
							else
							{
								EditorGUILayout.LabelField(new GUIContent(((Texture2D)image.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
							}


							EditorGUILayout.BeginVertical();
							{
								if(image.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)image.objectReferenceValue).width + " x " + ((Texture2D)image.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)image.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)image.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUILayout.EndHorizontal();

						if(_drawTooltip)
						{
							EditorGUILayout.PropertyField(contentProperty.FindPropertyRelative("m_Tooltip"), new GUIContent("Tooltip", "The tooltip of this element."));
						}
					}
					EditorGUILayout.EndVertical();


					// Couper prématurément la boucle si le type de ce GuiItemsCollection.GuiItem ne permet pas le multicontenus
					if((itemType != GuiItemsCollection.GuiItem.itemType.TOOLBAR) && (itemType != GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
						break;


					// Bouton pour ajouter un GUIContent
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button(new GUIContent("+", "Ajouter un contenu ici"), CustomStyles.littleButton))
					{
						ContentAddRequest.RequestId(index + 1);
					}
					EditorGUILayout.Space();
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();

					index++;
				}
			}
		}
		GUILayout.EndVertical();


		// Gérer les contents
		{
			// Ajouter un GUIContent dans ce contents si requis
			int newContentPositionId = ContentAddRequest.GetId();
			if(newContentPositionId != -1)
			{
				if(newContentPositionId != guiContentsArrayProperty.arraySize)
				{
					guiContentsArrayProperty.InsertArrayElementAtIndex(newContentPositionId);

					GuiItemsTools.ResetPropertyValues(guiContentsArrayProperty.GetArrayElementAtIndex(newContentPositionId));
				}
				else
				{
					guiContentsArrayProperty.arraySize++;
					GuiItemsTools.ResetPropertyValues(guiContentsArrayProperty.GetArrayElementAtIndex(guiContentsArrayProperty.arraySize - 1));
				}
			}
		}
		{
			// Retirer un GuiItemsCollection.GuiItem dans ce GuiItems si requis
			int contentToRemoveId = ContentRemoveRequest.GetId();
			if(contentToRemoveId != -1)
			{
				guiContentsArrayProperty.DeleteArrayElementAtIndex(contentToRemoveId);
			}
		}
		{
			// Intervertir des GuiItemsCollection.GuiItem dans ce GuiItems si requis
			int[] contentToMoveId = ContentMoveRequest.GetIds();
			if(contentToMoveId.Length != 0)
			{
				guiContentsArrayProperty.MoveArrayElement(contentToMoveId[0], contentToMoveId[1]);
			}
		}
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur d'ajouter un GuiItemsCollection.GuiItem dans le GuiItems
/// </summary>
static public class AddRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit l'id du GuiItemsCollection.GuiItem à ajouter.
	/// </summary>
	/// <param name="_id">Id que va prendre un nouveau GuiItemsCollection.GuiItem dans son conteneur GuiItems.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Obtenir l'Id enregistré. Retourne -1 si l'id n'a pas été défini.
	/// </summary>
	/// <returns></returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}

/// <summary>
/// Classe pour indiquer à l'éditeur de retirer un GuiItemsCollection.GuiItem dans le GuiItems
/// </summary>
static public class RemoveRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit l'id du GuiItemsCollection.GuiItem à détruire.
	/// </summary>
	/// <param name="_id">Id du GuiItemsCollection.GuiItem dans son conteneur GuiItems.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Obtenir l'Id enregistré. Retourne -1 si l'id n'a pas été défini.
	/// </summary>
	/// <returns></returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur de bouger un GuiItemsCollection.GuiItem dans le GuiItems
/// </summary>
static public class MoveRequest
{
	static private int id = 0;
	static private int idToReplace = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit les id des GuiItemsCollection.GuiItem à intervertir.
	/// </summary>
	/// <param name="_id">Id du GuiItemsCollection.GuiItem dans son conteneur GuiItems qui change de place.</param>
	/// <param name="_idToReplace">Id du GuiItemsCollection.GuiItem dans son conteneur GuiItems dont la place va être prise.</param>
	static public void RequestIds(int _id, int _idToReplace)
	{
		id = _id;
		idToReplace = _idToReplace;
		requested = true;
	}


	/// <summary>
	/// Obtenir les Ids enregistrés. Retourne un tableau vide si les ids n'ont pas été définis.
	/// </summary>
	/// <returns></returns>
	static public int[] GetIds()
	{
		if(!requested)
			return new int[0];

		requested = false;

		return new int[] { id, idToReplace };
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur d'ajouter un GuiItemsCollection.GuiItem dans le GuiItems
/// </summary>
static public class CopyRequest
{
	static private int id = 0;
	static private int idToCopy = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit l'id du GuiItemsCollection.GuiItem à copier et son futur id dans la liste.
	/// </summary>
	/// <param name="_id">Id que va prendre un nouveau GuiItemsCollection.GuiItem dans son conteneur GuiItems.</param>
	/// <param name="_idToCopy">Id que va prendre un nouveau GuiItemsCollection.GuiItem dans son conteneur GuiItems.</param>
	static public void RequestId(int _id, int _idToCopy)
	{
		id = _id;
		idToCopy = _idToCopy;
		requested = true;
	}


	/// <summary>
	/// Obtenir l'Id enregistré. Retourne -1 si l'id n'a pas été défini.
	/// </summary>
	/// <returns></returns>
	static public int GetId(out int _idToCopy)
	{
		_idToCopy = idToCopy;

		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur d'ajouter un GUIContent dans le contents
/// </summary>
static public class ContentAddRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit l'id du GUIContent à ajouter.
	/// </summary>
	/// <param name="_id">Id que va prendre un nouveau GUIContent dans son conteneur contents.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Obtenir l'Id enregistré. Retourne -1 si l'id n'a pas été défini.
	/// </summary>
	/// <returns></returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur de retirer un GUIContent dans le contents
/// </summary>
static public class ContentRemoveRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit l'id du GUIContent à détruire.
	/// </summary>
	/// <param name="_id">Id du GUIContent dans son conteneur contents.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Obtenir l'Id enregistré. Retourne -1 si l'id n'a pas été défini.
	/// </summary>
	/// <returns></returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Classe pour indiquer à l'éditeur de bouger un GUIContent dans le contents
/// </summary>
static public class ContentMoveRequest
{
	static private int id = 0;
	static private int idToReplace = 0;
	static private bool requested = false;


	/// <summary>
	/// Définit les id des GUIContent à intervertir.
	/// </summary>
	/// <param name="_id">Id du GUIContent dans son conteneur contents qui change de place.</param>
	/// <param name="_idToReplace">Id du GUIContent dans son conteneur contents dont la place va être prise.</param>
	static public void RequestIds(int _id, int _idToReplace)
	{
		id = _id;
		idToReplace = _idToReplace;
		requested = true;
	}


	/// <summary>
	/// Obtenir les Ids enregistrés. Retourne un tableau vide si les ids n'ont pas été définis.
	/// </summary>
	/// <returns></returns>
	static public int[] GetIds()
	{
		if(!requested)
			return new int[0];

		requested = false;

		return new int[] { id, idToReplace };
	}
}

static public class CustomStyles
{
	public const float LEFT_MARGIN = 9f;
	public const float TEXTURE_BOX_SIZE = 64f;
	public const float BACKGROUND_BOX_MIN_HEIGHT = 98f;
	public const float IMAGE_PROPERTIES_MIN_WIDTH = 50f;
	public static Color BACKGROUND_COLOR_BLUE = new Color(0.7f, 0.8f, 0.9f, 0.9f);
	public static Color BACKGROUND_COLOR_GREY = new Color(0.9f, 0.9f, 0.9f, 0.4f);

	static public GUIStyle littleButton
	{
		get
		{
			GUIStyle gs = new GUIStyle(GUI.skin.button);
			gs.alignment = TextAnchor.MiddleCenter;
			gs.padding = new RectOffset(0, 0, 0, 0);
			//gs.fixedHeight = 15f;
			gs.fixedWidth = 30f;
			gs.clipping = TextClipping.Overflow;
			gs.padding.right += 3;
			return gs;
		}
	}

	static public GUIStyle textFieldNoMargin
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.textField);
			gs.margin = new RectOffset(0, 0, 0, 0);
			gs.padding = new RectOffset(0, 0, 0, 0);
			return gs;
		}
	}

	static public GUIStyle foldoutNormal
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.foldout);
			gs.margin = new RectOffset(0, 0, 3, 0);
			return gs;
		}
	}

	static public GUIStyle foldoutActive
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.foldout);
			gs.normal.background = gs.onNormal.background;
			gs.normal.textColor = gs.onNormal.textColor;
			gs.hover.background = gs.onHover.background;
			gs.hover.textColor = gs.onHover.textColor;
			gs.active.background = gs.onActive.background;
			gs.active.textColor = gs.onActive.textColor;
			gs.focused.background = gs.onFocused.background;
			gs.focused.textColor = gs.onFocused.textColor;
			gs.margin = new RectOffset(0, 0, 3, 0);
			return gs;
		}
	}

	static public GUIStyle imagePreviewLabel
	{
		get
		{
			GUIStyle gs = new GUIStyle();
			gs.alignment = TextAnchor.MiddleCenter;
			gs.normal.background = EditorGUIUtility.FindTexture("../GuiItems/Textures/texturePreviewChecker.png");
			gs.font = EditorStyles.boldFont;
			gs.fontSize = 10;
			//gs.normal.textColor = Color.white;
			gs.padding = new RectOffset(0, 0, 0, 0);

			return gs;
		}
	}
	static public GUIStyle imagePropertiesLabel
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.miniLabel);
			gs.padding = new RectOffset(0, 0, 0, 0);
			return gs;
		}
	}
	static public GUIStyle miniButtonLeft
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.miniButtonLeft);
			gs.padding = new RectOffset(3, 6, 3, 3);
			gs.fixedWidth = 20f;
			return gs;
		}
	}
	static public GUIStyle miniButtonMid
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.miniButtonMid);
			gs.padding = new RectOffset(3, 6, 3, 3);
			gs.stretchWidth = false;
			return gs;
		}
	}
	static public GUIStyle miniButtonRight
	{
		get
		{
			GUIStyle gs = new GUIStyle(EditorStyles.miniButtonRight);
			gs.padding = new RectOffset(3, 6, 3, 3);
			gs.fixedWidth = 20f;
			return gs;
		}
	}
}