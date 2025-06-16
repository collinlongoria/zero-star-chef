using Godot;
using System;

[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string ItemName { get; set; } = "New Item";
    [Export] public Texture2D ItemSprite { get; set; } = null;
    [Export] public 
}
