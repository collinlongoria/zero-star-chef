using Godot;
using System;

public partial class Spill : StaticBody2D
{
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		SignalBus.Instance.EmitDialogueRequest("spill_1");
	}
}
