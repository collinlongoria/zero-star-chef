using Godot;
using System;

public partial class Item : Sprite2D
{
    private ItemData _data;
    
    [Export]
    public ItemData Data
    {
        get => _data;
        set
        {
            _data = value;
            this.Texture = value.ItemSprite;
        }
    }

}
