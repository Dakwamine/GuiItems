using UnityEngine;
using System.Collections;

namespace NS_GuiItems
{
	[AddComponentMenu("GuiItems/GuiStyleElement")]
	public class GuiStyleElement : MonoBehaviour
	{
		/// <summary>
		/// The GUIStyle of this GuiStyleElement.
		/// </summary>
		public GUIStyle guiStyle
		{
			get
			{
				return guiStyleExtension.guiStyle;
			}
			set
			{
				guiStyleExtension.guiStyle = value;
			}
		}

		/// <summary>
		/// GUIStyleExtension object containing a GUIStyle object and additional editor parameters.
		/// </summary>
		public GUIStyleExtension guiStyleExtension;
	}
}
