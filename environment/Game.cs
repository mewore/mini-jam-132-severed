using Godot;
using System;

public class Game : Node2D
{
    [Signal]
    public delegate void GameOver();

    [Signal]
    public delegate void RestartRequested();

    private Node2D timerLine;
    private Timer levelTimer;
    private Timer beatTimer;

    [Export]
    private PackedScene obstacleScene = null;

    private Node obstacleContainer;
    private Obstacle obstacleBeingCreated;
    private float obstacleCreationStartedAt;
    private float initialClosenessToBeat;

    [Export]
    private float obstacleSuccessLostPerBeat = .25f;

    [Export]
    private Curve obstacleSuccessByCloseness = null;

    [Export]
    private Gradient obstacleColourGradient = null;

    [Export]
    private int calculationResolution = 2;

    private SlimeMold mold;

    private CalculationOverlay calculationOverlay;
    private int analyzedSquares = 0;
    private int analyzedPixels = 0;
    private int successfulPixels = 0;

    private float _analysisProgress = 0f;
    [Export(PropertyHint.Range, "0,1")]
    private float analysisProgress
    {
        get => _analysisProgress; set
        {
            _analysisProgress = value;
            updateAnalysisProgress();
        }
    }

    private bool _active = true;

    [Export]
    private bool active
    {
        get => _active;
        set
        {
            _active = value;
            if (!value)
            {
                if (obstacleBeingCreated != null)
                {
                    obstacleBeingCreated.Place(0f);
                    obstacleBeingCreated = null;
                }
            }
        }
    }

    private string scoreTemplate;
    private Label scoreLabel;

    private float now = 0f;

    Vector2[][] shapes;

    public override void _Ready()
    {
        timerLine = GetNode<Node2D>("TimerLine");
        levelTimer = GetNode<Timer>("LevelTimer");
        beatTimer = GetNode<Timer>("BeatTimer");

        obstacleContainer = GetNode("Obstacles");

        mold = GetNode<SlimeMold>("SlimeMold");

        calculationOverlay = GetNode<CalculationOverlay>("CalculationOverlay");
        scoreLabel = GetNode<Label>("CanvasLayer/ScoreLabel");
        scoreTemplate = scoreLabel.Text;

        var polygonNodes = GetNode("Shapes").GetChildren();
        shapes = new Vector2[polygonNodes.Count][];
        for (int index = 0; index < polygonNodes.Count; index++)
        {
            shapes[index] = (polygonNodes[index] as Polygon2D).Polygon;
            for (int pointIndex = 0; pointIndex < shapes[index].Length; pointIndex++)
            {
                shapes[index][pointIndex] += (polygonNodes[index] as Polygon2D).GlobalPosition;
            }
        }
    }

    private void updateAnalysisProgress()
    {
        // No idea why I need to add one to this
        int targetAnalysis = Mathf.RoundToInt(_analysisProgress * calculationOverlay.SquareCount) + 1;
        var oldAnalyzedSquares = analyzedSquares;
        while (analyzedSquares < targetAnalysis)
        {
            Rect2 square = calculationOverlay.GetNextSquare();
            int currentSuccessfulPixels = 0;
            int currentPixels = 0;
            var currentResolution = calculationResolution;
            if (pointIsInShapes(square.Position) || pointIsInShapes(square.Position + square.Size)
                || pointIsInShapes(new Vector2(square.Position.x, square.Position.y + square.Size.y))
                || pointIsInShapes(new Vector2(square.Position.x + square.Size.x, square.Position.y)))
            {
                currentResolution *= 4;
            }
            Vector2 step = square.Size / Mathf.Max(calculationResolution - 1, 1);
            Vector2 firstPosition = square.Position + step / 2;
            float y = firstPosition.y;
            for (int row = 0; row < calculationResolution; row++, y += step.y)
            {
                float x = firstPosition.x;
                for (int col = 0; col < calculationResolution; col++, x += step.x)
                {
                    currentPixels++;
                    var point = new Vector2(x, y);
                    if (pointIsInShapes(point) == mold.Contains(point))
                    {
                        currentSuccessfulPixels++;
                    }
                }
            }

            calculationOverlay.SquareDone(square, (float)currentSuccessfulPixels / currentPixels);
            successfulPixels += currentSuccessfulPixels;
            analyzedPixels += currentPixels;
            analyzedSquares++;
        }
        if (oldAnalyzedSquares < analyzedSquares)
        {
            scoreLabel.Text = scoreTemplate.Replace("<SCORE>", getScore().ToString());
            scoreLabel.Visible = true;
        }
    }

    private bool pointIsInShapes(Vector2 point)
    {
        foreach (var shape in shapes)
        {
            if (Geometry.IsPointInPolygon(point, shape))
            {
                return true;
            }
        }
        return false;
    }

    public override void _Process(float delta)
    {
        if (!levelTimer.IsStopped())
        {
            timerLine.Scale = new Vector2(1f - levelTimer.TimeLeft / levelTimer.WaitTime, timerLine.Scale.y);
        }
        if (obstacleBeingCreated != null)
        {
            obstacleBeingCreated.Success = getLineSuccess(getCosmeticClosenessToBeat());
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        now += delta;
        if (!_active)
        {
            return;
        }
        if (obstacleBeingCreated != null && getLineSuccess(1f) <= Mathf.Epsilon)
        {
            obstacleBeingCreated.Place(0f);
            obstacleBeingCreated = null;
            obstacleContainer.GetNode<AudioStreamPlayer>("CreateFail").Play();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_active)
        {
            if (@event.IsActionPressed("create_line"))
            {
                switch (GetNode<AnimationPlayer>("AnimationPlayer").CurrentAnimation)
                {
                    case "end_game":
                        GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 4f;
                        break;
                    case "exit_prompt":
                        EmitSignal(nameof(GameOver));
                        break;
                }
            }
            else if (@event.IsActionPressed("restart"))
            {
                EmitSignal(nameof(RestartRequested));
            }
            return;
        }
        if (@event.IsActionPressed("create_line") && obstacleBeingCreated == null && obstacleScene != null)
        {
            obstacleBeingCreated = obstacleScene.Instance<Obstacle>();
            obstacleBeingCreated.ColourGradient = obstacleColourGradient;
            initialClosenessToBeat = getClosenessToBeat();
            obstacleContainer.AddChild(obstacleBeingCreated as Node);
            obstacleCreationStartedAt = now;
            obstacleContainer.GetNode<AudioStreamPlayer>("CreateStart").Play();
        }
        else if (@event.IsActionReleased("create_line") && obstacleBeingCreated != null)
        {
            if (now - obstacleCreationStartedAt < beatTimer.WaitTime * .5f)
            {
                GD.Print("Too soon!");
                obstacleBeingCreated.Place(0f);
                obstacleBeingCreated = null;
                obstacleContainer.GetNode<AudioStreamPlayer>("CreateFail").Play();
                return;
            }
            float success = getLineSuccess();

            obstacleBeingCreated.Place(success);
            if (obstacleBeingCreated is ObstacleLine)
            {
                var line = obstacleBeingCreated as ObstacleLine;
                var hasDamaged = false;
                foreach (var moldBranch in GetNode<SlimeMold>("SlimeMold").Branches)
                {
                    hasDamaged = hasDamaged || moldBranch.TestIntersection(line.Points[0], line.Points[1], success);
                }
                if (hasDamaged)
                {
                    mold.GetNode<AudioStreamPlayer>("Damage").Play();
                }
            }
            obstacleContainer.GetNode<AudioStreamPlayer>("CreateSuccess").Play();
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

    public void _on_LevelTimer_timeout()
    {
        GlobalSound.GetInstance(this).PlayClearLevel();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("end_game");
    }

    public void _on_AnimationPlayer_animation_finished(string animationName)
    {
        if (animationName == "end_game")
        {
            Global.TryWinLevel(getScore());
            analysisProgress = 1f;
            GetNode<AnimationPlayer>("AnimationPlayer").PlaybackSpeed = 1f;
            GetNode<AnimationPlayer>("AnimationPlayer").Play("exit_prompt");
        }
    }

    private int getScore()
    {
        return Mathf.RoundToInt((float)successfulPixels * 100f / analyzedPixels);
    }
}
