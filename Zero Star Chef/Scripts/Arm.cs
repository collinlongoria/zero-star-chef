using Godot;
using System;

public partial class Arm : Node2D
{
	[Export]
	public bool Active
	{
		get => _active;

		set
		{
			if(_active == value) return;
			_active = value;
			AnimateArm();
		}
	}

	private bool _active = false;
	private float _startY;

	public override void _Ready()
	{
		_startY = Position.Y;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("interact"))
		{
			Active = !Active;
		}
	}

	private void AnimateArm()
	{
		float targetY = Active ? _startY - (48f * this.Scale.Y) : _startY;
		
		// create tween
		var tween = CreateTween();
		tween.TweenProperty(this, "position:y", targetY, 0.25f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}
}
