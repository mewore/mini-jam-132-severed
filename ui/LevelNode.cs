using Godot;
using System;

[Tool]
public class LevelNode : Node2D, Selectable
{
    private const float BOUNCE = .5f;
    private const float BOUNCE_DECAY = .1f;
    private const float SELECTION_RADIUS = 24f;
    public const float SELECTION_RADIUS_SQUARED = SELECTION_RADIUS * SELECTION_RADIUS;

    private static readonly Color UNSELECTABLE_COLOR = new Color(.5f, .5f, .5f);

    [Signal]
    public delegate void Changed();

    private int _level;

    [Export(PropertyHint.Range, "0,100")]
    private int level
    {
        get => _level; set
        {
            if (value == 0 || Global.LevelExists(value))
            {
                _level = value;
                if (HasNode("Label"))
                {
                    GetNode<Label>("Label").Text = value.ToString();
                }
                EmitSignal(nameof(Changed));
            }
        }
    }
    public int Level => _level;

    private int _previousLevel;

    [Export(PropertyHint.Range, "0,100")]
    private int previousLevel
    {
        get => _previousLevel; set
        {
            if (value != _previousLevel && Global.LevelExists(value))
            {
                _previousLevel = value;
                EmitSignal(nameof(Changed));
            }
        }
    }
    public int PreviousLevel => _previousLevel;

    private Color _levelColor = new Color(1f, 1f, 1f, 1f);

    [Export]
    private Color levelColor
    {
        get => _levelColor; set
        {
            _levelColor = value;
            EmitSignal(nameof(Changed));
            if (sprite != null)
            {
                SelfModulate = _levelColor;
            }
        }
    }
    public Color LevelColor => _levelColor;

    public string ScenePath => Global.GetBackgroundScenePath(level);

    public Color Color => _levelColor;

    public bool Hovered
    {
        set
        {
            if (value)
            {
                Scale = Vector2.One * (1f + BOUNCE);
            }
        }
    }

    public bool Selectable => (Engine.EditorHint || Global.LevelHasBeenCleared(level)) && Global.BackgroundExists(level);

    public int TargetLevel => level;

    private Vector2 lastPosition;
    private string lastName;
    private Sprite sprite;

    public override void _Ready()
    {
        sprite = GetNode<Sprite>("Sprite");
        sprite.SelfModulate = _levelColor;
        Modulate = Selectable ? new Color(1f, 1f, 1f) : UNSELECTABLE_COLOR;
        lastPosition = Position;
        checkName();
    }

    public override void _Process(float delta)
    {
        if (Engine.EditorHint && Position != lastPosition)
        {
            lastPosition = Position;
            EmitSignal(nameof(Changed));
        }
        if (Scale.x > 1f)
        {
            Scale = new Vector2(Mathf.Lerp(Scale.x, 1f, BOUNCE_DECAY), Mathf.Lerp(Scale.y, 1f, BOUNCE_DECAY));
        }
        checkName();
    }

    private void checkName()
    {
        if (Engine.EditorHint && Name != lastName)
        {
            var digitsAtEnd = getDigitsAtEnd(Name);
            if (digitsAtEnd != "")
            {
                level = digitsAtEnd.ToInt();
            }
            lastName = Name;
        }
    }

    private static string getDigitsAtEnd(string input)
    {
        int numberOfDigitsAtEnd = 0;
        for (var i = input.Length - 1; i >= 0 && char.IsNumber(input[i]); i--, numberOfDigitsAtEnd++) ;
        return numberOfDigitsAtEnd > 0 ? input.Substring(input.Length - numberOfDigitsAtEnd) : "";
    }
}
