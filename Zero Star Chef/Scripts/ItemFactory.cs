using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Recipe
{
    public string Name;
    public string Cooker;
    public List<string> Ingredients;

    public Recipe(string name, string cooker, params string[] ingredients)
    {
        Name = name;
        Cooker = cooker;
        Ingredients = new List<string>(ingredients);
    }
}


public partial class ItemFactory : Node
{
    // Singleton reference
    public static ItemFactory Instance { get; private set; }

    private Dictionary<string, ItemData> _itemData = new Dictionary<string, ItemData>();

    private Dictionary<string, Recipe> _recipes = new();
    
    /*
     * Functions related to recipes
     */
    string GetKey(IEnumerable<string> ingredients)
    {
        var sorted = ingredients.OrderBy(i => i).ToArray();
        return string.Join(",", sorted);
    }
    
    public void AddRecipe(Recipe recipe){
        string key = GetKey(recipe.Ingredients);
        _recipes.Add(key, recipe);
    }
    
    public Recipe? FindRecipe(string ing1, string ing2, string ing3, string cookware)
    {
        string key = GetKey(new[] { ing1, ing2, ing3 });
        var curr = _recipes.TryGetValue(key, out Recipe? recipe) ? recipe : null;

        if (curr == null) return null;
        return (curr.Cooker == cookware) ? recipe : null;
    }
    
    public bool IsRecipe(string itemName)
    {
        // linearlinearlinearlinearlinear
        foreach (var r in _recipes.Values)
            if (r.Name == itemName) return true;

        return false;
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        LoadItemData();
        
        // Load in my recipes
        // Would do this serialized. but at this point
        // I have no time left to set all that up
        // so hardcoding it is!
        AddRecipe(new Recipe("Cheeseburger", "Stovetop","Bread", "Beef", "Cheese"));
        AddRecipe(new Recipe("House Salad", "Mixing Bowl","Mixed Lettuce", "Tomato", "House 'Sauce'"));
        AddRecipe(new Recipe("Fruit Salad", "Mixing Bowl","Mixed Lettuce", "Fruit Medley", "Pine Nuts"));
        AddRecipe(new Recipe("Cucumber Salad", "Mixing Bowl","Cucumber", "Vinegar", "Salt"));
        AddRecipe(new Recipe("Beef Wellington", "Oven","Pastry", "Beef", "Mushrooms"));
        AddRecipe(new Recipe("Chicken Marsala", "Oven","Chicken", "House 'Sauce'", "Mushrooms"));
        AddRecipe(new Recipe("Spaghetti With Balls", "Stovetop","Tomato", "Pasta", "Beef"));
        AddRecipe(new Recipe("Mac And Cheese", "Stovetop","Cheese", "Pasta", "Cream"));
        AddRecipe(new Recipe("Tomato Soup", "Stovetop","Tomato", "Cream", "Garlic"));
        AddRecipe(new Recipe("Stir Fry", "Stovetop","Mixed Vegetables", "Garlic", "Salt"));
        AddRecipe(new Recipe("Shortbread Cookies", "Oven","Salt", "Sugar", "Butter"));
        AddRecipe(new Recipe("Candied Figs", "Mixing Bowl","Fruit Medley", "Sugar", "House 'Sauce'"));
        AddRecipe(new Recipe("Blueberry Tart", "Oven","Fruit Medley", "Sugar", "Dough"));
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

    public bool ItemExists(string itemName)
    {
        return _itemData.ContainsKey(itemName);
    }

    // its the last day of the jam. i am no longer writing safe code.
    // i am evil now.
    public string GetItemDesc(string itemName)
    {
        return _itemData[itemName].ItemDescription;
    }

    public ItemData GetItem(string itemName)
    {
        if(_itemData.ContainsKey(itemName)) return _itemData[itemName];
        else return null;
    }
}
