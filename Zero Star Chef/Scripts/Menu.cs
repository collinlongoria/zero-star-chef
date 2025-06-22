using Godot;
using System;

public partial class Menu : Node2D
{
    private Button _playButton;
    private Button _creditsButton;
    private Button _exitButton;
    
    private Button _exitCreditsButton;

    private bool _inCredits = false;

    private CanvasLayer _creditsLayer;
    private CanvasLayer _mainLayer;
    private Control _buttons;
    private AnimatedSprite2D _logo;
    
    public override void _Ready()
    {
        _creditsLayer = GetNode<CanvasLayer>("Credits");
        _mainLayer = GetNode<CanvasLayer>("Main");
        
        _logo = GetNode<AnimatedSprite2D>("Main/Logo");
        
        _buttons = GetNode<Control>("Main/Control");
        _buttons.Visible = false;
        
        _playButton = GetNode<Button>("Main/Control/HBoxContainer/PlayButton");
        _playButton.Pressed += () =>
        {
            SignalBus.Instance.EmitRequestSceneSwitch("res://Scenes/how_to_play.tscn");
        };
        _creditsButton = GetNode<Button>("Main/Control/HBoxContainer/CreditsButton");
        _creditsButton.Pressed += () =>
        {
            _inCredits = true;
            _creditsLayer.Visible = true;
            _mainLayer.Visible = false;
        };
        
        _exitButton = GetNode<Button>("Main/Control/HBoxContainer/ExitButton");
        _exitButton.Pressed += () =>
        {
            SignalBus.Instance.EmitRequestShutdown();
        };
        
        _exitCreditsButton = GetNode<Button>("Credits/Control/Button");
        _exitCreditsButton.Pressed += () =>
        {
            _inCredits = false;
            _creditsLayer.Visible = false;
            _mainLayer.Visible = true;
        };
    }

    public override void _Process(double delta)
    {
        if (!_buttons.Visible && _logo != null && _logo.Frame == 6)
        {
            _buttons.Visible = true;
        }
    }
}
