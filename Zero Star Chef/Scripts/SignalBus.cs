using Godot;
using System;

public partial class SignalBus : Node
{
    // singleton reference
    public static SignalBus Instance { get; private set; }
    
    /*
     * Signal Definitions
     */
    public event Action AddItemRequest;
    public event Action RemoveItemRequest;
    
    public event Action<string> DialogueRequest;
    
    public event Action DialogueFinished;

    public event Action ServedRecipeSpawned;

    public event Action BeginTrip;

    public event Action<string> RequestSceneSwitch;
    public event Action RequestShutdown;

    public event Action GameIsDone;
    
    /*
     * Signal Functions
     */
    public void EmitGameIsDone()
    {
        GameIsDone?.Invoke();
    }
    
    public void EmitRequestSceneSwitch(string scene)
    {
        RequestSceneSwitch?.Invoke(scene);
    }

    public void EmitRequestShutdown()
    {
        RequestShutdown?.Invoke();
    }
    
    public void EmitBeginTrip()
    {
        BeginTrip?.Invoke();
    }
    
    public void EmitAddItemRequest()
    {
        AddItemRequest?.Invoke();
    }

    public void EmitRemoveItemRequest()
    {
        RemoveItemRequest?.Invoke();
    }
    
    public void EmitDialogueRequest(string id)
    {
        DialogueRequest?.Invoke(id);
    }

    public void EmitDialogueFinished()
    {
        DialogueFinished?.Invoke();
    }

    public void EmitServedRecipeSpawned()
    {
        ServedRecipeSpawned?.Invoke();
    }

    public override void _Ready()
    {
        Instance = this;
        
        GD.Print("SignalBus Ready");
    }
}
