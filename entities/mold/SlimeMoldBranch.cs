using Godot;
using System;
using System.Collections.Generic;

public class SlimeMoldBranch : Line2D
{
    [Export]
    private string slimeMoldBranchScene = "res://entities/mold/SlimeMoldBranch.tscn";

    [Export]
    private float branchTimeMin = 3f;

    [Export]
    private float branchTimeMax = 10f;

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

    [Export]
    private int widthDecrement = 2;

    private bool active = true;

    private Vector2 motion;
    private float realAngle;

    private List<Vector2> currentPoints = new List<Vector2>(new Vector2[] { Vector2.Zero });

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
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!active)
        {
            return;
        }
        delta *= speedUp;
        angleChangeSpeed += currentAngleAcceleration * delta;
        angleChangeSpeed = Mathf.Clamp(angleChangeSpeed, -maxAngleSpeed, maxAngleSpeed);
        realAngle += angleChangeSpeed * delta;
        Vector2 candidateMotion = Vector2.Right.Rotated(realAngle);
        if (Mathf.Abs(candidateMotion.AngleTo(motion)) > angleSnap * Mathf.Pi / 180)
        {
            motion = candidateMotion * speed;
            currentPoints.Add(currentPoints[currentPoints.Count - 1]);
        }
        currentPoints[currentPoints.Count - 1] += motion * delta;
        Points = currentPoints.ToArray();
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
        firstMold.Width = secondMold.Width = Width - widthDecrement;
        if (GD.Randf() > 0.5f)
        {
            firstMold.Width = Width;
            secondMold.Width = Width / 2;
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
}