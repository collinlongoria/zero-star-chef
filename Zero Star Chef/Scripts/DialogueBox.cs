using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueBox : Node2D
{
	private RichTextLabel _textNode = null;
	
	// string that contains full text
	private string _text = "";
	private int _currentCharIdx = 0;
	
	// stream that is displayed to screen
	private string _displayBuffer = "";

	private bool _instant = false;
	private float _scrollSpeed = 0.05f;

	private bool _running = false;
	private bool _finished = false;
	
	private DialogueNode _currentNode = null;
	private List<string> _results = new List<string>();
	private int _currentResultIdx = 0;

	private HBoxContainer _choiceContainer = null;
	private List<RichTextLabel> _choices = new List<RichTextLabel>();

	private Theme _labelTheme;
	
	public override void _Ready()
	{
		_textNode = GetNodeOrNull<RichTextLabel>("Ui/Text Container/Text");
		_choiceContainer = GetNodeOrNull<HBoxContainer>("Ui/Text Container/Choice Container");
		_labelTheme = GD.Load<Theme>("res://Resources/base_font.tres");
		
		// TODO: move this to proper place
		Global.Instance.DialogueBox = this;
		SignalBus.Instance.DialogueRequest += PlayDialogue;

		_choiceContainer.Visible = false;
		Visible = false;
	}

	private float _timer = 0.0f;
	public override void _Process(double delta)
	{
		if (!_instant && _running)
		{
			_timer += (float)delta;
			if (_timer >= _scrollSpeed)
			{
				_displayBuffer = _displayBuffer + _text[_currentCharIdx];
				_currentCharIdx++;
				
				_timer = 0.0f;
			}

			if (_displayBuffer.Length == _text.Length)
			{
				_running = false;
				_finished = true;
			}
		}
		
		_textNode.Text = _displayBuffer;
		
		// Input
		if (Input.IsActionJustPressed("interact") && Visible)
		{
			// If there is dialogue playing, skip to end
			if (_running)
			{
				_displayBuffer = _text;
				_running = false;
				_finished = true;
			}
			// If finished, then go to next (or end)
			else if (_finished)
			{
				var result = _results[_currentResultIdx];
				var resultOutput = _currentNode.GetResult(result);

				if (resultOutput != null)
				{
					if (resultOutput == "end")
					{
						SignalBus.Instance.EmitDialogueFinished();
						Visible = false;
					}
					else
					{
						PlayDialogue(resultOutput);
					}
				}

			}
		}
		
		// Choices
		if (_finished && _choiceContainer.Visible == false && _results.Count > 1)
		{
			ShowChoices();
		}
		
		// Input for choices
		if (_choiceContainer.Visible)
		{
			if (Input.IsActionJustPressed("move_left"))
			{
				_currentResultIdx = (_currentResultIdx + 1) % _results.Count;
				UpdateChoiceHighlight();
			}
			else if (Input.IsActionJustPressed("move_right"))
			{
				_currentResultIdx = (_currentResultIdx - 1 + _results.Count) % _results.Count;
				UpdateChoiceHighlight();
			}
		}
	}

	public void PlayDialogue(string id)
	{
		_currentNode = Dialogue.Instance.GetDialogueNode(id);
		GD.Print($"PlayDialogue called. New node is with ID: {_currentNode.Id}");
		if (_currentNode != null)
		{
			// Get available options
			_results.Clear();
			foreach (var key in _currentNode.Results.Keys)
			{
				_results.Add(key);
			}
			
			SetText(_currentNode.Content, false);
		}

		_currentResultIdx = 0;
		_choiceContainer.Visible = false;
		foreach(Node child in _choiceContainer.GetChildren())
			_choiceContainer.RemoveChild(child);
		_choices.Clear();
		_finished = false;
		Visible = true;
	}

	public void SetText(string text, bool instant)
	{
		_displayBuffer = "";
		_text = text;
		_currentCharIdx = 0;
		_instant = instant;

		if (_instant) _displayBuffer = _text;
		else _running = true;
	}

	private void ShowChoices()
	{
		_choiceContainer.Visible = true;
		_choices.Clear();
		foreach (Node child in _choiceContainer.GetChildren())
		{
			_choiceContainer.RemoveChild(child);
		}

		for (int i = 0; i < _results.Count; i++)
		{
			var label = new RichTextLabel();
			label.Text = _results[i];
			label.ScrollActive = false;
			label.ScrollFollowing = false;
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			label.FitContent = true;
			label.Theme = _labelTheme;
			_choiceContainer.AddChild(label);
			_choices.Add(label);
		}

		UpdateChoiceHighlight();
	}

	private void UpdateChoiceHighlight()
	{
		for (int i = 0; i < _choices.Count; i++)
		{
			if (i == _currentResultIdx)
				_choices[i].Text = $"> {_results[i]}";
			else
				_choices[i].Text = $"  {_results[i]}";
		}
	}
}
