using Godot;
using System;
using System.Collections.Generic;

public partial class ItemFactory : Node
{
    // Singleton reference
    public static ItemFactory Instance { get; private set; }

    private Dictionary<string, ItemData> _itemData = new Dictionary<string, ItemData>(); 
    
    public override void _Ready()
    {
        Instance = this;
    }
}
