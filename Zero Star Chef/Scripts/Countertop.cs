using Godot;
using System;
using System.Collections.Generic;

public partial class Countertop : StaticBody2D
{
	private Item _item;
	
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
	}
}
