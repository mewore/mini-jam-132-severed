using Godot;
using System;

public class ObstacleLine : Line2D, Obstacle
{
    private float initialWidth;

    private Gradient colourGradient;
    public Gradient ColourGradient { set => colourGradient = value; }

    public float Success
    {
        set
        {
            DefaultColor = colourGradient.Interpolate(value);
            Width = initialWidth * value;
        }
    }

    private bool placed = false;

    public override void _Ready()
    {
        initialWidth = Width;
        Points = new Vector2[] { GetGlobalMousePosition() };
    }

    public override void _Process(float delta)
    {
        if (placed)
        {
            return;
        }
        Points = new Vector2[] { Points[0], GetGlobalMousePosition() };
    }

    public void Place(float success)
    {
        if (success < Mathf.Epsilon)
        {
            QueueFree();
            return;
        }
        Points = new Vector2[] { Points[0], GetGlobalMousePosition() };
        Success = success;

        var collisionShape = GetNode<CollisionShape2D>("Body/CollisionShape2D");
        collisionShape.Disabled = false;
        var rectangleShape = new RectangleShape2D();
        rectangleShape.Extents = new Vector2(Points[0].DistanceTo(Points[1]), Width) * .5f;
        collisionShape.Rotation = Points[0].AngleToPoint(Points[1]);
        collisionShape.Position = (Points[0] + Points[1]) * .5f;
        collisionShape.Shape = rectangleShape;
        placed = true;
    }
}
