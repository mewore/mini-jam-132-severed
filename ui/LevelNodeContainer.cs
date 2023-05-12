using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class LevelNodeContainer : Node2D
{
    [Signal]
    public delegate void Refreshed();

    private readonly List<LevelNode> nodes = new List<LevelNode>();
    private Dictionary<int, LevelNode> nodeByLevel = new Dictionary<int, LevelNode>();
    public IReadOnlyCollection<LevelNode> Nodes => nodeByLevel.Values;

    private readonly List<LevelNodeLine> lines = new List<LevelNodeLine>();
    public IReadOnlyCollection<LevelNodeLine> Lines => lines;

    private Node lineContainer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (Node node in GetChildren())
        {
            if (node is LevelNode)
            {
                addNode(node as LevelNode);
            }
        }
        lineContainer = GetNode("Lines");
        refresh();
    }

    private void addNode(LevelNode node)
    {
        nodes.Add(node as LevelNode);
        if (Engine.EditorHint)
        {
            if (!node.IsConnected(nameof(LevelNode.Changed), this, nameof(refresh)))
            {
                node.Connect(nameof(LevelNode.Changed), this, nameof(refresh));
            }
        }
    }

    public void _on_LevelNodes_child_entered_tree(Node node)
    {
        if (!(Engine.EditorHint && node is LevelNode))
        {
            return;
        }
        addNode(node as LevelNode);
        refresh();
    }

    public void _on_LevelNodes_child_exiting_tree(Node node)
    {
        if (!(Engine.EditorHint && node is LevelNode))
        {
            return;
        }
        nodes.Remove(node as LevelNode);
        refresh();
    }

    private void refresh()
    {
        nodeByLevel.Clear();
        foreach (LevelNode node in nodes)
        {
            nodeByLevel[node.Level] = node;
        }
        int activeLineCount = 0;
        foreach (LevelNode node in nodes)
        {
            if (node.PreviousLevel == node.Level || !nodeByLevel.ContainsKey(node.PreviousLevel))
            {
                continue;
            }
            while (lines.Count <= activeLineCount)
            {
                LevelNodeLine newLine = new LevelNodeLine();
                lines.Add(newLine);
                lineContainer.AddChild(newLine);
            }
            lines[activeLineCount++].Update(nodeByLevel[node.PreviousLevel], node);
        }
        while (lines.Count > activeLineCount)
        {
            lines[lines.Count - 1].QueueFree();
            lines.RemoveAt(lines.Count - 1);
        }
        EmitSignal(nameof(Refreshed));
    }
}
