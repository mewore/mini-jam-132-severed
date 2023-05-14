using Godot;
using System;

public class Background : Node2D
{
    [Export]
    private PackedScene mainMenuScene = null;


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            GetTree().ChangeSceneTo(mainMenuScene);
        }
    }
}
