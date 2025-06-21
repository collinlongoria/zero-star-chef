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

    private void LoadItemData()
    {
        using var file = FileAccess.Open("res://Resources/items.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr("items json could not be loaded!");
            return;
        }

        var content = file.GetAsText();

        var json = new Json();
        var result = json.Parse(content);

        if (result != Error.Ok)
        {
            GD.PrintErr("items json not good");
            return;
        }

        var root = json.Data.AsGodotDictionary();
        if (!root.ContainsKey("items"))
        {
            GD.PrintErr("wrong items json??");
            return;
        }

        var itemsArray = root["items"].AsGodotArray();

        foreach (var itemVariant in itemsArray)
        {
            var itemDict = itemVariant.AsGodotDictionary();
            
            string name = itemDict.GetValueOrDefault("Name", "").AsString();
            string description = itemDict.GetValueOrDefault("Description", "").AsString();
            string spritePath = itemDict.GetValueOrDefault("Sprite", "").AsString();

            var itemData = new ItemData
            {
                ItemName = name,
                ItemDescription = description,
                ItemSprite = GD.Load<Texture2D>($"res://Assets/{spritePath}")
            };
            
            _itemData[name] = itemData;
        }
        
        GD.Print($"Loaded {_itemData.Count} items");
    }
}
