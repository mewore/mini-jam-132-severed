using Godot;
using System;

public class Game : Node2D
{
    private Node2D timerLine;
    private Timer levelTimer;

    public override void _Ready()
    {
        timerLine = GetNode<Node2D>("TimerLine");
        levelTimer = GetNode<Timer>("LevelTimer");
    }

    public override void _Process(float delta)
    {
        timerLine.Scale = new Vector2(1f - levelTimer.TimeLeft / levelTimer.WaitTime, timerLine.Scale.y);
    }
}
