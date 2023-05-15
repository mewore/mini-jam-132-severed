using System;
using System.Collections.Generic;
using Godot;

public class MainMenu : VBoxContainer
{
    private float now = 0f;

    private Selectable hoveredNode;
    private IReadOnlyCollection<LevelNode> nodes = new List<LevelNode>();
    private IReadOnlyCollection<LevelNodeLine> lines = new List<LevelNodeLine>();

    private Color targetBackgroundGlowColor;
    private Color initialBackgroundGlowColor;
    private Node2D backgroundGlow;
    private const float BACKGROUND_GLOW_CHANGE_SPEED = .025f;

    public override void _Ready()
    {
        Global.LoadGameData();
        Global.SaveGameData();

        backgroundGlow = GetNode<Node2D>("Background/BackgroundGlow");
        targetBackgroundGlowColor = initialBackgroundGlowColor = backgroundGlow.SelfModulate;
        GlobalSound.GetInstance(this).MainMenuMusic = GlobalSound.GetInstance(this).AmbienetNoiseLab = true;
    }

    public override void _Process(float delta)
    {
        now += delta;
        if (backgroundGlow.SelfModulate != targetBackgroundGlowColor)
        {
            backgroundGlow.SelfModulate = backgroundGlow.SelfModulate.LinearInterpolate(targetBackgroundGlowColor, BACKGROUND_GLOW_CHANGE_SPEED);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!(@event is InputEventMouse))
        {
            return;
        }
        Selectable oldHoveredNode = hoveredNode;
        hoveredNode = getHovered((@event as InputEventMouse).GlobalPosition);
        if (hoveredNode != oldHoveredNode)
        {
            if (hoveredNode != null)
            {
                hoveredNode.Hovered = true;
            }
            if (oldHoveredNode != null)
            {
                oldHoveredNode.Hovered = false;
            }
            MouseDefaultCursorShape = hoveredNode != null ? CursorShape.PointingHand : CursorShape.Arrow;
            targetBackgroundGlowColor = hoveredNode == null ? initialBackgroundGlowColor : hoveredNode.Color;
        }
        if (@event.IsActionPressed("ui_navigate") && hoveredNode != null)
        {
            GD.Print("Navigating to: " + hoveredNode.ScenePath);
            Global.CurrentLevel = hoveredNode.TargetLevel;
            GetNode<Overlay>("Overlay").RequestTransition(hoveredNode.ScenePath);
            GlobalSound.GetInstance(this).PlayEnterLevel();
            GlobalSound.GetInstance(this).MainMenuMusic = GlobalSound.GetInstance(this).AmbienetNoiseLab = false;
        }
    }


    private Selectable getHovered(Vector2 position)
    {
        float bestDistanceSquared = Mathf.Max(LevelNodeLine.EDGE_SELECTION_RADIUS_SQUARED, LevelNode.SELECTION_RADIUS_SQUARED);
        Selectable result = null;
        foreach (LevelNode node in nodes)
        {
            if (node.Selectable)
            {
                float distanceSquared = node.GlobalPosition.DistanceSquaredTo(position);
                if (distanceSquared < LevelNode.SELECTION_RADIUS_SQUARED && distanceSquared < bestDistanceSquared)
                {
                    result = node;
                    bestDistanceSquared = distanceSquared;
                }
            }
        }
        if (result != null)
        {
            return result;
        }
        foreach (LevelNodeLine line in lines)
        {
            if (line.Selectable)
            {
                float distanceSquared = (line.GlobalPosition + (line.Points[0] + line.Points[1]) / 2).DistanceSquaredTo(position);
                if (distanceSquared < LevelNodeLine.EDGE_SELECTION_RADIUS_SQUARED && distanceSquared < bestDistanceSquared)
                {
                    result = line;
                    bestDistanceSquared = distanceSquared;
                }
            }
        }
        return result;
    }

    public void _on_LevelNodes_Refreshed()
    {
        var container = GetNode<LevelNodeContainer>("LevelNodes");
        nodes = container.Nodes;
        lines = container.Lines;
    }

    public void _on_Overlay_DoneDisappearing()
    {
        GetTree().Paused = false;
    }
}
