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
    private float speed = 10f;

    private float angleChangeSpeed = 0f;

    [Export(PropertyHint.Range, "0,180")]
    private float maxAngleSpeed = 180f;

    [Export]
    private float maxAngleAcceleration = 15f;

    private float currentAngleAcceleration = 0f;

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

    private Vector2 motion;
    private float realAngle;

    private List<Vector2> currentPoints = new List<Vector2>(new Vector2[] { Vector2.Zero });

    [Export]
    private int raycastSamples = 3;

    private float maxLineRepulsion = 3f;

    [Export(PropertyHint.Range, "0,1")]
    private float visionRange = .25f;

    private RayCast2D rayCast;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (motion != Vector2.Zero)
        {
            realAngle = motion.Angle();
        }
        else
        {
            realAngle = (GD.Randi() % 12) * Mathf.Pi / 6;
            motion = Vector2.Right.Rotated(realAngle) * speed;
        }
        currentPoints.Add(Vector2.Right.Rotated(realAngle));
        Points = currentPoints.ToArray();
        _on_DirectionChangeTimer_timeout();
        if (slimeMoldBranchScene != null)
        {
            GetNode<Timer>("BranchTimer").Start((float)GD.RandRange(branchTimeMin, branchTimeMax));
        }
        maxAngleSpeed *= Mathf.Pi / 180;

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
        angleChangeSpeed += currentAngleAcceleration * delta;
        angleChangeSpeed = Mathf.Clamp(angleChangeSpeed, -maxAngleSpeed, maxAngleSpeed);
        realAngle += angleChangeSpeed * delta;
        Vector2 candidateMotion = Vector2.Right.Rotated(realAngle);
        realAngle += getRepulsion() * delta;
        if (Mathf.Abs(candidateMotion.AngleTo(motion)) > angleSnap * Mathf.Pi / 180)
        {
            motion = candidateMotion * speed;
            currentPoints.Add(currentPoints[currentPoints.Count - 1]);
        }
        currentPoints[currentPoints.Count - 1] += motion * delta;
        Points = currentPoints.ToArray();
    }

    private float getRepulsion()
    {
        rayCast.Position = currentPoints[currentPoints.Count - 1];
        float angleRange = visionRange * Mathf.Pi;
        float angle = raycastSamples == 1 ? 0f : (-angleRange / 2);
        float angleStep = angleRange / (raycastSamples - 1);
        float motionAngle = motion.Angle();
        float totalRepulsion = 0f;

        for (int sample = 0; sample < raycastSamples; sample++, angle += angleStep)
        {
            rayCast.Rotation = motionAngle + angle;
            rayCast.ForceRaycastUpdate();
            if (!rayCast.IsColliding())
            {
                continue;
            }
            float directedRepulsion = Mathf.Abs(angle) < Mathf.Epsilon
                ? -.5f
                : -angle * 2 / angleRange;
            float closeness = 1f - rayCast.GetCollisionPoint().DistanceTo(rayCast.GlobalPosition) / rayCast.CastTo.Length();
            closeness *= closeness;
            totalRepulsion += directedRepulsion * maxLineRepulsion * closeness;
        }
        return totalRepulsion;
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
        firstMold.motion = secondMold.motion = motion;
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
        var speedExtremity = angleChangeSpeed / maxAngleSpeed;
        var bounceBack = Mathf.Abs(speedExtremity) * GD.Randf();
        currentAngleAcceleration = (GD.Randf() * 2 - 1f) * maxAngleAcceleration * Mathf.Pi / 180;
        currentAngleAcceleration = Mathf.Lerp(currentAngleAcceleration, -speedExtremity * maxAngleAcceleration, bounceBack);
    }

    private void _on_beat()
    {
        speedUp = initialSpeedUp * beatSpeedMultiplier;
        Width = initialWidth * beatWidthMultiplier;
    }
}
