using Godot;
using System;
using Godot.Collections;

public partial class DialogueNode(string id = "", string content = "", Dictionary<string, string> results = null)
    : Resource
{
    public string Id = id;

    public string Content = content;

    public Dictionary<string, string> Results = results;

    public string GetResult(string key)
    {
        if(Results.ContainsKey(key)) return Results[key];
        else return null;
    }
}

public partial class Dialogue : Node
{
    public static Dialogue Instance;

    private Godot.Collections.Dictionary<string, DialogueNode> _dialogueNodes = new Godot.Collections.Dictionary<string, DialogueNode>();
    
    public override void _Ready()
    {
        Instance = this;
        
        LoadDialogue();
    }

    public DialogueNode GetDialogueNode(string id)
    {
        if(_dialogueNodes.ContainsKey(id)) return _dialogueNodes[id];
        else return null;
    }
    
    private void LoadDialogue()
    {
        using var file = FileAccess.Open("res://Resources/dialogue.json", FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr("Failed to load dialogue file");
            return;
        }
        
        var content = file.GetAsText();

        var json = new Json();
        var result = json.Parse(content);

        if (result != Error.Ok)
        {
            GD.PrintErr($"Failed to parse dialogue file. Error: {json.GetErrorMessage()}");
            return;
        }

        var root = json.Data.AsGodotDictionary();
        if (!root.ContainsKey("dialogue"))
        {
            GD.PrintErr("dialogue key not found");
            return;
        }
        
        var dialogueArray = root["dialogue"].AsGodotArray();

        foreach (var entry in dialogueArray)
        {
            var entryDict = entry.AsGodotDictionary();

            string id = entryDict.ContainsKey("id") ? entryDict["id"].AsString() : "";
            string contentText = entryDict.ContainsKey("content") ? entryDict["content"].AsString() : "";
            var results = new Dictionary<string, string>();
            if (entryDict.ContainsKey("results") && entryDict["results"].AsGodotDictionary() is Dictionary resultsDictRaw)
            {
                foreach (var key in resultsDictRaw.Keys)
                {
                    results[key.AsString()] = resultsDictRaw[key].AsString();
                }
            }

            var node = new DialogueNode(id, contentText, results);
            _dialogueNodes.Add(id, node);
        }
    }
}
