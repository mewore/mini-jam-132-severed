using Godot;
using System;

public class Game : Node2D
{
    private Node2D timerLine;
    private Timer levelTimer;
    private Timer beatTimer;

    private Node lineContainer;
    private Line2D lineBeingCreated;
    private float lineCreationStartedAt;
    private float initialClosenessToBeat;

    [Export]
    private float lineSuccessLostPerBeat = .25f;

    [Export]
    private Curve lineSuccessByCloseness;

    [Export]
    private Gradient lineColourGradient;

    [Export]
    private float lineWidth = 10f;

    private float now = 0f;

    public override void _Ready()
    {
        timerLine = GetNode<Node2D>("TimerLine");
        levelTimer = GetNode<Timer>("LevelTimer");
        beatTimer = GetNode<Timer>("BeatTimer");

        lineContainer = GetNode("Lines");
    }

    public override void _Process(float delta)
    {
        timerLine.Scale = new Vector2(1f - levelTimer.TimeLeft / levelTimer.WaitTime, timerLine.Scale.y);
        if (lineBeingCreated != null)
        {
            lineBeingCreated.Points = new Vector2[] { lineBeingCreated.Points[0], GetGlobalMousePosition() };
            lineBeingCreated.DefaultColor = lineColourGradient.Interpolate(getLineSuccess(getCosmeticClosenessToBeat()));
            lineBeingCreated.Width = lineWidth * getLineSuccess(getCosmeticClosenessToBeat());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("create_line") && lineBeingCreated == null)
        {
            lineBeingCreated = new Line2D();
            lineBeingCreated.Points = new Vector2[] { GetGlobalMousePosition() };
            initialClosenessToBeat = getClosenessToBeat();
            lineBeingCreated.DefaultColor = lineColourGradient.Interpolate(initialClosenessToBeat);
            lineBeingCreated.Width = lineWidth * initialClosenessToBeat;
            lineContainer.AddChild(lineBeingCreated);
            lineCreationStartedAt = now;
        }
        else if (@event.IsActionReleased("create_line") && lineBeingCreated != null)
        {
            float lineSuccess = getLineSuccess();
            if (now - lineCreationStartedAt < beatTimer.WaitTime / 2 || lineSuccess < .01f)
            {
                lineBeingCreated.QueueFree();
            }
            else
            {
                lineBeingCreated.Points = new Vector2[] { lineBeingCreated.Points[0], GetGlobalMousePosition() };
                lineBeingCreated.DefaultColor = lineColourGradient.Interpolate(lineSuccess);
                lineBeingCreated.Width = lineWidth * getLineSuccess(lineSuccess);
            }
            lineBeingCreated = null;
        }
    }

    private float getLineSuccess() => getLineSuccess(getClosenessToBeat());

    private float getLineSuccess(float closenessToBeat)
    {
        float successLost = Mathf.Max((now - lineCreationStartedAt) / beatTimer.WaitTime - 1f, 0f) * lineSuccessLostPerBeat;
        return Mathf.Max((initialClosenessToBeat + closenessToBeat) * .5f - successLost, 0f);
    }

    private float getClosenessToBeat()
    {
        float closenessToBeat = Mathf.Abs(beatTimer.TimeLeft / beatTimer.WaitTime - .5f) * 2f;
        return lineSuccessByCloseness.InterpolateBaked(closenessToBeat);
    }

    private float getCosmeticClosenessToBeat()
    {
        float closenessToBeat = Mathf.Max(beatTimer.TimeLeft / beatTimer.WaitTime - .5f, 0f) * 2f;
        return lineSuccessByCloseness.InterpolateBaked(closenessToBeat);
    }
}
