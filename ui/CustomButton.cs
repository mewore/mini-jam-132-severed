using Godot;

[Tool]
public class CustomButton : HBoxContainer
{
    private const string GROUP_NAME = "custom_buttons";

    [Signal]
    public delegate void Pressed();

    private string text = "Start";

    [Export]
    public string Text
    {
        get => text;
        set
        {
            GetNode<Label>("Label").Text = text = value;
        }
    }

    private bool enabled = true;
    [Export]
    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;
            if (enabled)
            {
                MouseDefaultCursorShape = CursorShape.PointingHand;
            }
            else
            {
                MouseDefaultCursorShape = CursorShape.Arrow;
            }
            FocusMode = enabled ? FocusModeEnum.All : FocusModeEnum.None;
            MouseFilter = enabled ? MouseFilterEnum.Pass : MouseFilterEnum.Ignore;
            Modulate = new Color(Modulate, enabled ? 1f : .5f);
        }
    }

    private AnimationPlayer animationPlayer;

    private bool active = false;
    public bool Active
    {
        get => active; set
        {
            if (value == active)
            {
                return;
            }
            active = value && enabled;
            animationPlayer.Play(active ? "active" : "inactive");
            if (active)
            {
                GrabFocus();
            }
            else
            {
                ReleaseFocus();
            }
        }
    }

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Active = active && enabled;
        Enabled = enabled;
        AddToGroup(GROUP_NAME, true);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_select") || @event.IsActionPressed("ui_accept"))
        {
            EmitSignal(nameof(Pressed));
        }
        else if (@event is InputEventMouseMotion)
        {
            foreach (CustomButton button in GetTree().GetNodesInGroup(GROUP_NAME))
            {
                button.Active = button == this;
            }
        }
        else
        {
            GetParent<Control>()._GuiInput(@event);
        }
    }
}
