using Godot;
using System;

public partial class DmoJoe : StaticBody2D
{
	private bool _spoken = false;
	
	public override void _Ready()
	{
		AddToGroup("Interactable");
	}

	public void Interact()
	{
		if(!Global.Instance.IsBreakActive())
		{
			if (!_spoken)
			{
				_spoken = true;
				SignalBus.Instance.EmitDialogueRequest("dmo_intro");
			}
			else
			{
				SignalBus.Instance.EmitDialogueRequest("dmo_end");
			}
		}
		else
		{
			_spoken = true;
			SignalBus.Instance.EmitDialogueRequest("surprise1");
		}
	}
}
