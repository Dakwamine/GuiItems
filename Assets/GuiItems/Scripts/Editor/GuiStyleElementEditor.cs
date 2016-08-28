using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GuiStyleElement))]
[CanEditMultipleObjects]
public class GuiStyleElementEditor : Editor
{
	/// <summary>
	/// Simplified access to target.
	/// </summary>
	private GuiStyleElement guiStyleElement
	{
		get
		{
			return (GuiStyleElement)target;
		}
	}

	public SerializedObject SerializedObject
	{
		get
		{
			return serializedObject;
		}
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		GuiStyleInspector.Draw(serializedObject.FindProperty("guiStyleExtension"));

		serializedObject.ApplyModifiedProperties();
	}
}


/// <summary>
/// Class to draw custom inspector of GUIStyle.
/// </summary>
static public class GuiStyleInspector
{
	/// <summary>
	/// Draws the GUIStyleExtension property.
	/// </summary>
	/// <param name="_guiStyleExtensionProperty"></param>
	static public void Draw(SerializedProperty _guiStyleExtensionProperty)
	{
		SerializedProperty guiStyleProperty = _guiStyleExtensionProperty.FindPropertyRelative("guiStyle");


		// Main container
		EditorGUILayout.BeginVertical();
		{
			// Text style
			Color c = GUI.color;
			GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUI.color = c;
			{
				if(GuiItemsGUILayoutExtension.FoldoutButtonProperty(_guiStyleExtensionProperty.FindPropertyRelative("showTextStyle"), new GUIContent("Text style")))
				{
					EditorGUI.indentLevel++;

					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Font"), new GUIContent("Font", "The font to use for rendering. If null, the default font for the current GUISkin is used instead."));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_FontStyle"), new GUIContent("Font style", "The font style to use (for dynamic fonts)"));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_FontSize"), new GUIContent("Font size", "The font size to use (for dynamic fonts)"));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_WordWrap"), new GUIContent("Word wrap", "Word wrap the text?"));
					TextClipping tc = (TextClipping)EditorGUILayout.EnumPopup(new GUIContent("Text clipping", "What to do when the contents to be rendered is too large to fit within the area given."), (TextClipping)guiStyleProperty.FindPropertyRelative("m_TextClipping").intValue);
					if(tc != (TextClipping)guiStyleProperty.FindPropertyRelative("m_TextClipping").intValue)
						guiStyleProperty.FindPropertyRelative("m_TextClipping").intValue = (int)tc;

					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_RichText"), new GUIContent("Rich text", "Enable HTML-style tags for Text Formatting Markup."));

					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();


			// Text color
			GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUI.color = c;
			{
				if(GuiItemsGUILayoutExtension.FoldoutButtonProperty(_guiStyleExtensionProperty.FindPropertyRelative("showTextColor"), new GUIContent("Text colors")))
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.BeginVertical();
						{
							GUILayout.Label("Normal");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_Normal.m_TextColor"));
							GUILayout.Label("Hover");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_Hover.m_TextColor"));
							GUILayout.Label("Active");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_Active.m_TextColor"));
							GUILayout.Label("Focused");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_Focused.m_TextColor"));
						}
						EditorGUILayout.EndVertical();
						EditorGUILayout.BeginVertical();
						{
							GUILayout.Label("On Normal");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_OnNormal.m_TextColor"));
							GUILayout.Label("On Hover");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_OnHover.m_TextColor"));
							GUILayout.Label("On Active");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_OnActive.m_TextColor"));
							GUILayout.Label("On Focused");
							GuiItemsGUILayoutExtension.ColorFieldProperty(guiStyleProperty.FindPropertyRelative("m_OnFocused.m_TextColor"));
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();


					// Info couleurs
					EditorGUILayout.Space();

					EditorGUILayout.HelpBox("Colors other than the one of the Normal state are taken into account only when a background texture has been set for the corresponding state.", MessageType.Info);
				}
			}
			EditorGUILayout.EndVertical();


			// Background texture
			GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUI.color = c;
			{
				if(GuiItemsGUILayoutExtension.FoldoutButtonProperty(_guiStyleExtensionProperty.FindPropertyRelative("showBackgroundTextures"), new GUIContent("Background textures")))
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.BeginVertical();
						{
							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_Normal.m_Background");

								GUILayout.Label("Normal");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();


							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_Hover.m_Background");

								GUILayout.Label("Hover");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();


							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_Active.m_Background");

								GUILayout.Label("Active");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();


							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_Focused.m_Background");

								GUILayout.Label("Focused");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUILayout.EndVertical();


						////////////////////////////////////////////////////////////
						///////////////////// OnNormal, OnHover, OnActive, OnFocused
						////////////////////////////////////////////////////////////
						EditorGUILayout.BeginVertical();
						{
							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_OnNormal.m_Background");

								GUILayout.Label("On Normal");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();

							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_OnHover.m_Background");

								GUILayout.Label("On Hover");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();


							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_OnActive.m_Background");

								GUILayout.Label("On Active");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();


							EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MinHeight(CustomStyles.BACKGROUND_BOX_MIN_HEIGHT));
							{
								SerializedProperty sp = guiStyleProperty.FindPropertyRelative("m_OnFocused.m_Background");

								GUILayout.Label("On Focused");
								EditorGUILayout.BeginHorizontal();
								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField(new GUIContent("No image"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField(new GUIContent("Multiple"), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								else
								{
									EditorGUILayout.LabelField(new GUIContent(((Texture2D)sp.objectReferenceValue)), CustomStyles.imagePreviewLabel, GUILayout.Height(CustomStyles.TEXTURE_BOX_SIZE), GUILayout.Width(CustomStyles.TEXTURE_BOX_SIZE));
								}
								EditorGUILayout.BeginVertical();
								EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));

								if(sp.objectReferenceValue == null)
								{
									EditorGUILayout.LabelField("No image", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else if(sp.hasMultipleDifferentValues)
								{
									EditorGUILayout.LabelField("Multiple", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								else
								{
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).width + " x " + ((Texture2D)sp.objectReferenceValue).height + " (L x H)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).format + " (TextureFormat)", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
									EditorGUILayout.LabelField(((Texture2D)sp.objectReferenceValue).filterMode + " (FilterMode) ", CustomStyles.imagePropertiesLabel, GUILayout.MinWidth(CustomStyles.IMAGE_PROPERTIES_MIN_WIDTH));
								}
								EditorGUILayout.EndVertical();
								EditorGUILayout.EndHorizontal();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();


			// Miscellaneous parameters
			GUI.color = CustomStyles.BACKGROUND_COLOR_GREY;
			EditorGUILayout.BeginVertical(GUI.skin.box);
			GUI.color = c;
			{
				if(GuiItemsGUILayoutExtension.FoldoutButtonProperty(_guiStyleExtensionProperty.FindPropertyRelative("showMisc"), new GUIContent("Other parameters")))
				{
					EditorGUI.indentLevel++;

					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Alignment"), new GUIContent("Alignement", "Text alignment."));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_ImagePosition"), new GUIContent("Image Position", "How image and text of the GUIContent is combined."));


					_guiStyleExtensionProperty.FindPropertyRelative("guiStyle.m_ContentOffset").vector2Value = EditorGUILayout.Vector2Field("Content offset", _guiStyleExtensionProperty.FindPropertyRelative("guiStyle.m_ContentOffset").vector2Value);


					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_FixedWidth"), new GUIContent("Fixed width", "If non-0, any GUI elements rendered with this style will have the width specified here."));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_FixedHeight"), new GUIContent("Fixed height", "If non-0, any GUI elements rendered with this style will have the height specified here."));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_StretchWidth"), new GUIContent("Stretch width", "Can GUI elements of this style be stretched horizontally for better layouting?"));
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_StretchHeight"), new GUIContent("Stretch height", "Can GUI elements of this style be stretched vertically for better layouting?"));

					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Border"), new GUIContent("Border", "The borders of all background images."), true);
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Padding"), new GUIContent("Padding", "Space from the edge of GUIStyle to the start of the contents."), true);
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Margin"), new GUIContent("Margin", "The margins between elements rendered in this style and any other GUI elements"), true);
					EditorGUILayout.PropertyField(guiStyleProperty.FindPropertyRelative("m_Overflow"), new GUIContent("Overflow", "Extra space to be added to the background image."), true);

					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.EndVertical();
	}
}