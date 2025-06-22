using Godot;
using System;

public partial class Rating : AnimatedSprite2D
{
	private float _startX;

	public override void _Ready()
	{
		_startX = Position.X;
		
		Global.Instance.Rating = this;
	}

	public void DispatchRating(int rating)
	{
		Frame = rating;

		float centerX = GetViewportRect().Size.X * 0.5f;
		float exitX   = (_startX < centerX)
			? GetViewportRect().Size.X + 100f 
			: -100f;

		var tween = CreateTween();
		
		tween.TweenProperty(this, "position:x", centerX, 0.6f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase (Tween.EaseType.Out);
		
		tween.TweenInterval(2.5f);
		
		tween.TweenProperty(this, "position:x", exitX, 0.6f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase (Tween.EaseType.In);
		
		tween.TweenCallback(Callable.From(() =>
		{
			Position = new Vector2(_startX, Position.Y);
		}));
	}
}
