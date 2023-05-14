using Godot;
using System;
using System.Collections.Generic;

public class SlimeMoldBranch : Line2D
{
    [Export]
    private string slimeMoldBranchScene = "res://entities/mold/SlimeMoldBranch.tscn";

    [Export]
    private float branchTimeMin = 1f;

    [Export]
    private float branchTimeMax = 5f;

    [Export]
    private float maxSpeed = 10f;

    [Export]
    private float forwardAcceleration = 5f;

    [Export]
    private float randomAcceleration = 10f;

    [Export]
    private float lineRepulsion = 50f;

    [Export(PropertyHint.Range, "0,180")]
    private float angleSnap = 15f;

    [Export]
    private float speedUp = 1f;

    private float initialSpeedUp;

    [Export]
    private int widthDecrement = 2;

    private float initialWidth;

    [Export]
    private float beatWidthMultiplier = 1.5f;

    [Export(PropertyHint.Range, "0,1")]
    private float widthDecay = 0.1f;

    [Export]
    private float beatSpeedMultiplier = 10.5f;

    [Export(PropertyHint.Range, "0,1")]
    private float speedDecay = 0.1f;

    private bool active = true;

    private Vector2 targetPoint;
    private Vector2 initialVelocity;
    private Vector2 velocity;
    private Vector2 realVelocity;

    private List<Vector2> currentPoints = new List<Vector2>(new Vector2[] { Vector2.Zero });

    [Export]
    private int raycastSamples = 5;

    [Export(PropertyHint.Range, "0,1")]
    private float visionRange = .5f;

    private RayCast2D rayCast;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (velocity == Vector2.Zero)
        {
            velocity = Vector2.Right.Rotated((GD.Randi() % 12) * Mathf.Pi / 6) * maxSpeed;
        }
        realVelocity = velocity;
        currentPoints.Add(realVelocity.Normalized());
        Points = currentPoints.ToArray();
        _on_DirectionChangeTimer_timeout();
        if (slimeMoldBranchScene != null)
        {
            GetNode<Timer>("BranchTimer").Start((float)GD.RandRange(branchTimeMin, branchTimeMax));
        }

        var beatTimers = GetTree().GetNodesInGroup("beat_timer");
        if (beatTimers.Count > 0)
        {
            (beatTimers[0] as Timer).Connect("timeout", this, nameof(_on_beat));
        }

        initialSpeedUp = speedUp;
        initialWidth = Width;

        rayCast = GetNode<RayCast2D>("RayCast2D");
    }

    public override void _Process(float delta)
    {
        if (Width != initialWidth)
        {
            Width = Mathf.Lerp(Width, initialWidth, widthDecay);
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!active)
        {
            return;
        }
        if (speedUp != initialSpeedUp)
        {
            speedUp = Mathf.Lerp(speedUp, initialSpeedUp, speedDecay);
        }
        delta *= speedUp;
        realVelocity += (targetPoint - (Position + currentPoints[currentPoints.Count - 1])).Normalized() * randomAcceleration * delta;
        realVelocity += realVelocity.Normalized() * forwardAcceleration * delta;
        realVelocity = realVelocity.LimitLength(maxSpeed);
        realVelocity += getRepulsion() * delta;
        realVelocity = realVelocity.LimitLength(maxSpeed);
        if (Mathf.Abs(realVelocity.AngleTo(velocity)) > angleSnap * Mathf.Pi / 180)
        {
            velocity = realVelocity;
            currentPoints.Add(currentPoints[currentPoints.Count - 1]);
        }
        currentPoints[currentPoints.Count - 1] += velocity * delta;
        Points = currentPoints.ToArray();
    }

    private Vector2 getRepulsion()
    {
        rayCast.Position = currentPoints[currentPoints.Count - 1];
        float angleRange = visionRange * Mathf.Pi;
        float angle = raycastSamples == 1 ? 0f : (-angleRange / 2);
        float angleStep = angleRange / (raycastSamples - 1);
        float motionAngle = velocity.Angle();
        Vector2 totalRepulsion = Vector2.Zero;

        for (int sample = 0; sample < raycastSamples; sample++, angle += angleStep)
        {
            rayCast.Rotation = motionAngle + angle;
            rayCast.ForceRaycastUpdate();
            if (!rayCast.IsColliding())
            {
                continue;
            }
            Vector2 obstacleNormal = rayCast.GetCollisionNormal();
            Vector2 raycastNormalized = rayCast.CastTo.Normalized().Rotated(rayCast.Rotation);
            Vector2 reflected = raycastNormalized - 2 * raycastNormalized.Dot(obstacleNormal) * obstacleNormal;
            float closeness = 1f - rayCast.GetCollisionPoint().DistanceTo(rayCast.GlobalPosition) / rayCast.CastTo.Length();
            closeness *= closeness;
            totalRepulsion += reflected * closeness;
        }
        return totalRepulsion / raycastSamples * lineRepulsion;
    }

    public void _on_BranchTimer_timeout()
    {
        active = false;
        if (Width <= widthDecrement)
        {
            return;
        }
        var scene = GD.Load<PackedScene>(slimeMoldBranchScene);
        var firstMold = scene.Instance<SlimeMoldBranch>();
        var secondMold = scene.Instance<SlimeMoldBranch>();

        firstMold.Position = secondMold.Position = Position + Points[Points.Length - 1];
        firstMold.velocity = secondMold.velocity = velocity;
        firstMold.Width = secondMold.Width = initialWidth - widthDecrement;
        if (GD.Randf() > 0.5f)
        {
            firstMold.Width = initialWidth;
            secondMold.Width = initialWidth / 2;
        }
        GetParent().AddChild(firstMold);
        if (secondMold.Width > 1f)
        {
            GetParent().AddChild(secondMold);
        }
    }

    public void _on_DirectionChangeTimer_timeout()
    {
        targetPoint = new Vector2(GD.Randf() * 1024, GD.Randf() * 600);
    }

    private void _on_beat()
    {
        speedUp = initialSpeedUp * beatSpeedMultiplier;
        Width = initialWidth * beatWidthMultiplier;
    }
}
