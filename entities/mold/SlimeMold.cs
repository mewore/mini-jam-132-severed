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
                foreach (var branch in branches)
                {
                    branch.Disable();
                }
            }
        }
    }

    private float _widthMultiplier = 1f;
    [Export]
    private float widthMultiplier
    {
        get => _widthMultiplier; set
        {
            _widthMultiplier = value;
            foreach (var branch in branches)
            {
                branch.SetWidthMultiplier(value);
            }
        }
    }

    public override void _Ready()
    {
        if (HasNode("SlimeMoldBranch"))
        {
            branches.Add(GetNode<SlimeMoldBranch>("SlimeMoldBranch"));
        }
    }

    public void _on_SpawnTimer_timeout()
    {
        if (active && moldScene != null)
        {
            var mold = moldScene.Instance<SlimeMoldBranch>();
            mold.Position = new Vector2(random.RandfRange(0f, 1024f), random.RandfRange(0f, 600f));
            branches.Add(mold);
            AddChild(mold);
        }
    }

    public bool Contains(Vector2 point)
    {
        foreach (var branch in branches)
        {
            if (branch.Contains(point))
            {
                return true;
            }
        }
        return false;
    }
}
