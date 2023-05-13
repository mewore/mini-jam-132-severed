using Godot;
using System;

public class LevelNodeLine : Line2D, Selectable
{
    private const float THICKNESS = 10f;
    private const float HOVER_THICKNESS = 15f;
    private const float THICKNESS_DECAY = .1f;

    private const float UNHOVERED_OPACITY = .5f;

    private LevelNode from;
    public LevelNode From => from;

    private LevelNode to;
    public LevelNode To => to;

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

    public override void _Ready()
    {
        Gradient = new Gradient();
        Gradient.Colors = new Color[] { new Color(), new Color() };
        Gradient.Offsets = new float[] { 0f, 1f };
        Hovered = false;
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
        from = previousNode;
        to = targetNode;
        Points = new Vector2[]{
            previousNode.Position,
            targetNode.Position
        };
        Gradient.Colors = new Color[] { previousNode.LevelColor, targetNode.LevelColor };
        Visible = true;
    }

    public void Deactivate()
    {
        from = to = null;
        Visible = false;
    }
}
