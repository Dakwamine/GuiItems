using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Class which lists values from TEXT_FIELD, TEXT_AREA and PASSWORD_FIELD.
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
	/// Lists fields values.
	/// </summary>
	protected List<GuiItemReturnValue> guiItemFieldValues;


	/// <summary>
	/// GuiItemsCollection reference which shares the same transform. Text fields can be checked this way.
	/// </summary>
	protected GuiItemsCollection associatedGuiItems
	{
		get
		{
			return GetComponent<GuiItemsCollection>();
		}
	}


	/// <summary>
	/// Called when the GuiItem in parameter has launched an event.
	/// </summary>
	/// <param name="guiItem">GuiItem which triggered the event.</param>
	abstract public void ReceiveEvent(GuiItemsCollection.GuiItem guiItem);


	/// <summary>
	/// Called when the GuiItem in parameter has been hovered.
	/// </summary>
	/// <param name="guiItem">GuiItem which triggered the event</param>
	abstract public void OnHover(GuiItemsCollection.GuiItem guiItem);


	void OnEnable()
	{
		guiItemFieldValues = new List<GuiItemReturnValue>();
	}


	void OnGUI()
	{
		// Empty previous field values
		guiItemFieldValues.Clear();

		foreach(GuiItemsCollection.GuiItem g in associatedGuiItems.items)
		{
			switch(g.thisItemType)
			{
				case GuiItemsCollection.GuiItem.itemType.PASSWORD_FIELD:
				case GuiItemsCollection.GuiItem.itemType.TEXT_AREA:
				case GuiItemsCollection.GuiItem.itemType.TEXT_FIELD:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.content.text));
					break;

				case GuiItemsCollection.GuiItem.itemType.TOOLBAR:
				case GuiItemsCollection.GuiItem.itemType.SELECTION_GRID:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.selected));
					break;

				case GuiItemsCollection.GuiItem.itemType.TOGGLE:
					guiItemFieldValues.Add(new GuiItemReturnValue(g, g.toggle));
					break;
			}
		}
	}
}
