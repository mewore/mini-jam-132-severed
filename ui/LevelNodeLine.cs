using Godot;
using System;

public class LevelNodeLine : Line2D, Selectable
{
    private const int EDGE_SELECTION_RADIUS = 64;
    public static readonly int EDGE_SELECTION_RADIUS_SQUARED = EDGE_SELECTION_RADIUS * EDGE_SELECTION_RADIUS;

    private const float THICKNESS = 10f;
    private const float HOVER_THICKNESS = 15f;
    private const float THICKNESS_DECAY = .1f;

    private const float UNHOVERED_OPACITY = .5f;

    private LevelNode from;
    public LevelNode From => from;

    private LevelNode to;
    public LevelNode To => to;

    private Label scoreLabel;

    private readonly Gradient gradient = new Gradient();

    public string ScenePath => Global.GetLevelScenePath(to.Level);

    public bool Hovered
    {
        set
        {
            if (value)
            {
                Width = HOVER_THICKNESS;
                SelfModulate = new Color(SelfModulate, 1f);
            }
            else
            {
                SelfModulate = new Color(SelfModulate, UNHOVERED_OPACITY);
            }
        }
    }

    public Color Color => from.LevelColor.LinearInterpolate(to.LevelColor, .5f);

    public bool Selectable => Global.LevelHasBeenCleared(from.Level);

    public int TargetLevel => to.Level;

    public override void _Ready()
    {
        Hovered = false;
        gradient.Colors = new Color[] { new Color(), new Color() };
        gradient.Offsets = new float[] { 0f, 1f };
        scoreLabel = GetNode<Label>("ScoreLabel");
    }

    public override void _Process(float delta)
    {
        if (Width > THICKNESS)
        {
            Width -= THICKNESS_DECAY * (Width - THICKNESS);
        }
    }

    public void Update(LevelNode previousNode, LevelNode targetNode)
    {
        Position = previousNode.Position;
        from = previousNode;
        to = targetNode;
        Points = new Vector2[]{
            Vector2.Zero,
            targetNode.Position - previousNode.Position
        };
        gradient.Colors = new Color[] { previousNode.LevelColor, targetNode.LevelColor };
        Visible = true;
        if (Selectable)
        {
            Gradient = gradient;
            scoreLabel.RectPosition = (Points[0] + Points[1] - scoreLabel.RectSize) * .5f;
        }
        else
        {
            Gradient = null;
            DefaultColor = new Color(.3f, .3f, .3f);
            Modulate = new Color(Modulate, .5f);
        }
        if (Global.LevelHasBeenCleared(to.Level))
        {
            scoreLabel.Visible = true;
            scoreLabel.Text = Global.GetLevelScore(to.Level).ToString();
        }
        else
        {
            scoreLabel.Visible = false;
        }
    }

    public void Deactivate()
    {
        from = to = null;
        Visible = false;
    }
}
