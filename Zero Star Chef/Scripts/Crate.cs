using Godot;
using System;

public partial class Crate : StaticBody2D
{
	[Export] public string ItemName = "";

	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		if (ItemFactory.Instance.ItemExists(ItemName))
		{
			if(Global.Instance.Player.IsTrip()) 
				SignalBus.Instance.EmitDialogueRequest("trip_inspect");
			else 
				SignalBus.Instance.EmitDialogueRequest(ItemFactory.Instance.GetItemDesc(ItemName));
		}
		else
		{
			GD.PrintErr("Item not found");
		}
	}
}
