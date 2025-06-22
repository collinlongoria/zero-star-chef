using Godot;
using System;

public partial class TrashCan : StaticBody2D
{
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		if(Global.Instance.Player.HasHeldItem()) SignalBus.Instance.EmitDialogueRequest("trash_prompt");   
		else SignalBus.Instance.EmitDialogueRequest("trash_mad");
	}
}
