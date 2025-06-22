using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
    public static Global Instance { get; private set; }

    private Chef _player = null;
    private DialogueBox _dialogueBox = null;
    private Arm _arm = null;
    private Rating _rating = null;
    private TripEffect _tripEffect = null;
    
    private RandomNumberGenerator _rng = new();

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

    public Arm Arm
    {
        get { return _arm; }
        
        set { _arm = value; }
    }

    public Rating Rating
    {
        get { return _rating; }
        
        set { _rating = value; }
    }

    public TripEffect TripEffect
    {
        get { return _tripEffect; }
        
        set { _tripEffect = value; }
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("fullscreen_toggle"))
        {
            ToggleFullscreen();
        }

        if (_isQuitHeld)
        {
            _quitHeldTime += (float)delta;
            if (_quitHeldTime >= HoldRequired)
            {
                SignalBus.Instance.EmitRequestShutdown();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("quit"))
        {
            GD.Print("quit");
            _isQuitHeld = true;
            _quitHeldTime = 0f;
        }
        else if (@event.IsActionReleased("quit"))
        {
            _isQuitHeld = false;
            _quitHeldTime = 0f;
        }
    }

    private void ToggleFullscreen()
    {
        var mode = DisplayServer.WindowGetMode();
        DisplayServer.WindowSetMode(
            mode == DisplayServer.WindowMode.Fullscreen
                ? DisplayServer.WindowMode.Windowed
                : DisplayServer.WindowMode.Fullscreen);
    }

    private const float HoldRequired = 2.0f;
    private float _quitHeldTime = 0.0f;
    private bool _isQuitHeld = false;

    private int _score = 0;
    private int _numAdded = 0; // technically the other ah forget it

    public void AddScore(int score)
    {
        _score += score;
        _numAdded++;
    }

    public int GetScore()
    {
        // god i hate meself
        return (int)Math.Round(_score / (float)_numAdded);
    }
    
    // The game is divided into two acts
    // There are 13 recipes in the game: 10 dishes and 3 desserts
    // The first act has three tiers and covers all 10 dishes and 3 desserts once
    // The second act requires baking 8 dishes, randomly picked from the first three tiers
    
    private HashSet<string> _completedTierRecipes = new();
	
    private string[] _tierOneRecipes = new string[]
    {
        "House Salad",
        "Fruit Salad",
        "Cucumber Salad"
    };

    private string[] _tierTwoRecipes = new string[]
    {
        "Cheeseburger",
        "Beef Wellington",
        "Chicken Marsala",
        "Candied Figs",
        "Blueberry Tart"
    };

    private string[] _tierThreeRecipes = new string[]
    {
        "Spaghetti With Balls",
        "Mac And Cheese",
        "Tomato Soup",
        "Stir Fry",
        "Shortbread Cookies"
    };

    public int CurrentTier = 1;
    public int RecipesCooked = 0;

    private List<string> _activeTierRecipes = new();
    
    [Signal] public delegate void RecipeCountChangedEventHandler(int totalServed);
    [Signal] public delegate void BreakStartedEventHandler();
    private int _totalRecipesServed = 0;
    public  int TotalRecipesServed => _totalRecipesServed;
    
    private bool _breakInProgress = false;
    
    // Gets a new recipe that hasn't been assigned yet in the current tier.
    // Returns true and outputs the recipe name. False if all are completed.
    public bool TryGetNextRecipe(out string recipe)
    {
        if (_breakInProgress)
        {
            recipe = "";
            return false;
        }
        
        recipe = "";

        string[] tierRecipes = CurrentTier switch
        {
            1 => _tierOneRecipes,
            2 => _tierTwoRecipes,
            3 => _tierThreeRecipes,
            4 => null,
            _ => null
        };

        // Tier 4 = random from any prior tier
        if (CurrentTier == 4)
        {
            string[] allRecipes = CombineAllTiers();
            recipe = allRecipes[_rng.RandiRange(0, allRecipes.Length - 1)];
            return true;
        }

        // If all recipes in this tier are active, we can’t assign another yet
        if (tierRecipes == null || _activeTierRecipes.Count >= tierRecipes.Length)
        {
            return false;
        }

        // Assign one that's not already active
        foreach (var r in tierRecipes)
        {
            if (!_activeTierRecipes.Contains(r) && !_completedTierRecipes.Contains(r))
            {
                _activeTierRecipes.Add(r);
                recipe = r;
                return true;
            }
        }

        return false;
    }
    
    public void MarkRecipeCompleted(string recipe)
    {
        _activeTierRecipes.Remove(recipe);
        _completedTierRecipes.Add(recipe);
        RecipesCooked++;
        
        _totalRecipesServed++;
        EmitSignal(nameof(RecipeCountChanged), _totalRecipesServed);

        GD.Print($"{_totalRecipesServed.ToString()} Recipes served!");
        if (_totalRecipesServed == 13)
        {
            _breakInProgress = true;
            EmitSignal(nameof(BreakStarted));
        }

        if (_totalRecipesServed == 21)
        {
            SignalBus.Instance.EmitGameIsDone();
        }

        string[] tierRecipes = CurrentTier switch
        {
            1 => _tierOneRecipes,
            2 => _tierTwoRecipes,
            3 => _tierThreeRecipes,
            _ => null
        };

        // If no more active recipes and all are completed advance tier
        if (tierRecipes != null &&
            _activeTierRecipes.Count == 0 &&
            RecipesCooked >= tierRecipes.Length)
        {
            GD.Print($"Tier {CurrentTier} complete!");
            CurrentTier++;
            RecipesCooked = 0;
            _activeTierRecipes.Clear();
            _completedTierRecipes.Clear();
        }
    }

    private string[] CombineAllTiers()
    {
        var all = new List<string>();
        all.AddRange(_tierOneRecipes);
        all.AddRange(_tierTwoRecipes);
        all.AddRange(_tierThreeRecipes);
        return all.ToArray();
    }

    public bool IsBreakActive()
    {
        return _breakInProgress;
    }
}
