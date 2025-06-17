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
	
	private RayCast2D _rayCast;
	
	// Current held item
	private Item _heldItem = null;
	

	public override void _Ready()
	{
		// MUST be done
		Global.Instance.Player = this;
		
		// Get animated sprite
		_sprite = GetNodeOrNull<AnimatedSprite2D>("Sprite");
		
		// Get raycast
		_rayCast = GetNodeOrNull<RayCast2D>("Interact Ray");

		SignalBus.Instance.AddItemRequest += AddItem;
	}

	public override void _Process(double delta)
	{
		if (_sprite != null && (_direction != _lastDirection || _state != _lastState))
		{
			GetAnimationName();
			_lastDirection = _direction;
			_lastState = _state;
		}

		if (Input.IsActionJustPressed("interact"))
		{
			if (_rayCast != null)
			{
				if (_rayCast.IsColliding())
				{
					var target = _rayCast.GetCollider() as Node;
					if (target != null && target.IsInGroup("Interactable"))
					{
						if(target.HasMethod("Interact"))
							target.Call("Interact");
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

		if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= 1;
			_direction = Direction.Left;
		}
		else if (Input.IsActionPressed("move_right"))
		{
			velocity.X += 1;
			_direction = Direction.Right;
		}

		if (Input.IsActionPressed("move_front"))
		{
			velocity.Y += 1;
			_direction = Direction.Front;
		}
		else if (Input.IsActionPressed("move_back"))
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

	private void AddItem(string item)
	{
		if (_heldItem != null)
		{
			GD.Print("Chef already has an item!");
			return;
		}

		if (item == "Plate")
		{
			var curr = ResourceLoader.Load("res://Resources/Items/Plate.tres") as ItemData;
			var plate = new Item();
			plate.Data = curr;
			AddChild(plate);
			_heldItem = plate;
		}
	}
	
	public Item GetHeldItem()
	{
		return _heldItem;
	}

	public bool HasHeldItem()
	{
		return _heldItem != null;
	}
}
