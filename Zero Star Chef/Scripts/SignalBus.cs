using Godot;
using System;

public partial class SignalBus : Node
{
    // singleton reference
    public static SignalBus Instance { get; private set; }
    
    /*
     * Signal Definitions
     */
    public event Action<string> AddItemRequest;
    
    public event Action<string> DialogueRequest;
    
    public event Action DialogueFinished;
    
    /*
     * Signal Functions
     */
    public void EmitAddItemRequest(string item)
    {
        AddItemRequest?.Invoke(item);
    }

    public void EmitDialogueRequest(string id)
    {
        DialogueRequest?.Invoke(id);
    }

    public void EmitDialogueFinished()
    {
        DialogueFinished?.Invoke();
    }

    public override void _Ready()
    {
        Instance = this;
        
        GD.Print("SignalBus Ready");
    }
}
