using Godot;
using System;

[Tool]
public class LevelNode : Node2D
{
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
            Modulate = _levelColor;
        }
    }
    public Color LevelColor => _levelColor;

    private Vector2 lastPosition;
    private string lastName;

    public override void _Ready()
    {
        Modulate = _levelColor;
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
