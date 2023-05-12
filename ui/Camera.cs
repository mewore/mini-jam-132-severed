using Godot;
using System;

public class Camera : Camera2D
{
    [Export]
    private float shakeAmplitude = 3f;

    [Export]
    private float shakePeriodX = .05f;

    [Export]
    private float shakePeriodY = .1f;

    [Export]
    private float speedup = 1f;

    [Export]
    private float shakeDuration = .5f;

    private float shakenAt = -Mathf.Inf;

    private float now = 0f;

    [Export]
    private NodePath copyCamera;

    private Camera2D copyCameraNode;

    public override void _Ready()
    {
        copyCameraNode = copyCamera == null ? this : GetNode<Camera2D>(copyCamera);
    }

    public override void _Process(float delta)
    {
        now += delta;
        if (shakenAt + shakeDuration <= now)
        {
            copyCameraNode.Offset = Offset = Vector2.Zero;
            return;
        }
        float amount = 1f - (now - shakenAt) / shakeDuration;
        amount *= amount;
        float mew = now * Mathf.Pi * speedup;
        copyCameraNode.Offset = Offset = new Vector2(Mathf.Cos(mew / shakePeriodX), Mathf.Sin(mew / shakePeriodX)) * shakeAmplitude;
    }

    public void Shake()
    {
        shakenAt = now;
    }
}
