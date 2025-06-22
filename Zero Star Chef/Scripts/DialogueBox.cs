using Godot;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class DialogueBox : Node2D
{
	private RichTextLabel _textNode = null;
	
	// string that contains full text
	private string _text = "";
	private int _currentCharIdx = 0;
	
	// stream that is displayed to screen
	private string _displayBuffer = "";

	private bool _instant = false;
	private float _scrollSpeed = 0.015f;

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
					else if (resultOutput == "insert_item")
					{
						var curr = Global.Instance.Player.GetLastInteracted();
						if (curr.IsInGroup("Cooker") && curr.HasMethod("Add"))
						{
							curr.Call("Add");
							SignalBus.Instance.EmitRemoveItemRequest();
							
							SignalBus.Instance.EmitDialogueFinished();
							Visible = false;
						}
					}
					else if (resultOutput == "trash")
					{
						SignalBus.Instance.EmitRemoveItemRequest();
						
						SignalBus.Instance.EmitDialogueFinished();
						Visible = false;
					}
					else if (resultOutput == "empty_items")
					{
						var curr = Global.Instance.Player.GetLastInteracted();
						if (curr.IsInGroup("Cooker") && curr.HasMethod("Empty"))
						{
							curr.Call("Empty");
							
							SignalBus.Instance.EmitDialogueFinished();
							Visible = false;
						}
					}
					else if (resultOutput == "send")
					{
						// i do not care anymore
						// i can barely keep my eyes open
						// you should be grateful to be getting code at all
						Global.Instance.Player.GetLastInteracted().Call("PlaceDish");

						SignalBus.Instance.EmitDialogueFinished();
						Visible = false;
					}
					else if (resultOutput == "judged")
					{
						Global.Instance.Player.GetLastInteracted().Call("SubmitCorrectItem");
						
						SignalBus.Instance.EmitDialogueFinished();
						Visible = false;
					}
					else if (resultOutput == "grab_item")
					{
						// Player is trying to take an item.
						// First, determine if the player already has an item.
						// If so, prompt for switch. If they don't, just give it to them.
						var player = Global.Instance.Player;
						if (player.HasHeldItem() && !player.GetLastInteracted().IsInGroup("Cooker")) PlayDialogue("switch_prompt");

						else
						{
							SignalBus.Instance.EmitAddItemRequest();

							SignalBus.Instance.EmitDialogueFinished();
							Visible = false;
						}
					}
					else if (resultOutput == "end_surprise")
					{
						SignalBus.Instance.EmitBeginTrip();
						Global.Instance.TripEffect.Visible = true;
						
						SignalBus.Instance.EmitDialogueFinished();
						Visible = false;
					}
					else if (resultOutput == "switch_item")
					{
						SignalBus.Instance.EmitAddItemRequest();
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
			if (Input.IsActionJustPressed("move_right"))
			{
				_currentResultIdx = (_currentResultIdx + 1) % _results.Count;
				UpdateChoiceHighlight();
			}
			else if (Input.IsActionJustPressed("move_left"))
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

			var content = _currentNode.Content;
			var thing = Global.Instance.Player.GetLastInteracted();
			if (thing.IsInGroup("Cooker"))
			{
				var num_items = thing.Call("GetNumItems").AsInt32();
				var cooker_type = thing.Call("GetCookType").AsString();
				content = Regex.Replace(content, @"\bNUM_ITEMS\b", num_items.ToString());
				content = Regex.Replace(content, @"\bCOOKER_NAME\b", cooker_type);
			}

			if (thing is ServingCounter)
			{
				var item_name = thing.Call("GetServedItem").AsString();
				content = Regex.Replace(content, @"\bITEM_NAME\b", item_name);
			}
			
			SetText(content, false);
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
