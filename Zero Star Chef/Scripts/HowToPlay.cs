using Godot;
using System;

public partial class HowToPlay : Node2D
{
    
    private AnimatedSprite2D _rules;
    private int _currIdx = 0;

    public override void _Ready()
    {
        _rules = GetNode<AnimatedSprite2D>("Rules");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("interact"))
        {
            _currIdx++;
            if (_currIdx >= 3)
            {
                SignalBus.Instance.EmitRequestSceneSwitch("res://Scenes/game.tscn");
            }
            else
            {
                _rules.Frame = _currIdx;
            }
        }
    }
}
