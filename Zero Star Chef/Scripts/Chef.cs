using Godot;
using System;

public partial class Chef : CharacterBody2D
{
	enum Direction { Left, Right, Front, Back }
	enum State { Idle, Walking }
	
	[Export] public int Speed { get; set; } = 100;

	private AnimatedSprite2D _sprite;
	private Direction _direction = Direction.Front;
	private State _state = State.Idle;
	
	// save the last known state, so we don't update animation every frame
	private Direction _lastDirection;
	private State _lastState;

	private bool _inDialogue = false;
	
	private RayCast2D _rayCast;
	private Node _lastInteracted = null;
	
	// Current held item
	private Item _heldItem = null;

	private float _inputCooldown = 0f;

	private bool _hasPlayedBreakDialogue = false;

	private bool _trip = false;

	private float _gameIsDoneTimer = 0f;
	private bool _gameIsDone = false;

	public override void _Ready()
	{
		// MUST be done
		Global.Instance.Player = this;
		
		// Get animated sprite
		_sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		
		// Get raycast
		_rayCast = GetNodeOrNull<RayCast2D>("Interact Ray");

		SignalBus.Instance.AddItemRequest += AddItem;
		SignalBus.Instance.RemoveItemRequest += RemoveItem;
		SignalBus.Instance.GameIsDone += () =>
		{
			_gameIsDone = true;
		};
		SignalBus.Instance.BeginTrip += () => _trip = true;
		SignalBus.Instance.DialogueRequest += (string id) =>
		{
			_ = id; // no need it >:(
			_inDialogue = true;
		};
		SignalBus.Instance.DialogueFinished += () =>
		{
			_inDialogue = false; 
			_inputCooldown = 0.05f;
		};
	}

	private float _breakTimer = 0f;
	public override void _Process(double delta)
	{
		if (_gameIsDone)
		{
			if (_gameIsDoneTimer >= 5f)
				SignalBus.Instance.EmitRequestSceneSwitch("res://Scenes/end_screen.tscn");
			_gameIsDoneTimer += (float)delta;
		}
		
		if (Global.Instance.IsBreakActive() && !_hasPlayedBreakDialogue && !_inDialogue)
		{
			_breakTimer += (float)delta;
			
			if(_breakTimer >= 2.0f)
			{
				SignalBus.Instance.EmitDialogueRequest("break_time");
				_hasPlayedBreakDialogue = true;
			}
		}
		
		if (_sprite != null && (_direction != _lastDirection || _state != _lastState))
		{
			GetAnimationName();
			_lastDirection = _direction;
			_lastState = _state;
		}

		Speed = Input.IsActionPressed("run") ? 130 : 90;

		if (Input.IsActionJustPressed("interact") && !_inDialogue)
		{
			if (_inputCooldown > 0f)
			{
				_inputCooldown -= (float)delta;
				return;
			}
			
			if (_rayCast != null)
			{
				if (_rayCast.IsColliding())
				{
					var target = _rayCast.GetCollider() as Node;
					if (target != null && target.IsInGroup("Interactable"))
					{
						if(target.HasMethod("Interact"))
						{
							_lastInteracted = target;
							target.Call("Interact");
						}
						else
							GD.PrintErr("Interactable had no Interact Function.");
					}
				}
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Vector2.Zero;

		if (Input.IsActionPressed("move_left") && !_inDialogue)
		{
			velocity.X -= 1;
			_direction = Direction.Left;
		}
		else if (Input.IsActionPressed("move_right")&& !_inDialogue)
		{
			velocity.X += 1;
			_direction = Direction.Right;
		}

		if (Input.IsActionPressed("move_front") && !_inDialogue)
		{
			velocity.Y += 1;
			_direction = Direction.Front;
		}
		else if (Input.IsActionPressed("move_back") && !_inDialogue)
		{
			velocity.Y -= 1;
			_direction = Direction.Back;
		}
		
		if(_direction != _lastDirection)
			UpdateRayDirection();
		
		// Update state
		_state = velocity == Vector2.Zero ? State.Idle : State.Walking;

		if (velocity.Length() > 0)
		{
			if (_inputCooldown > 0f) _inputCooldown = 0f;
			velocity = velocity.Normalized() * Speed;
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}

	private void GetAnimationName()
	{
		string animationName = "";

		switch (_state)
		{
			case State.Idle:
				animationName = "idle_";
				break;
			case State.Walking:
				animationName = "walking_";
				break;
		}
		
		switch (_direction)
		{
			case Direction.Left:
				animationName += "left";
				break;
			case Direction.Right:
				animationName += "right";
				break;
			case Direction.Front:
				animationName += "front";
				break;
			case Direction.Back:
				animationName += "back";
				break;
		}
		
		if(_sprite.Animation != animationName)
			_sprite.Play(animationName);
	}

	private void UpdateRayDirection()
	{
		if (_rayCast != null)
		{
			switch (_direction)
			{
				case Direction.Left:
					_rayCast.TargetPosition = Vector2.Left * 10;
					break;
				case Direction.Right:
					_rayCast.TargetPosition = Vector2.Right * 10;
					break;
				case Direction.Front:
					_rayCast.TargetPosition = Vector2.Down * 10;
					break;
				case Direction.Back:
					_rayCast.TargetPosition = Vector2.Up * 10;
					break;
			}
		}
	}

	private void AddItem()
	{
		if (_heldItem != null)
		{
			// if storage, can swap out items
			if (_lastInteracted.IsInGroup("Storage"))
			{
				//RemoveChild(_heldItem);
				_lastInteracted.Call("HandleInteraction");
				return;
			}
			
			else RemoveChild(_heldItem);
		}
		
		// dont need to worry about the existing one
		var curr = new Item();
		if (_lastInteracted is Crate crate)
		{
			curr.Data = ItemFactory.Instance.GetItem(crate.ItemName);
		}
		else if (_lastInteracted is Shelf shelf)
		{
			curr.Data = ItemFactory.Instance.GetItem(shelf.ItemName);
		}
		else if (_lastInteracted is Box box)
		{
			curr.Data = ItemFactory.Instance.GetItem(box.ItemName);
		}
		else if (_lastInteracted.IsInGroup("Cooker"))
		{
			curr.Data = (ItemData)_lastInteracted.Call("Grab");
		}
		else if (_lastInteracted.IsInGroup("Storage"))
		{
			curr = (Item)_lastInteracted.Call("Grab");
		}
		else if (_lastInteracted is Platestack plate)
		{
			curr.Data = ItemFactory.Instance.GetItem("Plate");
		}
		
		_heldItem = curr;
		_heldItem.Visible = false;
		AddChild(curr);

		bool cover = false;
		if (_lastInteracted.IsInGroup("Cooker"))
			cover = true;
		else if (_lastInteracted is Countertop)
			cover = (curr != null) && Global.Instance.Arm.GetCoverVisible();

		Global.Instance.Arm.SetActive(true, curr.Data.ItemName, cover);
	}

	public void RemoveItem()
	{
		if (_heldItem != null)
		{
			RemoveChild(_heldItem);
			_heldItem = null;
			Global.Instance.Arm.SetActive(false);
		}
	}
	
	public void ReceiveItem(Item item)
	{
		_heldItem = item;
		_heldItem.Visible = false;
		AddChild(_heldItem);
		Global.Instance.Arm.SetActive(true, item.Data.ItemName);
	}
	
	public Item GetHeldItem()
	{
		return _heldItem;
	}

	public bool HasHeldItem()
	{
		return _heldItem != null;
	}

	public Node GetLastInteracted()
	{
		return _lastInteracted;
	}

	public bool IsInDialogue()
	{
		return _inDialogue;
	}

	public bool IsTrip()
	{
		return _trip;
	}
}
