using Godot;
using System;

public partial class Countertop : StaticBody2D
{
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		GD.Print("You interacted with a countertop. Weird.");
	}
}
