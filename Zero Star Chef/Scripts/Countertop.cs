using Godot;
using System;
using System.Collections.Generic;

public partial class Countertop : StaticBody2D
{
	[Export] public Node2D ItemSpawnPoint;

	private string _storedItem = "";
	private Sprite2D _coverSprite = null;
	private bool _covered = false;
	
	public override void _Ready()
	{
		AddToGroup("Interactable");
		AddToGroup("Storage");

		if (ItemSpawnPoint == null)
			ItemSpawnPoint = this;
		
		_coverSprite = GetNode<Sprite2D>("Cover Sprite");
		_coverSprite.Visible = false;
	}

	public void Interact()
	{
		var player = Global.Instance.Player;

		if (!string.IsNullOrEmpty(_storedItem))
		{
			
			if (player.HasHeldItem())
			{
				SignalBus.Instance.EmitDialogueRequest("storage_switch_prompt");
			}
			else
			{
				SignalBus.Instance.EmitDialogueRequest("storage_grab_prompt");
			}
		}

		else
		{
			if (player.HasHeldItem())
			{
				SignalBus.Instance.EmitDialogueRequest("storage_store_prompt");
			}
			else
			{
				SignalBus.Instance.EmitDialogueRequest("storage_empty");
			}
		}
		
		
	}

	public void HandleInteraction()
	{
		var player = Global.Instance.Player;

		// PLAYER IS HOLDING SOMETHING
		if (player.HasHeldItem())
		{
			// Swap with existing item
			if (!string.IsNullOrEmpty(_storedItem))
			{
				// keep a copy of the counter’s current cover state for the player
				var oldItem        = _storedItem;
				var oldCovered     = _covered;

				// store the player item on the counter
				_storedItem = player.GetHeldItem().Data.ItemName;
				_covered    = Global.Instance.Arm.GetCoverVisible();

				ClearVisual();
				SpawnVisual(_storedItem, _covered);

				// give the player the old item
				var item = new Item { Data = ItemFactory.Instance.GetItem(oldItem) };
				player.ReceiveItem(item);
				Global.Instance.Arm.SetActive(true, item.Data.ItemName, oldCovered);
			}
			// Just put the item down (counter was empty)
			else
			{
				_storedItem = player.GetHeldItem().Data.ItemName;
				_covered    = Global.Instance.Arm.GetCoverVisible();

				ClearVisual();
				SpawnVisual(_storedItem, _covered);

				player.RemoveItem();
			}
		}

		// PLAYER IS NOT HOLDING ANYTHING
		else if (!string.IsNullOrEmpty(_storedItem))
		{
			var item = new Item { Data = ItemFactory.Instance.GetItem(_storedItem) };

			// give itemto player
			player.ReceiveItem(item);
			Global.Instance.Arm.SetActive(true, item.Data.ItemName, _covered);

			// clear the counter
			_storedItem = "";
			_covered    = false;
			ClearVisual();
			if (_coverSprite != null) _coverSprite.Visible = false;
		}
	}

	public void Store()
	{
		var player = Global.Instance.Player;
		var heldItem = player.GetHeldItem();
		if (heldItem == null) return;

		_storedItem = heldItem.Data.ItemName;
		_covered = Global.Instance.Arm.GetCoverVisible();

		ClearVisual();
		SpawnVisual(_storedItem, _covered);
	}

	public Item Grab()
	{
		if (string.IsNullOrEmpty(_storedItem))
			return null;

		// Remove existing visual
		foreach (Node child in GetChildren())
		{
			if (child is Item item)
			{
				RemoveChild(item);
				item.QueueFree();
				break;
			}
		}

		var newItem = new Item();
		newItem.Data = ItemFactory.Instance.GetItem(_storedItem);
		_storedItem = "";
		
		// Transfer covered state to arm
		Global.Instance.Arm.SetActive(true, newItem.Data.ItemName, _covered);
		_covered = false;

		if (_coverSprite != null)
			_coverSprite.Visible = false;
		
		return newItem;
	}
	
	private void SpawnVisual(string itemName, bool covered = false)
	{
		var itemVisual = new Item();
		itemVisual.Data = ItemFactory.Instance.GetItem(itemName);
		itemVisual.Position = ItemSpawnPoint.Position;
		AddChild(itemVisual);

		if (_coverSprite != null)
		{
			_coverSprite.Visible = covered;
			BringCoverToFront();  
		}
	}

	private void ClearVisual()
	{
		foreach (Node child in GetChildren())
		{
			if (child is Item item)
			{
				RemoveChild(item);
				item.QueueFree();
				break;
			}
		}
	}
	
	private void BringCoverToFront()
	{
		if (_coverSprite != null && _coverSprite.GetParent() == this)
			MoveChild(_coverSprite, GetChildCount() - 1);   // push to bottomo f tree = top of draw order???
	}
}
