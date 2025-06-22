using Godot;
using System;

public partial class ServingCounter : StaticBody2D
{
	[Export] public float MinWaitTime = 3f;
	[Export] public float MaxWaitTime = 10f;
	
	private string _currentRecipe = "";
	
	private Waiter _waiter = null;

	private bool _waiting = false;
	private float _startX;
	
	private AnimatedSprite2D _ticker = null;
	private float _tickerTime = 0f;
	
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	private float _waitTimer = 0f;
	private float _nextWaitTime = 0f;
	private float _spawnDelayOffset = 0f;
	
	[Export] public Node2D ItemSpawnPoint;
	[Export] public Sprite2D CoverSprite;
	
	private float _coverStartY;
	private string _servedItem = "";  
	
	public override void _Ready()
	{
		AddToGroup("Interactable");

		_waiter = GetNodeOrNull<Waiter>("Waiter");
		_startX = _waiter.Position.X;
		UpdateWaiter();
		
		_ticker = GetNodeOrNull<AnimatedSprite2D>("Ticker");
		_ticker.Visible = false;
		
		_rng.Randomize();

		ResetWaitTime();
		
		SignalBus.Instance.ServedRecipeSpawned += OnOtherCounterSummoned;
		
		if (ItemSpawnPoint == null) ItemSpawnPoint = this;
		if (CoverSprite    == null) CoverSprite    = GetNodeOrNull<Sprite2D>("Cover Sprite");
		if (CoverSprite != null)
		{
			_coverStartY       = CoverSprite.Position.Y;
			CoverSprite.Visible = false;
		}
	}

	private void ResetWaitTime()
	{
		_nextWaitTime = _rng.RandfRange(3f, 10f);
	}

	public override void _Process(double delta)
	{
		if (Global.Instance.Player.IsInDialogue()) return;
		
		if (_waiting)
		{
			_tickerTime += (float)delta;
			UpdateTicker();
		}
		else
		{
			_waitTimer += (float)delta;
			if (_waitTimer >= (_nextWaitTime + _spawnDelayOffset))
			{
				TrySummonWaiter();
				_waitTimer = 0f;
				_spawnDelayOffset = 0f;
			}
		}
	}
	
	private void TrySummonWaiter()
	{
		if (!Global.Instance.TryGetNextRecipe(out _currentRecipe))
		{
			// No recipes available right now
			ResetWaitTime();
			return;
		}

		_waiting = true;
		_tickerTime = 0f;
		_ticker.Visible = true;
		UpdateWaiter();

		// Tell other counters to delay slightly
		SignalBus.Instance.EmitServedRecipeSpawned();
	}

	private void UpdateTicker()
	{
		if (_tickerTime <= 15f) _ticker.Frame = 0;
		else if(_tickerTime <= 30f) _ticker.Frame = 1;
		else if(_tickerTime <= 45f) _ticker.Frame = 2;
		else if(_tickerTime <= 60f) _ticker.Frame = 3;
		else if(_tickerTime <= 75f) _ticker.Frame = 4;
		else if(_tickerTime <= 90f) _ticker.Frame = 5;
		else if(_tickerTime <= 105f) _ticker.Frame = 6;
		else if(_tickerTime <= 120f) _ticker.Frame = 7;
		else if(_tickerTime <= 135f) _ticker.Frame = 8;
		else if(_tickerTime <= 150f) _ticker.Frame = 9;
		else if(_tickerTime <= 165f) _ticker.Frame = 10;
		else if(_tickerTime <= 180f) _ticker.Frame = 11;
		else if(_tickerTime > 180f) _ticker.Frame = 12;
	}
	
	private void OnOtherCounterSummoned()
	{
		// Add small delay so we don’t all spawn waiters at once
		_spawnDelayOffset += _rng.RandfRange(1.5f, 3.5f);
	}
	
	private void UpdateWaiter()
	{
		float targetX = !_waiting ? _startX + (64f * _waiter.Scale.X) : _startX;

		var tween = CreateTween();
		tween.TweenProperty(_waiter, "position:x", targetX, 0.75f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}
	
	public void Interact()
	{
		if (!_waiting)
		{
			SignalBus.Instance.EmitDialogueRequest("no_server");
			return;
		}

		var player = Global.Instance.Player;
		if (player.HasHeldItem() && 
		    (ItemFactory.Instance.IsRecipe(player.GetHeldItem().Data.ItemName) ||
		     player.GetHeldItem().Data.ItemName == "Burnt Sh#t" ||
		     player.GetHeldItem().Data.ItemName == "Undercooked Slop" ||
		     player.GetHeldItem().Data.ItemName == "Unknown Junk"))
		{
			SignalBus.Instance.EmitDialogueRequest("submit_dish");
		}
		else
		{
			SignalBus.Instance.EmitDialogueRequest(ItemFactory.Instance.GetItem(_currentRecipe).ItemDescription);
		}
	}
	
	public string GetCurrentRecipeName()
	{
		return _currentRecipe;
	}

	public string GetServedItem()
	{
		return _servedItem;
	}
	
	private void PlaceDish()
	{
		_ticker.Visible = false;
		
		var player = Global.Instance.Player;
		var held   = player.GetHeldItem();
		if (held == null) return;   // sanity

		_servedItem = held.Data.ItemName;

		// Spawn the visual plate on the counter
		SpawnVisual(_servedItem);

		// Copy the lid the arm was showing
		bool covered = Global.Instance.Arm.GetCoverVisible();
		if (CoverSprite != null)
		{
			CoverSprite.Position = new Vector2(CoverSprite.Position.X, _coverStartY);
			CoverSprite.Visible  = covered;
		}

		// Clear the player’s hand/arm
		player.RemoveItem();
		Global.Instance.Arm.SetActive(false);

		// Animate the lid lifting (or go straight to judging if no lid)
		if (covered && CoverSprite != null)
		{
			var tween    = CreateTween();
			float targetY = _coverStartY - (72f * CoverSprite.Scale.Y);

			tween.TweenProperty(CoverSprite, "position:y", targetY, 0.40f)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase (Tween.EaseType.Out);

			tween.TweenCallback(Callable.From(JudgeDish));
		}
		else
		{
			JudgeDish();
		}
	}

	private int _cachedScore = 0;
	private void JudgeDish()
	{
		bool correct = _servedItem == _currentRecipe;
		SignalBus.Instance.EmitDialogueRequest(correct ? "correct_dish" : "incorrect_dish");
		
		// now actually judge the dish and add the score
		int score = 0;
		score += correct ? 3 : 0;
		if (_tickerTime <= 105f) score += 1;
		if (_tickerTime <= 165f) score += 1;
		Global.Instance.AddScore(score);
		
		GD.Print($"Score is now {Global.Instance.GetScore().ToString()}");
		_cachedScore = score;
	}
	
	public void SubmitCorrectItem()
	{
		Global.Instance.MarkRecipeCompleted(_currentRecipe);

		_currentRecipe = "";
		_waiting = false;
		_ticker.Visible = false;
		_tickerTime = 0f;

		UpdateWaiter();
		ResetWaitTime();
		
		ClearVisual();
		_servedItem = "";

		if (CoverSprite != null)
		{
			// snap lid back and hide it for the next order
			CoverSprite.Position = new Vector2(CoverSprite.Position.X, _coverStartY);
			CoverSprite.Visible  = false;
		}
		
		// dispatch rating to screen
		Global.Instance.Rating.DispatchRating(Math.Min(_cachedScore, 5)); // should never happen but idk
	}
	
	private void SpawnVisual(string itemName)
	{
		var itemVisual  = new Item();
		itemVisual.Data = ItemFactory.Instance.GetItem(itemName);
		itemVisual.Position = ItemSpawnPoint.Position;
		AddChild(itemVisual);
		
		// put the plate just underneath the cover sprite
		if (CoverSprite != null)
		{
			var lidParent = CoverSprite.GetParent();
			lidParent.MoveChild(CoverSprite, lidParent.GetChildCount() - 1);
		}
		
		GD.Print($"Spawned item: {itemName}");
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
}
