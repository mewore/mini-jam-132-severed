using Godot;
using System;
using System.Collections.Generic;

public class CalculationOverlay : Node2D
{
    private const int VIEWPORT_WIDTH = 1024;
    private const int VIEWPORT_HEIGHT = 600;

    [Export]
    private int squareSize = 64;
    public int SquareSize => squareSize;

    [Export]
    private Color newRectangleColor = new Color();

    [Export]
    private Gradient rectangleColor = null;

    private int squareRow = 0;
    private int squareColumn = 0;

    private int rowSize;
    private int columnSize;

    private readonly List<(Rect2, float)> newRectanglesToDraw = new List<(Rect2, float)>();
    private readonly List<(Rect2, float)> rectanglesToDraw = new List<(Rect2, float)>();

    public int SquareCount => Mathf.CeilToInt((float)VIEWPORT_HEIGHT / squareSize) * Mathf.CeilToInt((float)VIEWPORT_WIDTH / squareSize);

    public Rect2 GetNextSquare()
    {
        var result = new Rect2(squareColumn * squareSize, squareRow * squareSize, squareSize, squareSize);
        squareColumn++;
        if (squareColumn >= columnSize)
        {
            squareColumn = 0;
            squareRow++;
        }
        return result;
    }

    public void SquareDone(Rect2 rectangle, float success)
    {
        newRectanglesToDraw.Add((rectangle, success));
        Update();
    }

    public override void _Ready()
    {
        rowSize = Mathf.CeilToInt((float)VIEWPORT_HEIGHT / squareSize);
        columnSize = Mathf.CeilToInt((float)VIEWPORT_WIDTH / squareSize);
        GetViewport().RenderTargetClearMode = Viewport.ClearMode.Always;
    }

    public override void _Draw()
    {
        foreach (var rectangle in rectanglesToDraw)
        {
            DrawRect(rectangle.Item1, rectangleColor.Interpolate(rectangle.Item2));
        }
        foreach (var rectangle in newRectanglesToDraw)
        {
            DrawRect(rectangle.Item1, newRectangleColor);
            rectanglesToDraw.Add(rectangle);
            Update();
        }
    }
}
