using Godot;
using System;
using System.Collections.Generic;

public partial class Countertop : StaticBody2D
{
	private List<Item> _items = new List<Item>(); 
	
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		GD.Print("You interacted with a countertop. Weird.");
		if (Global.Instance.Player != null)
		{
			var player = Global.Instance.Player;
			var item = player.GetHeldItem();

			if (item == null) return;

			if (item.Data.ItemName == "Plate")
			{
				GD.Print("You placed a plate down.");
				item.GetParent().RemoveChild(item);
				AddChild(item);
				item.Position = new Vector2(0, -4);
			}
		}
	}
}
