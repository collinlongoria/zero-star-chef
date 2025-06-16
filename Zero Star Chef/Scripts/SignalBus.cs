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
    
    /*
     * Signal Functions
     */
    public void EmitAddItemRequest(string item)
    {
        AddItemRequest?.Invoke(item);
    }

    public override void _Ready()
    {
        Instance = this;
        
        GD.Print("SignalBus Ready");
    }
}
