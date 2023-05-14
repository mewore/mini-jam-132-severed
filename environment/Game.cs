using Godot;
using System;

public class Game : Node2D
{
    private Node2D timerLine;
    private Timer levelTimer;
    private Timer beatTimer;

    [Export]
    private PackedScene obstacleScene;

    private Node obstacleContainer;
    private Obstacle obstacleBeingCreated;
    private float obstacleCreationStartedAt;
    private float initialClosenessToBeat;

    [Export]
    private float obstacleSuccessLostPerBeat = .25f;

    [Export]
    private Curve obstacleSuccessByCloseness;

    [Export]
    private Gradient obstacleColourGradient;

    private float now = 0f;

    public override void _Ready()
    {
        timerLine = GetNode<Node2D>("TimerLine");
        levelTimer = GetNode<Timer>("LevelTimer");
        beatTimer = GetNode<Timer>("BeatTimer");

        obstacleContainer = GetNode("Lines");
    }

    public override void _Process(float delta)
    {
        timerLine.Scale = new Vector2(1f - levelTimer.TimeLeft / levelTimer.WaitTime, timerLine.Scale.y);
        if (obstacleBeingCreated != null)
        {
            obstacleBeingCreated.Success = getLineSuccess(getCosmeticClosenessToBeat());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("create_line") && obstacleBeingCreated == null && obstacleScene != null)
        {
            obstacleBeingCreated = obstacleScene.Instance<Obstacle>();
            obstacleBeingCreated.ColourGradient = obstacleColourGradient;
            initialClosenessToBeat = getClosenessToBeat();
            obstacleContainer.AddChild(obstacleBeingCreated as Node);
            obstacleCreationStartedAt = now;
        }
        else if (@event.IsActionReleased("create_line") && obstacleBeingCreated != null)
        {
            obstacleBeingCreated.Place(getLineSuccess());
            obstacleBeingCreated = null;
        }
    }

    private float getLineSuccess() => getLineSuccess(getClosenessToBeat());

    private float getLineSuccess(float closenessToBeat)
    {
        float successLost = Mathf.Max((now - obstacleCreationStartedAt) / beatTimer.WaitTime - 1f, 0f) * obstacleSuccessLostPerBeat;
        return Mathf.Max((initialClosenessToBeat + closenessToBeat) * .5f - successLost, 0f);
    }

    private float getClosenessToBeat()
    {
        float closenessToBeat = Mathf.Abs(beatTimer.TimeLeft / beatTimer.WaitTime - .5f) * 2f;
        return obstacleSuccessByCloseness.InterpolateBaked(closenessToBeat);
    }

    private float getCosmeticClosenessToBeat()
    {
        float closenessToBeat = Mathf.Max(beatTimer.TimeLeft / beatTimer.WaitTime - .5f, 0f) * 2f;
        return obstacleSuccessByCloseness.InterpolateBaked(closenessToBeat);
    }
}
