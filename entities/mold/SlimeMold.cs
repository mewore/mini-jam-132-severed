using Godot;
using System;
using System.Collections.Generic;

public class SlimeMold : Node2D
{
    [Export]
    private PackedScene moldScene = null;

    private readonly RandomNumberGenerator random = new RandomNumberGenerator();

    private readonly List<SlimeMoldBranch> branches = new List<SlimeMoldBranch>();
    public IReadOnlyList<SlimeMoldBranch> Branches => branches;

    public override void _Ready()
    {
        if (HasNode("SlimeMoldBranch"))
        {
            branches.Add(GetNode<SlimeMoldBranch>("SlimeMoldBranch"));
        }
    }

    public void _on_SpawnTimer_timeout()
    {
        if (moldScene != null)
        {
            var mold = moldScene.Instance<SlimeMoldBranch>();
            mold.Position = new Vector2(random.RandfRange(0f, 1024f), random.RandfRange(0f, 600f));
            branches.Add(mold);
            AddChild(mold);
        }
    }
}
