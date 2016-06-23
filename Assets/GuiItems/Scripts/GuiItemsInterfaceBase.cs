using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Classe pour lister les valeurs des différents éléments TextField, TextArea, PasswordField.
/// </summary>
public class GuiItemReturnValue
{
	public GuiItemsCollection.GuiItem guiItem = null;
	public string fieldValue = "";
	public bool toggle = false;
	public int selected = 0;

	public GuiItemReturnValue(GuiItemsCollection.GuiItem _guiItem, string _fieldValue)
	{
		guiItem = _guiItem;
		fieldValue = _fieldValue;
	}

	public GuiItemReturnValue(GuiItemsCollection.GuiItem _guiItem, int _selected)
	{
		guiItem = _guiItem;
		selected = _selected;
	}

	public GuiItemReturnValue(GuiItemsCollection.GuiItem _guiItem, bool _toggle)
	{
		guiItem = _guiItem;
		toggle = _toggle;
	}
}


abstract public class GuiItemsInterfaceBase : MonoBehaviour
{
	/// <summary>
	/// Liste des champs de valeurs.
	/// </summary>
	protected List<GuiItemReturnValue> guiItemFieldValues;


	/// <summary>
	/// GuiItems dont cet objet est associé. Permet de vérifier les entrées textes.
	/// </summary>
	protected GuiItemsCollection associatedGuiItems
	{
		get
		{
			return GetComponent<GuiItemsCollection>();
		}
	}


	/// <summary>
	/// Fonction qui reçoit les évènements provenant de GuiItems.
	/// </summary>
	/// <param name="guiItem">GuiItem qui a envoyé l'évènement.</param>
	abstract public void ReceiveEvent(GuiItemsCollection.GuiItem guiItem);


	/// <summary>
	/// Fonction lancée lorsque le guiItem indiqué en paramètre a été survolé.
	/// </summary>
	/// <param name="guiItem">GuiItem qui a envoyé l'évènement.</param>
	abstract public void OnHover(GuiItemsCollection.GuiItem guiItem);


	void OnEnable()
	{
		guiItemFieldValues = new List<GuiItemReturnValue>();
	}


	void OnGUI()
	{
		// Vider la liste
		guiItemFieldValues.Clear();

		foreach(GuiItemsCollection.GuiItem g in associatedGuiItems.items)
		{
			switch(g.thisItemType)
			{
				case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
				case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
				case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.content.text));
					//Debug.Log(g.tag + " : " + guiItemFieldValues[guiItemFieldValues.Count - 1].fieldValue);
					break;

				case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
				case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.selected));
					//Debug.Log(g.tag + " : " + guiItemFieldValues[guiItemFieldValues.Count - 1].selected);
					break;

				case GuiItemsCollection.GuiItem.itemType.TOGGLE:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.toggle));
					//Debug.Log(g.tag + " : " + guiItemFieldValues[guiItemFieldValues.Count - 1].toggle);
					break;
			}
		}
	}
}
