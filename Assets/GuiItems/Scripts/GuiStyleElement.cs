using UnityEngine;
using System.Collections;

[AddComponentMenu("GuiItems/GuiStyleElement")]
public class GuiStyleElement : MonoBehaviour
{
	/// <summary>
	/// Style de ce GuiStyleElement.
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
	/// Objet GUIStyleExtension référencé qui contient le style et les paramètres éditeur.
	/// </summary>
	public GuiItems.GUIStyleExtension guiStyleExtension;
}