using Godot;
using System;

public partial class Clock : Sprite2D
{
	[Export] public NodePath HandJointPath;
	[Export] public float  LerpTime  = 0.6f;
	private Node2D _handJoint;
	private Tween  _tween;

	private const int DISHES_BEFORE_BREAK = 13;
	private const int DISHES_AFTER_BREAK  = 8;

	private readonly float _startRad = Mathf.DegToRad(-180f);
	private readonly float _midRad   = Mathf.DegToRad(0f);
	private readonly float _endRad   = 180f;

	public override void _Ready()
	{
		_handJoint = GetNode<Node2D>(HandJointPath);
		_handJoint.Rotation = _startRad;
		
		Global.Instance.Connect(nameof(Global.RecipeCountChanged),
								Callable.From<int>(OnRecipeCountChanged));

		Global.Instance.Connect(nameof(Global.BreakStarted),
								Callable.From(OnBreakStarted));
	}
	
	private void OnRecipeCountChanged(int totalServed)
	{
		float target;

		if (totalServed <= DISHES_BEFORE_BREAK)
		{
			float t = (float)totalServed / DISHES_BEFORE_BREAK;
			target  = Mathf.Lerp(_startRad, _midRad, t);
		}
		else 
		{
			float t = (float)(totalServed - DISHES_BEFORE_BREAK) / DISHES_AFTER_BREAK;
			target  = Mathf.Lerp(_midRad, _endRad, t);
		}

		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_handJoint, "rotation", target, LerpTime)
			  .SetTrans(Tween.TransitionType.Sine)
			  .SetEase (Tween.EaseType.InOut);
	}
	
	private void OnBreakStarted()
	{
		SignalBus.Instance.EmitDialogueRequest("break_time");
	}
}
