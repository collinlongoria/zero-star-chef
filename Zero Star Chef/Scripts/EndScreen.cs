using Godot;
using System;

public partial class EndScreen : Node2D
{
    private Label _desc;
    private Rating _rating;
    private Button _exitButton;

    public override void _Ready()
    {
        var rating = Global.Instance.GetScore();
        
        _desc = GetNode<Label>("Label2");
        switch (rating)
        {
            case 0:
            default:
                _desc.Text = "You ARE the ZERO STAR CHEF. You LOSE. Thank you for playing!";
                break;
            case 1:
                _desc.Text = "You are a measly ONE STAR CHEF. You LOSE. Thank you for playing!";
                break;
            case 2:
                _desc.Text = "You are a pathetic TWO STAR CHEF. You LOSE. Thank you for playing!";
                break;
            case 3:
                _desc.Text = "You are a weakling THREE STAR CHEF. You LOSE. Thank you for playing!";
                break;
            case 4:
                _desc.Text = "You are a subpar FOUR STAR CHEF. You LOSE. Thank you for playing!";
                break;
            case 5:
                _desc.Text = "You maintained the rating of FIVE STAR CHEF. YOU WIN! Thank you for playing!";
                break;
        }
        _rating = GetNode<Rating>("Rating");
        _rating.DispatchRating(rating);
        _exitButton = GetNode<Button>("Button");
        _exitButton.Pressed += () =>
        {
            SignalBus.Instance.EmitRequestSceneSwitch("res://Scenes/menu.tscn");
        };
    }
}
