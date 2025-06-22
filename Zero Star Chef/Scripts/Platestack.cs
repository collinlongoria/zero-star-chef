using Godot;
using System;

public partial class Platestack : StaticBody2D
{
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		SignalBus.Instance.EmitDialogueRequest("plate");
	}
}
