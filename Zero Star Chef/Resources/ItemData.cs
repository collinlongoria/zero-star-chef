using Godot;
using System;

[GlobalClass, Tool]
public partial class ItemData : Resource
{
    [Export] public string ItemName { get; set; } = "New Item";
    [Export] public Texture2D ItemSprite { get; set; } = null;
}
