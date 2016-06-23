using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// Simply editor window that lets you quick check a path of
/// a texture in your project instead of waiting your code to 
/// compile.
///
/// if the path exists then it shows a message
/// else displays an error message
/// </summary>
public class EditorGUIUtilityFindTexture : EditorWindow
{
	string path = "";

	[MenuItem("Assets/Check Path For Texture")]
	static void Init()
	{
		EditorWindow window = EditorWindow.GetWindow(typeof(EditorGUIUtilityFindTexture));
		window.position = new Rect(0f, 0f, 180f, 55f);
		window.Show();
	}

	void OnGUI()
	{
		path = EditorGUILayout.TextField("Path To Test:", path);
		if(GUILayout.Button("Check"))
		{
			if(EditorGUIUtility.FindTexture(path))
			{
				Debug.Log("Yay!, texture found at: " + path);
			}
			else
			{
				Debug.LogError("No texture found at: " + path + " Check your path");
			}
		}
	}
}