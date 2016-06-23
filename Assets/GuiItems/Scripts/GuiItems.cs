using UnityEngine;
using System.Collections;

abstract public class GuiItems
{
	/// <summary>
	/// Classe � utiliser pour r�f�rencer des GUIStyle avec un custom editor.
	/// </summary>
	[System.Serializable]
	public class GUIStyleExtension
	{
		public GUIStyle guiStyle;

//#if UNITY_EDITOR
		//public Object[] showTextStyle = new Object[0];
		public bool showTextStyle = false;
		public bool showTextColor = false;
		public bool showBackgroundTextures = false;
		public bool showMisc = false;
//#endif

		/*public void ReplaceGUIStyle(GUIStyle _guiStyle)
		{
			guiStyle = new GUIStyle(_guiStyle);
		}*/

		/// <summary>
		/// Creates a blank GUIStyleExtension.
		/// </summary>
		public GUIStyleExtension()
		{
			guiStyle = new GUIStyle();
		}


		/// <summary>
		/// Creates a GUIStyleExtension Instance with a copy of _style.
		/// </summary>
		/// <param name="_style">GUIStyle to copy.</param>
		public GUIStyleExtension(GUIStyle _style)
		{
			guiStyle = new GUIStyle(_style);
		}


		/// <summary>
		/// Creates an Instance which will use a copy of the style of the GUIStyleExtension parameter.
		/// </summary>
		/// <param name="_style">GUIStyleExtension that will get its style copied.</param>
		public GUIStyleExtension(GUIStyleExtension _style)
		{
			guiStyle = new GUIStyle(_style.guiStyle);
		}
	}
}
