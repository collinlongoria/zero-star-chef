using Godot;
using System;

public partial class MixingBowl : StaticBody2D
{
	private string _itemOne = "";
	private string _itemTwo = "";
	private string _itemThree = "";

	private bool _cooking = false;
	private float _timer = 0f;

	private AnimatedSprite2D _sprite = null;
	private CookingParticles _particles = null;

	private const float CookTime = 8f;
	private const float BurnTime = 20f;
	
	public override void _Ready()
	{
		AddToGroup("Interactable");
		AddToGroup("Cooker");

		_sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		_particles = GetNodeOrNull<CookingParticles>("Cooking Particles");
	}

	public override void _Process(double delta)
	{
		if(_cooking && !Global.Instance.Player.IsInDialogue()) _timer += (float)delta;
		else if (!_cooking && _timer != 0f) _timer = 0f;
		
		if(_timer >= CookTime && _timer <= BurnTime) _particles.SetParticleType(CookingParticles.ParticleType.SPARKLE);
		else if(_timer > BurnTime) _particles.SetParticleType(CookingParticles.ParticleType.SMOKE);
	}

	public void Interact()
	{
		var player = Global.Instance.Player;
		
		// Is the cooker cooking?
		if (_itemOne != "" && _itemTwo != "" && _itemThree != "")
		{
			if(player.HasHeldItem() && player.GetHeldItem().Data.ItemName == "Plate")
				SignalBus.Instance.EmitDialogueRequest("cooker_cooking");
			else
				SignalBus.Instance.EmitDialogueRequest("cooker_plate_hint");
			
			return;
		}
		// does the player have an item?
		if (!player.HasHeldItem())
		{
			SignalBus.Instance.EmitDialogueRequest("cooker_no_item");
		}
		else
		{
			SignalBus.Instance.EmitDialogueRequest("cooker_item");
		}
	}

	public void Empty()
	{
		_itemOne = "";
		_itemTwo = "";
		_itemThree = "";
	}

	public ItemData Grab()
	{
		var ing1 = _itemOne;
		var ing2 = _itemTwo;
		var ing3 = _itemThree;
		
		Empty();
		_cooking = false;
		_sprite.Frame = 0;
		_particles.SetParticleType(CookingParticles.ParticleType.OFF);
		
		// first, the item could be undercooked
		if (_timer < CookTime) return ItemFactory.Instance.GetItem("Undercooked Slop");
		
		// next, the item could be burnt
		if(_timer > BurnTime) return ItemFactory.Instance.GetItem("Burnt Sh#t");
		
		// if not, it is simply a matter of determining if a recipe was made
		var recipe = ItemFactory.Instance.FindRecipe(ing1, ing2, ing3, "Mixing Bowl");
		if (recipe != null) return ItemFactory.Instance.GetItem(recipe.Name);
		else return ItemFactory.Instance.GetItem("Unknown Junk");
	}

	public void Add()
	{
		var item = Global.Instance.Player.GetHeldItem();
		
		if(item.Data.ItemName == "Plate")
		{
			SignalBus.Instance.EmitDialogueRequest("cooker_plate");
			return;
		}

		// add to first slot
		if (_itemOne == "")
		{
			_itemOne = item.Data.ItemName;
		}
		// add to second slot
		else if (_itemTwo == "")
		{
			_itemTwo = item.Data.ItemName;
		}
		// add to third slot
		else if (_itemThree == "")
		{
			_itemThree = item.Data.ItemName;
		}
		
		// now check if we start cooking
		if (_itemOne != "" && _itemTwo != "" && _itemThree != "")
		{
			_cooking = true;
			_sprite.Frame = 1;
		}
	}

	public int GetNumItems()
	{
		int count = 0;
		
		if(_itemOne != "") count++;
		if(_itemTwo != "") count++;
		if(_itemThree != "") count++;

		return count;
	}

	// This is literally the only difference between the three cookers LMAO
	// could have totally just done this better 
	// im quitting programming after this
	public string GetCookType()
	{
		return "Mixing Bowl";
	}
}
