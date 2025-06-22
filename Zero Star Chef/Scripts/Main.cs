using Godot;
using System;
using System.Net.Sockets;

public partial class Main : Node2D
{
    private Node _currentScene;

    public override void _Ready()
    {
        SignalBus.Instance.RequestSceneSwitch += ChangeScene;
        SignalBus.Instance.RequestShutdown += Shutdown;

        SignalBus.Instance.EmitRequestSceneSwitch("res://Scenes/menu.tscn");
    }

    private void ChangeScene(string sceneName)
    {
        GD.Print("Scene changed to: " + sceneName);

        if (_currentScene != null)
        {
            _currentScene.QueueFree();
        }
        
        var packedScene = GD.Load<PackedScene>(sceneName);
        _currentScene = packedScene.Instantiate();
        AddChild(_currentScene);
    }

    private void Shutdown()
    {
        GD.Print("Shutting down...");
        GetTree().Quit();
    }
}
