using Godot;
using System;

[GlobalClass, Tool]
public partial class ItemData : Resource
{
    [Export] public string ItemName { get; set; } = "New Item";
    
    [Export] public string ItemDescription { get; set; } = "new_item";
    [Export] public Texture2D ItemSprite { get; set; } = null;
}
