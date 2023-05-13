using Godot;
using System;

public class SlimeMold : Node2D
{
    [Export]
    private PackedScene moldScene = null;

    private readonly RandomNumberGenerator random = new RandomNumberGenerator();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void _on_SpawnTimer_timeout()
    {
        if (moldScene != null)
        {
            var mold = moldScene.Instance<SlimeMoldBranch>();
            mold.Position = new Vector2(random.RandfRange(0f, 1024f), random.RandfRange(0f, 600f));
            AddChild(mold);
        }
    }
}
