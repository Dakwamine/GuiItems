using UnityEditor;
using UnityEngine;


static public class GuiItemsTools
{
	static public class GUIStyleExtensionEditor
	{
		/// <summary>
		/// Copies a style to another.
		/// </summary>
		/// <param name="_original">The original style to copy.</param>
		/// <param name="_destination">The style to update.</param>
		static public void CopyStyleTo(SerializedProperty _original, SerializedProperty _destination)
		{
			if(_original == null)
			{
				Debug.LogError("_original style not set when trying to copy style");
				return;
			}

			if(_destination == null)
			{
				Debug.LogError("_destination style not set when trying to copy style");
				return;
			}

			if(_original.type != "GUIStyle")
			{
				Debug.LogError("_original type is not a GUIStyle. Indeed, it is a : " + _original.type);
				return;
			}

			if(_destination.type != "GUIStyle")
			{
				Debug.LogError("_destination type is not a GUIStyle. Indeed, it is a : " + _destination.type);
				return;
			}

			//SerializedProperty original = _original.Copy();
			//SerializedProperty destination = _destination.Copy();



			Debug.LogWarning("copy1 todo");
		}


		/// <summary>
		/// Copies a style to another.
		/// </summary>
		/// <param name="_original">The original style to copy.</param>
		/// <param name="_destination">The style to update.</param>
		static public void CopyStyleTo(GUIStyle _original, SerializedProperty _destination)
		{
			if(_original == null)
			{
				Debug.LogError("_original style not set when trying to copy style");
				return;
			}

			if(_destination == null)
			{
				Debug.LogError("_destination style not set when trying to copy style");
				return;
			}

			if(_destination.type != "GUIStyle")
			{
				Debug.LogError("_destination type is not a GUIStyle. Indeed, it is a : " + _destination.type);
				return;
			}


			_destination.FindPropertyRelative("m_Normal.m_Background").objectReferenceValue = _original.normal.background;
			_destination.FindPropertyRelative("m_Hover.m_Background").objectReferenceValue = _original.hover.background;
			_destination.FindPropertyRelative("m_Active.m_Background").objectReferenceValue = _original.active.background;
			_destination.FindPropertyRelative("m_Focused.m_Background").objectReferenceValue = _original.focused.background;
			_destination.FindPropertyRelative("m_OnNormal.m_Background").objectReferenceValue = _original.onNormal.background;
			_destination.FindPropertyRelative("m_OnHover.m_Background").objectReferenceValue = _original.onHover.background;
			_destination.FindPropertyRelative("m_OnActive.m_Background").objectReferenceValue = _original.onActive.background;
			_destination.FindPropertyRelative("m_OnFocused.m_Background").objectReferenceValue = _original.onFocused.background;

			_destination.FindPropertyRelative("m_Normal.m_TextColor").colorValue = _original.normal.textColor;
			_destination.FindPropertyRelative("m_Hover.m_TextColor").colorValue = _original.hover.textColor;
			_destination.FindPropertyRelative("m_Active.m_TextColor").colorValue = _original.active.textColor;
			_destination.FindPropertyRelative("m_Focused.m_TextColor").colorValue = _original.focused.textColor;
			_destination.FindPropertyRelative("m_OnNormal.m_TextColor").colorValue = _original.onNormal.textColor;
			_destination.FindPropertyRelative("m_OnHover.m_TextColor").colorValue = _original.onHover.textColor;
			_destination.FindPropertyRelative("m_OnActive.m_TextColor").colorValue = _original.onActive.textColor;
			_destination.FindPropertyRelative("m_OnFocused.m_TextColor").colorValue = _original.onFocused.textColor;

			_destination.FindPropertyRelative("m_Font").objectReferenceValue = _original.font;
			_destination.FindPropertyRelative("m_FontStyle").enumValueIndex = (int)_original.fontStyle;
			_destination.FindPropertyRelative("m_FontSize").intValue = _original.fontSize;
			_destination.FindPropertyRelative("m_WordWrap").boolValue = _original.wordWrap;
			_destination.FindPropertyRelative("m_TextClipping").intValue = (int)_original.clipping;
			//_destination.FindPropertyRelative("m_TextClipping").intValue = (int)_original.clipping;

			_destination.FindPropertyRelative("m_Alignment").enumValueIndex = (int)_original.alignment;
			_destination.FindPropertyRelative("m_ImagePosition").enumValueIndex = (int)_original.imagePosition;
			_destination.FindPropertyRelative("m_FixedWidth").floatValue = _original.fixedWidth;
			_destination.FindPropertyRelative("m_FixedHeight").floatValue = _original.fixedHeight;
			_destination.FindPropertyRelative("m_StretchWidth").boolValue = _original.stretchWidth;
			_destination.FindPropertyRelative("m_StretchHeight").boolValue = _original.stretchHeight;

			_destination.FindPropertyRelative("m_Border.m_Left").intValue = _original.border.left;
			_destination.FindPropertyRelative("m_Border.m_Right").intValue = _original.border.right;
			_destination.FindPropertyRelative("m_Border.m_Top").intValue = _original.border.top;
			_destination.FindPropertyRelative("m_Border.m_Bottom").intValue = _original.border.bottom;

			_destination.FindPropertyRelative("m_Padding.m_Left").intValue = _original.padding.left;
			_destination.FindPropertyRelative("m_Padding.m_Right").intValue = _original.padding.right;
			_destination.FindPropertyRelative("m_Padding.m_Top").intValue = _original.padding.top;
			_destination.FindPropertyRelative("m_Padding.m_Bottom").intValue = _original.padding.bottom;

			_destination.FindPropertyRelative("m_Margin.m_Left").intValue = _original.margin.left;
			_destination.FindPropertyRelative("m_Margin.m_Right").intValue = _original.margin.right;
			_destination.FindPropertyRelative("m_Margin.m_Top").intValue = _original.margin.top;
			_destination.FindPropertyRelative("m_Margin.m_Bottom").intValue = _original.margin.bottom;

			_destination.FindPropertyRelative("m_Overflow.m_Left").intValue = _original.overflow.left;
			_destination.FindPropertyRelative("m_Overflow.m_Right").intValue = _original.overflow.right;
			_destination.FindPropertyRelative("m_Overflow.m_Top").intValue = _original.overflow.top;
			_destination.FindPropertyRelative("m_Overflow.m_Bottom").intValue = _original.overflow.bottom;
		}
	}


	/// <summary>
	/// Copies a Serialized Property GuiItem and all its contents into another.
	/// </summary>
	/// <param name="_source">GuiItem source.</param>
	/// <param name="_destination">GuiItem destination.</param>
	static public void CopyPropertyValues(SerializedProperty _source, SerializedProperty _destination)
	{
		// Vérifier si les types des objets sont identiques
		if(_source.type != _destination.type)
		{
			Debug.LogError("Not the same property types between _source and _destination.");
			return;
		}


		// Créer de nouveaux itérateurs pour éviter de travailler sur les itérateurs originaux
		SerializedProperty source = _source.Copy();
		SerializedProperty destination = _destination.Copy();


		int startingDepth = source.depth;

		do
		{
			switch(source.propertyType)
			{
				case SerializedPropertyType.AnimationCurve:
					destination.animationCurveValue = source.animationCurveValue;
					break;
				case SerializedPropertyType.ArraySize:
					destination.intValue = source.intValue;
					break;
				case SerializedPropertyType.Boolean:
					destination.boolValue = source.boolValue;
					break;
				case SerializedPropertyType.Bounds:
					destination.boundsValue = source.boundsValue;
					break;
				case SerializedPropertyType.Character:
					// Not sure how it works
					destination.objectReferenceValue = source.objectReferenceValue;
					break;
				case SerializedPropertyType.Color:
					destination.colorValue = source.colorValue;
					break;
				case SerializedPropertyType.Enum:
					destination.enumValueIndex = source.enumValueIndex;
					break;
				case SerializedPropertyType.Float:
					destination.floatValue = source.floatValue;
					break;
				case SerializedPropertyType.Generic:
					// Don't think there is a need to copy
					break;
				case SerializedPropertyType.Integer:
					destination.intValue = source.intValue;
					break;
				case SerializedPropertyType.LayerMask:
					// Not sure how it works
					destination.intValue = source.intValue;
					break;
				case SerializedPropertyType.ObjectReference:
					destination.objectReferenceValue = source.objectReferenceValue;
					break;
				case SerializedPropertyType.Rect:
					destination.rectValue = source.rectValue;
					break;
				case SerializedPropertyType.String:
					destination.stringValue = source.stringValue;
					break;
				case SerializedPropertyType.Vector2:
					destination.vector2Value = source.vector2Value;
					break;
				case SerializedPropertyType.Vector3:
					destination.vector3Value = source.vector3Value;
					break;
			}
		} while(source.NextVisible(true) && destination.NextVisible(true) && source.depth > startingDepth);
	}


	/// <summary>
	/// Resets a Serialized Property GuiItem and all its contents into another.
	/// </summary>
	/// <param name="_source">GuiItem source.</param>
	/// <param name="_property">GuiItem destination.</param>
	static public void ResetPropertyValues(SerializedProperty _property)
	{
		// Créer un nouvel itérateur pour éviter de travailler sur l'itérateur original
		SerializedProperty property = _property.Copy();


		int startingDepth = property.depth;

		do
		{
			switch(property.propertyType)
			{
				case SerializedPropertyType.AnimationCurve:
					property.animationCurveValue = new AnimationCurve();
					break;
				case SerializedPropertyType.ArraySize:
					property.intValue = 0;
					break;
				case SerializedPropertyType.Boolean:
					property.boolValue = false;
					break;
				case SerializedPropertyType.Bounds:
					property.boundsValue = new Bounds();
					break;
				case SerializedPropertyType.Character:
					// Not sure how it works
					property.objectReferenceValue = null;
					break;
				case SerializedPropertyType.Color:
					property.colorValue = new Color();
					break;
				case SerializedPropertyType.Enum:
					property.enumValueIndex = 0;
					break;
				case SerializedPropertyType.Float:
					property.floatValue = 0f;
					break;
				case SerializedPropertyType.Generic:
					// Don't think there is a need to copy
					break;
				case SerializedPropertyType.Integer:
					property.intValue = 0;
					break;
				case SerializedPropertyType.LayerMask:
					// Not sure how it works
					property.intValue = 0;
					break;
				case SerializedPropertyType.ObjectReference:
					property.objectReferenceValue = null;
					break;
				case SerializedPropertyType.Rect:
					property.rectValue = new Rect();
					break;
				case SerializedPropertyType.String:
					property.stringValue = "";
					break;
				case SerializedPropertyType.Vector2:
					property.vector2Value = new Vector2();
					break;
				case SerializedPropertyType.Vector3:
					property.vector3Value = new Vector3();
					break;
			}
		} while(property.NextVisible(true) && property.depth > startingDepth);
	}
}