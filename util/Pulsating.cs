using Godot;
using System;

public class Pulsating : Node2D
{
    [Export(PropertyHint.Range, "0,1")]
    private float decay = .1f;

    [Export]
    private float multiplication = 1.5f;

    [Export]
    private float addition = 0f;

    private float initialOpacity;

    public override void _Ready()
    {
        var beatTimers = GetTree().GetNodesInGroup("beat_timer");
        if (beatTimers.Count > 0)
        {
            (beatTimers[0] as Timer).Connect("timeout", this, nameof(_on_beat));
        }

        initialOpacity = Modulate.a;
    }

    public override void _Process(float delta)
    {
        if (Modulate.a != initialOpacity)
        {
            Modulate = new Color(Modulate, Mathf.Lerp(Modulate.a, initialOpacity, decay));
        }
    }

    private void _on_beat()
    {
        Modulate = new Color(Modulate, initialOpacity * multiplication + addition);
    }
}
