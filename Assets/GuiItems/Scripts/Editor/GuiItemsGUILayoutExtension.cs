using UnityEngine;
using UnityEditor;
using System.Collections;

public class GuiItemsGUILayoutExtension
{
	/// <summary>
	/// Foldout with a clickable label, but not taking account of EditorGUI.indentLevel.
	/// </summary>
	/// <param name="_state">The shown foldout state.</param>
	/// <param name="_label">The label to show.</param>
	/// <returns></returns>
	public static bool FoldoutButton(bool _state, string _label)
	{
		bool b = false;

		if(_state)
			b = GUILayout.Button(_label, CustomStyles.foldoutActive);
		else
			b = GUILayout.Button(_label, CustomStyles.foldoutNormal);

		if(b)
			return !_state;
		else
			return _state;
	}

	/// <summary>
	/// Foldout with a clickable label, but not taking account of EditorGUI.indentLevel.
	/// </summary>
	/// <param name="_state">The shown foldout state.</param>
	/// <param name="_content">The label to show.</param>
	/// <returns>Foldout state.</returns>
	public static bool FoldoutButton(bool _state, GUIContent _content)
	{
		bool b = false;

		if(_state)
			b = GUILayout.Button(_content, CustomStyles.foldoutActive);
		else
			b = GUILayout.Button(_content, CustomStyles.foldoutNormal);

		if(b)
			return !_state;
		else
			return _state;
	}


	/// <summary>
	/// Foldout with a clickable label, but not taking account of EditorGUI.indentLevel, for properties.
	/// </summary>
	/// <param name="_property">The foldout variable property (bool).</param>
	/// <param name="_content">The label to show.</param>
	/// <returns>True when unfolded, false otherwise.</returns>
	public static bool FoldoutButtonProperty(SerializedProperty _property, GUIContent _content)
	{
		if(_property.boolValue)
		{
			if(GUILayout.Button(_content, CustomStyles.foldoutActive))
				_property.boolValue = !_property.boolValue;
		}
		else
		{
			if(GUILayout.Button(_content, CustomStyles.foldoutNormal))
				_property.boolValue = !_property.boolValue;
		}

		return _property.boolValue;
	}


	/// <summary>
	/// Color field for properties.
	/// </summary>
	/// <param name="_property">The Color property.</param>
	/// <param name="_content">The label to show.</param>
	public static void ColorFieldProperty(SerializedProperty _property, GUIContent _content = null)
	{
		// We use the appropriate overload because EditorGUILayout.ColorField(GUIContent, Color) bugs on draw (width is larger for unknown reason)
		Color c = _content == GUIContent.none || _content == null ? EditorGUILayout.ColorField(_property.colorValue) : EditorGUILayout.ColorField(_content, _property.colorValue);

		if(c != _property.colorValue)
			_property.colorValue = c;
	}


	public static bool ToggleButton(bool state, GUIContent content)
	{
		return ToggleButton(state, content, GUIStyle.none);
	}


	/// <summary>
	/// A field to draw the RectExtension object in the inspector.
	/// </summary>
	/// <param name="_r">The RectExtension property.</param>
	static public void RectExtensionField(SerializedProperty _r, bool _xEnabled = true, bool _yEnabled = true, bool _widthEnabled = true, bool _heightEnabled = true)
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUI.indentLevel++;

			FloatExtensionField(_r.FindPropertyRelative("x"), new GUIContent("X"), _xEnabled);
			FloatExtensionField(_r.FindPropertyRelative("y"), new GUIContent("Y"), _yEnabled);

			FloatExtensionField(_r.FindPropertyRelative("width"), new GUIContent("Width"), _widthEnabled);
			FloatExtensionField(_r.FindPropertyRelative("height"), new GUIContent("Height"), _heightEnabled);

			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// A field to draw the Vector2Extension object in the inspector.
	/// </summary>
	/// <param name="_r">The Vector2Extension.</param>
	static public void Vector2ExtensionField(SerializedProperty _v)
	{
		EditorGUILayout.BeginVertical();
		{
			EditorGUI.indentLevel++;

			FloatExtensionField(_v.FindPropertyRelative("x"), new GUIContent("X"));
			FloatExtensionField(_v.FindPropertyRelative("y"), new GUIContent("Y"));

			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndVertical();
	}


	/// <summary>
	/// A field to draw the FloatExtension object in the inspector.
	/// </summary>
	/// <param name="_r">The FloatExtension.</param>
	static public void FloatExtensionField(SerializedProperty _f, GUIContent _label, bool _fieldEnabled = true)
	{
		EditorGUILayout.BeginHorizontal();
		{
			bool guiEnabledSave = GUI.enabled;

			GUI.enabled = _fieldEnabled;

			EditorGUILayout.PropertyField(_f.FindPropertyRelative("value"), _label);

			EditorGUILayout.PropertyField(_f.FindPropertyRelative("relative"), GUIContent.none, GUILayout.Width(25f));

			GUI.enabled = guiEnabledSave;
		}
		EditorGUILayout.EndHorizontal();
	}

	public static bool ToggleButton(bool state, GUIContent content, GUIStyle _guiStyle)
	{
		bool b = false;

		if(_guiStyle == GUIStyle.none)
			_guiStyle = new GUIStyle(GUI.skin.button);

		Color guiColorSave = GUI.color;

		if(state)
			GUI.color = Color.green;

		b = GUILayout.Button(content, _guiStyle, GUILayout.ExpandWidth(false));

		if(state)
			GUI.color = guiColorSave;
		if(b)
		{
			GUI.changed = true;
			return !state;
		}
		else
			return state;
	}
}
