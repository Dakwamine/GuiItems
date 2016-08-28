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
	/// Quick access to target object.
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


		// Allows the editor to know if we need to save changes on disk
		GUI.changed = false;


		// Indicates to the user that no event receiver has been added to this Collection
		if(!guiItems.GetComponent<GuiItemsInterfaceBase>())
		{
			EditorGUILayout.HelpBox("No event receiver has been detected on this Collection. Please add a component inheriting from GuiItemsInterfaceBase to be able to process events from this Collection.", MessageType.Info);
		}


		// Boolean to know if we have to draw the collection while playing
		EditorGUILayout.PropertyField(serializedObject.FindProperty("draw"), new GUIContent("Draw", "Does this collection have to show up ingame?"));


		// Area rect of this Collection
		sp = serializedObject.FindProperty("useArea");
		EditorGUILayout.PropertyField(sp, new GUIContent("Use Area", "Begin a GUILayout block of GUI controls in a fixed screen area."));
		bool guiEnabledSave = GUI.enabled;

		if(!sp.boolValue)
			GUI.enabled = false;
		else
			GUI.enabled = guiEnabledSave;


		// Use transform.position to position this collection?
		sp = serializedObject.FindProperty("useTransformPosition");
		EditorGUI.indentLevel++;
		EditorGUILayout.PropertyField(sp, new GUIContent("Use Transform.position", "Does this collection uses Transform.position to position on screen? If so, values have to be % to the current screen : x=0f means left border, x=1f means right border..."));
		EditorGUI.indentLevel--;


		// Boolean which prevents X and Y fields
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


			// Buttons for adding a GuiItem in the Collection
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();

			GUI.enabled = false;
			GUILayout.Button(new GUIContent("Copy above", "Copy the above GuiItem"), EditorStyles.miniButtonLeft);
			GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("+", "Add a GuiItem here"), EditorStyles.miniButtonMid))
			{
				AddRequest.RequestId(0);
			}

			if(guiItems.items.Count == 0)
				GUI.enabled = false;
			if(GUILayout.Button(new GUIContent("Copy below", "Copy the below GuiItem"), EditorStyles.miniButtonRight))
			{
				CopyRequest.RequestId(0, 0);
			}

			GUI.enabled = guiEnabledSave;

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();


			// Prepare to display GuiItem in the inspector
			int prevIdentLevel = EditorGUI.indentLevel;


			// Iterate over all child properties of array
			SerializedProperty property = itemsProperty.FindPropertyRelative("Array.size");

			int arraySize = property.intValue;

			int propertyStartingDepth = property.depth;
			int index = 0;

			while(property.NextVisible(false) && property.depth == propertyStartingDepth)
			{
				// Draw the GuiItem inspector
				GuiItemInspector.Draw(property, guiItems.items[index], index);


				// Buttons for adding a GuiItem in the Collection
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.Space();

					if(GUILayout.Button(new GUIContent("Copy above", "Copy the above GuiItem"), EditorStyles.miniButtonLeft))
					{
						CopyRequest.RequestId(index + 1, index);
					}

					if(GUILayout.Button(new GUIContent("+", "Add a GuiItem here"), EditorStyles.miniButtonMid))
					{
						AddRequest.RequestId(index + 1);
					}

					if(index == arraySize - 1)
						GUI.enabled = false;
					if(GUILayout.Button(new GUIContent("Copy below", "Copy the below GuiItem"), EditorStyles.miniButtonRight))
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
				// Add a GuiItem in this Collection if needed
				int id = AddRequest.GetId();
				if(id != -1)
				{
					if(id < itemsProperty.arraySize)
						itemsProperty.InsertArrayElementAtIndex(id);
					else
						itemsProperty.arraySize++;
					ResetGuiItem(itemsProperty.GetArrayElementAtIndex(id));

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Remove a GuiItem from this Collection if needed
				int id = RemoveRequest.GetId();
				if(id != -1)
				{
					itemsProperty.DeleteArrayElementAtIndex(id);

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Swap two GuiItem objects from this Collection if needed
				int[] id = MoveRequest.GetIds();
				if(id.Length != 0)
				{
					itemsProperty.MoveArrayElement(id[0], id[1]);

					GUI.FocusControl(focusUnset);
				}
			}
			{
				// Copy a GuiItem if needed
				int idToCopy;
				int destinationId = CopyRequest.GetId(out idToCopy);
				if(destinationId != -1)
				{
					itemsProperty.InsertArrayElementAtIndex(idToCopy);
					itemsProperty.MoveArrayElement(idToCopy, destinationId);

					GUI.FocusControl(focusUnset);
				}
			}
		}

		EditorGUILayout.EndVertical();


		// Apply modifications
		serializedObject.ApplyModifiedProperties();


		// Revert to previous state
		GUI.changed = guiChangedSave;
	}


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
/// GuiItem inspector.
/// </summary>
static public class GuiItemInspector
{
	/// <summary>
	/// Indicates if it is possible to reference objects from Scene.
	/// GuiItem objects should not reference Scene object if they are not instantiated in the Scene hierarchy.
	/// </summary>
	/// <param name="collection">The Collection which contains this GuiItem.</param>
	/// <returns></returns>
	static private bool AllowSceneObjects(GuiItemsCollection collection)
	{
		return EditorUtility.IsPersistent(collection) ? false : true;
	}


	/// <summary>
	/// Draws a GuiItem in the Inspector.
	/// </summary>
	/// <param name="_itemProperty">The serialized property of the GuiItem to draw.</param>
	/// <param name="guiItem">The GuiItem object to draw.</param>
	/// <param name="_id">The id of this GuiItem in the Collection.</param>
	static public void Draw(SerializedProperty _itemProperty, GuiItemsCollection.GuiItem guiItem, int _id)
	{
		bool guiEnabledSave = GUI.enabled;

		Color c = GUI.color;
		GUI.color = CustomStyles.BACKGROUND_COLOR_BLUE;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		GUI.color = c;


		// Title bar with its buttons
		EditorGUILayout.BeginHorizontal();
		{
			// Property which indicates if this GuiItem is folded
			SerializedProperty editor_foldedProperty = _itemProperty.FindPropertyRelative("editor_folded");


			// We use a custom Foldout with a clickable label
			bool foldTemp = GuiItemsGUILayoutExtension.FoldoutButton(guiItem.editor_folded, new GUIContent(guiItem.thisItemType.ToString() + (guiItem.tag != "" ? ":" + guiItem.tag : "")));


			// Save the editor_folded value
			if(foldTemp != guiItem.editor_folded)
				editor_foldedProperty.boolValue = foldTemp;


			// Property of the list which contains the GuiItem elements
			SerializedProperty itemsArrayProperty = _itemProperty.serializedObject.FindProperty("items");


			// Buttons to move or remove this GuiItem
			if(_id == 0)
				GUI.enabled = false;
			else
				GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("▲", "Move up"), CustomStyles.miniButtonLeft))
			{
				MoveRequest.RequestIds(_id, _id - 1);
			}

			if(_id == itemsArrayProperty.arraySize - 1)
				GUI.enabled = false;
			else
				GUI.enabled = true;

			if(GUILayout.Button(new GUIContent("▼", "Move down"), CustomStyles.miniButtonMid))
			{
				MoveRequest.RequestIds(_id, _id + 1);
			}

			GUI.enabled = guiEnabledSave;


			if(GUILayout.Button(new GUIContent("X", "Remove"), CustomStyles.miniButtonRight))
			{
				RemoveRequest.RequestId(_id);
			}
		}
		EditorGUILayout.EndHorizontal();


		if(guiItem.editor_folded)
		{
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("thisItemType"), new GUIContent("Element Type"));
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("enabled"), new GUIContent("Enabled", "Is this GuiItem enabled?"));
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("activated"), new GUIContent("Activated", "Is this GuiItem activated? In other words, defines if this GuiItem is interactive (it is still displayed)."));


			// Rotation // Deactivated for the moment because the system is not robust
			//guiItem.rotationAngle = EditorGUILayout.FloatField(new GUIContent("Rotation Angle", "Rotation applying on this element and all next elements. You don't need to reset it at the end."), guiItem.rotationAngle);
			//guiItem.pivotPoint = EditorGUILayout.Vector2Field("Rotation pivot point", guiItem.pivotPoint);


			// This GuiItem name. This is needed for GuiItemsInterfaceBase children for event handling. Can be empty.
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("tag"), new GUIContent("Tag", "This GuiItem name. This is needed for GuiItemsInterfaceBase children for event handling. Can be empty."));


			// Color
			EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("color"), new GUIContent("Color", "Color of this GuiItem. The applied color is relative to the global one set on the Collection."));


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


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BOX:
					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BUTTON:
					// Events properties
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("eventToLaunch"), new GUIContent("Event To Launch", "Event name to launch on activation."));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("parameter"), new GUIContent("Parameter", "Parameter of the event."));


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.REPEAT_BUTTON:
					// Events properties
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("eventToLaunch"), new GUIContent("Event To Launch", "Event name to launch on activation."));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("parameter"), new GUIContent("Parameter", "Parameter of the event."));


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
					// Max characters count
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Max characters count. Use negative value for infinite length."));


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
					// Caractère de masque
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maskChar"), new GUIContent("Mask Char", "Character to mask the password with."));


					// Max characters count
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Max characters count. Use negative value for infinite length."));


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
					// Max characters count
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("maxLength"), new GUIContent("Max Length", "Max characters count. Use negative value for infinite length."));


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TOGGLE:
					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
					// Column count in the SelectionGrid
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("xCount"), new GUIContent("xCount", "How many elements to fit in the horizontal direction. The elements will be scaled to fit unless the style defines a fixedWidth to use. The height of the control will be determined from the number of elements."));

					if(_itemProperty.FindPropertyRelative("xCount").intValue < 1)
						_itemProperty.FindPropertyRelative("xCount").intValue = 1;


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.SPACE:
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("pixels"), new GUIContent("Pixels", "Size of the space. Can be negative."));


					EditorGUILayout.HelpBox("The direction of the space is dependent on the layout group you're currently in when issuing the command. If in a vertical group, the space will be vertical. Note: This will override the GUILayout.ExpandWidth and GUILayout.ExpandHeight.", MessageType.Info);
					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_HORIZONTAL:
					EditorGUILayout.HelpBox("The content cannot be drawn if style mode of this GuiItem is not NO_STYLE. Moreover, images and texts not possibly drawn simultaneously, the image has priority over text.", MessageType.Info);


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_VERTICAL:
					EditorGUILayout.HelpBox("The content cannot be drawn if style mode of this GuiItem is not NO_STYLE. Moreover, images and texts not possibly drawn simultaneously, the image has priority over text.", MessageType.Info);


					// Draw GuiContent inspector
					GuiContentInspector.Draw(_itemProperty);


					// GUIStyle
					DrawGuiStyleBlock(_itemProperty);

					break;

				case GuiItemsCollection.GuiItem.itemType.BEGIN_SCROLL_VIEW:
					EditorGUILayout.HelpBox("BeginScrollView is bugged (Unity has to fix it). Therefore, we cannot use GuiStyleElement for bar styling. For bar styling, use the skin used by this Collection.guiSkin (GuiItemsDrawer.thisGuiSkin also do the job).", MessageType.Info);

					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("alwaysShowHorizontal"));
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("alwaysShowVertical"));


					// GUISkin
					EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("guiSkin"));


					// GUIStyle is bugged so don't use unless Unity has fixed this
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
			SerializedProperty editor_styleFoldedProperty = _itemProperty.FindPropertyRelative("editor_styleFolded");
			bool tmp = GuiItemsGUILayoutExtension.FoldoutButton(editor_styleFoldedProperty.boolValue, new GUIContent("Style"));

			if(tmp != editor_styleFoldedProperty.boolValue)
				editor_styleFoldedProperty.boolValue = tmp;

			if(tmp)
			{
				EditorGUILayout.Space();


				// thisGuiStyleMode selector
				DrawStyleModeSelector(_itemProperty);


				// GUIStyle inspector
				DrawGuiStyleInspector(_itemProperty);

				EditorGUILayout.Space();
			}
		}
		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// Draws the thisGuiStyleMode selector with Edit and Customize buttons.
	/// </summary>
	/// <param name="_itemProperty">The serialized property of the GuiItem from which we want to draw the style mode selector.</param>
	static private void DrawStyleModeSelector(SerializedProperty _itemProperty)
	{
		EditorGUILayout.BeginHorizontal();

		SerializedProperty thisGuiStyleModeProperty = _itemProperty.FindPropertyRelative("thisGuiStyleMode");

		EditorGUILayout.PropertyField(thisGuiStyleModeProperty);

		GUIStyle gs = GUI.skin.button;
		GUI.skin.button.font = EditorStyles.miniFont;


		// Button to edit the current style
		switch((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex)
		{
			case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
				if(GUILayout.Button(new GUIContent("Edit", "Edit this style"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false)))
				{
					// Select this object
					Selection.activeObject = _itemProperty.FindPropertyRelative("guiStyleElement").objectReferenceValue;
				}
				break;

			default:
				GUI.enabled = false;
				GUILayout.Button(new GUIContent("Edit", "Edit this style"), EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false));
				GUI.enabled = true;
				break;
		}


		// Button to customize the current style
		bool guiEnabledSave = GUI.enabled;

		if((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex == GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE
			|| (GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex == GuiItemsCollection.GuiItem.guiStyleMode.DEFAULT)
			GUI.enabled = false;
		else
			GUI.enabled = guiEnabledSave;


		if(GUILayout.Button(new GUIContent("Customize", "Customize the current style applied on this GuiItem. Only this GuiItem will have access to this style."), EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
		{
			switch((GuiItemsCollection.GuiItem.guiStyleMode)thisGuiStyleModeProperty.enumValueIndex)
			{
				case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
					// Copy the style of guiStyleElement to CustomStyle
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
	/// Draws a GuiItem style inspector.
	/// </summary>
	/// <param name="_itemProperty">The serialized property of the GuiItem from which we want to draw the style inspector.</param>
	static private void DrawGuiStyleInspector(SerializedProperty _itemProperty)
	{
		switch((GuiItemsCollection.GuiItem.guiStyleMode)_itemProperty.FindPropertyRelative("thisGuiStyleMode").enumValueIndex)
		{
			case GuiItemsCollection.GuiItem.guiStyleMode.GUI_STYLE_ELEMENT:
				EditorGUILayout.Space();

				EditorGUILayout.PropertyField(_itemProperty.FindPropertyRelative("guiStyleElement"), new GUIContent("Gui Style Element", "Game Object referencing the style to use."));

				break;

			case GuiItemsCollection.GuiItem.guiStyleMode.CUSTOM_STYLE:
				EditorGUILayout.Space();


				// Draw customStyle's inspector
				GuiStyleInspector.Draw(_itemProperty.FindPropertyRelative("customStyle"));
				break;
		}
	}
}


/// <summary>
/// Class for GUIContent's inspector.
/// </summary>
public class GuiContentInspector
{
	/// <summary>
	/// Draws a GUIContent's inspector.
	/// </summary>
	/// <param name="_guiItemProperty">The GuiItem from which this GUIContent belongs.</param>
	/// <param name="_drawTooltip">Do we have to draw the tooltip field? (for BEGIN_HORIZONTAL and BEGIN_VERTICAL).</param>
	static public void Draw(SerializedProperty _guiItemProperty, bool _drawTooltip = true)
	{
		// List of the contents to draw
		SerializedProperty guiContentsArrayProperty = _guiItemProperty.FindPropertyRelative("contents");
		GuiItemsCollection.GuiItem.itemType itemType = (GuiItemsCollection.GuiItem.itemType)_guiItemProperty.FindPropertyRelative("thisItemType").enumValueIndex;


		// Main container
		Color c = GUI.color;
		GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
		EditorGUILayout.BeginVertical(GUI.skin.box);
		GUI.color = c;
		{
			// GUIContent fold state
			SerializedProperty editor_contentsFoldedProperty = _guiItemProperty.FindPropertyRelative("editor_contentsFolded");

			bool fold = GuiItemsGUILayoutExtension.FoldoutButton(editor_contentsFoldedProperty.boolValue, new GUIContent("Content(s)", "Content(s) of this element."));


			// Save the value if needed
			if(fold != editor_contentsFoldedProperty.boolValue)
				editor_contentsFoldedProperty.boolValue = fold;


			if(fold)
			{
				// Remove add or move actions if this GuiItem's type is not permitting multicontent
				if((itemType == GuiItemsCollection.GuiItem.itemType.TOOLBAR) || (itemType == GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
				{
					// Button to add a GUIContent
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button(new GUIContent("+", "Add a content here"), CustomStyles.littleButton))
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
					// Title bar + buttons on the right side
					c = GUI.color;
					GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
					EditorGUILayout.BeginVertical(GUI.skin.box);
					GUI.color = c;
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Content #" + index);


						// Remove add or move actions if this GuiItem's type is not permitting multicontent
						if((itemType == GuiItemsCollection.GuiItem.itemType.TOOLBAR) || (itemType == GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
						{
							bool guiEnabledSave = GUI.enabled;

							if(index == 0)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("▲", "Move up"), CustomStyles.miniButtonLeft))
							{
								ContentMoveRequest.RequestIds(index, index - 1);
							}

							if(index == guiContentsArrayProperty.arraySize - 1)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("▼", "Move down"), CustomStyles.miniButtonMid))
							{
								ContentMoveRequest.RequestIds(index, index + 1);
							}

							GUI.enabled = guiEnabledSave;
							if(GUILayout.Button(new GUIContent("Reset", "Reset this content"), CustomStyles.miniButtonMid))
							{
								GuiItemsTools.ResetPropertyValues(contentProperty);
							}


							// We should not have 0 content
							if(guiContentsArrayProperty.arraySize <= 1)
								GUI.enabled = false;
							else
								GUI.enabled = guiEnabledSave;

							if(GUILayout.Button(new GUIContent("X", "Remove"), CustomStyles.miniButtonRight))
							{
								ContentRemoveRequest.RequestId(index);
							}

							GUI.enabled = guiEnabledSave;
						}

						EditorGUILayout.EndHorizontal();


						// GUIContent's text
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


					// Cut prematurely the loop if this GuiItem's type does not permit multicontent
					if((itemType != GuiItemsCollection.GuiItem.itemType.TOOLBAR) && (itemType != GuiItemsCollection.GuiItem.itemType.SELECTION_GRID))
						break;


					// Button to add a GUIContent
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					if(GUILayout.Button(new GUIContent("+", "Add"), CustomStyles.littleButton))
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


		// Manage contents
		{
			// Add
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
			// Remove
			int contentToRemoveId = ContentRemoveRequest.GetId();
			if(contentToRemoveId != -1)
			{
				guiContentsArrayProperty.DeleteArrayElementAtIndex(contentToRemoveId);
			}
		}
		{
			// Swap
			int[] contentToMoveId = ContentMoveRequest.GetIds();
			if(contentToMoveId.Length != 0)
			{
				guiContentsArrayProperty.MoveArrayElement(contentToMoveId[0], contentToMoveId[1]);
			}
		}
	}
}


/// <summary>
/// Class to indicate the editor to add a GuiItem in the Collection.
/// </summary>
static public class AddRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the id of the GuiItem to add.
	/// </summary>
	/// <param name="_id">The Id used by the added GuiItem in the Collection.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Retrieves the registered Id.
	/// </summary>
	/// <returns>The Id. Returns -1 if no id has been defined.</returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}

/// <summary>
/// Class to indicate the editor to remove a GuiItem from the Collection.
/// </summary>
static public class RemoveRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the id of the GuiItem to remove.
	/// </summary>
	/// <param name="_id">GuiItem's Id in its Collection.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Retrieves the registered Id.
	/// </summary>
	/// <returns>The Id. Returns -1 if no id has been defined.</returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Class to indicate the editor to move a GuiItem in the Collection.
/// </summary>
static public class MoveRequest
{
	static private int id = 0;
	static private int idToReplace = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the ids of the GuiItem objects to swap.
	/// </summary>
	/// <param name="_id">Id if the GuiItem to move.</param>
	/// <param name="_idToReplace">Destination id.</param>
	static public void RequestIds(int _id, int _idToReplace)
	{
		id = _id;
		idToReplace = _idToReplace;
		requested = true;
	}


	/// <summary>
	/// Obtain registered Ids.
	/// </summary>
	/// <returns>The Ids. Returns an empty array if ids were not defined.</returns>
	static public int[] GetIds()
	{
		if(!requested)
			return new int[0];

		requested = false;

		return new int[] { id, idToReplace };
	}
}


/// <summary>
/// Class to indicate the editor to copy a GuiItem in the Collection.
/// </summary>
static public class CopyRequest
{
	static private int id = 0;
	static private int idToCopy = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the id of the GuiItem to copy and the copy's future id.
	/// </summary>
	/// <param name="_id">Original's id in the Collection.</param>
	/// <param name="_idToCopy">Destination id the Collection.</param>
	static public void RequestId(int _id, int _idToCopy)
	{
		id = _id;
		idToCopy = _idToCopy;
		requested = true;
	}


	/// <summary>
	/// Retrieves the registered Id.
	/// </summary>
	/// <returns>The Id. Returns -1 if no id has been defined.</returns>
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
/// Class to indicate the editor to add a GUIContent in the contents list.
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
	/// Retrieves the registered Id.
	/// </summary>
	/// <returns>The Id. Returns -1 if no id has been defined.</returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Class to indicate the editor to remove a GUIContent from the contents list.
/// </summary>
static public class ContentRemoveRequest
{
	static private int id = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the id of the GUIContent to remove.
	/// </summary>
	/// <param name="_id">GUIContent's Id in its contents array.</param>
	static public void RequestId(int _id)
	{
		id = _id;
		requested = true;
	}


	/// <summary>
	/// Retrieves the registered Id.
	/// </summary>
	/// <returns>The Id. Returns -1 if no id has been defined.</returns>
	static public int GetId()
	{
		if(!requested)
			return -1;

		requested = false;

		return id;
	}
}


/// <summary>
/// Class to indicate the editor to move a GUIContent in the contents list.
/// </summary>
static public class ContentMoveRequest
{
	static private int id = 0;
	static private int idToReplace = 0;
	static private bool requested = false;


	/// <summary>
	/// Defines the ids of the GUIContent objects to swap.
	/// </summary>
	/// <param name="_id">Id if the GUIContent to move.</param>
	/// <param name="_idToReplace">Destination id.</param>
	static public void RequestIds(int _id, int _idToReplace)
	{
		id = _id;
		idToReplace = _idToReplace;
		requested = true;
	}


	/// <summary>
	/// Obtain registered Ids.
	/// </summary>
	/// <returns>The Ids. Returns an empty array if ids were not defined.</returns>
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