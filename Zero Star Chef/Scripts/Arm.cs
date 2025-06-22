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

	private Sprite2D _itemSprite;
	private Sprite2D _coverSprite;

	private bool _covered = false;
	public override void _Ready()
	{
		_startY = Position.Y;

		_itemSprite = GetNodeOrNull<Sprite2D>("Main Sprite/Item Sprite");
		_coverSprite = GetNodeOrNull<Sprite2D>("Main Sprite/Item Sprite/Cover Sprite");
		_coverSprite.Visible = _covered;
		
		Global.Instance.Arm = this;
	}

	public override void _Process(double delta)
	{
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

	public void SetActive(bool active, string itemName = "", bool covered = false)
	{
		Active = active;
		if (itemName != "")
		{
			var sprite = ItemFactory.Instance.GetItem(itemName).ItemSprite;
			if (_itemSprite != null) _itemSprite.Texture = sprite;
			
			if(covered) _coverSprite.Visible = true;
			else _coverSprite.Visible = false;
		}
		else
		{
			if (_itemSprite != null) _itemSprite.Texture = null;
			if(_coverSprite != null) _coverSprite.Visible = false;
		}
	}
	
	public bool GetCoverVisible()
	{
		return _coverSprite?.Visible ?? false;
	}
}
