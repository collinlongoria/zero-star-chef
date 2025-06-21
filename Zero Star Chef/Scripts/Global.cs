using Godot;
using System;

public partial class Global : Node
{
    public static Global Instance { get; private set; }

    private Chef _player = null;
    private DialogueBox _dialogueBox = null;

    public Chef Player
    {
        get { return _player; }
        
        set { _player = value; }
    }

    public DialogueBox DialogueBox
    {
        get { return _dialogueBox; }
        
        set { _dialogueBox = value; }
    }
    
    public override void _Ready()
    {
        Instance = this;
    }
}
