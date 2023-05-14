using Godot;
using System;

[Tool]
public class Level : Node2D
{
    [Export]
    private PackedScene mainMenuScene = null;

    [Export]
    private int levelNumber;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Engine.EditorHint)
        {
            levelNumber = Global.GetLevelPathNumber(GetTree().EditedSceneRoot.Filename.ToString());
            if (Global.CurrentLevel <= 0 && levelNumber > 0)
            {
                Global.CurrentLevel = levelNumber;
            }
        }
    }

    public void _on_RestartButton_pressed()
    {
        GetTree().ReloadCurrentScene();
    }

    public void _on_MainMenuButton_pressed()
    {
        GetTree().ChangeSceneTo(mainMenuScene);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("force_restart"))
        {
            _on_RestartButton_pressed();
        }
    }

    public void _on_Game_GameOver()
    {
        GetTree().ChangeSceneTo(mainMenuScene);
    }
}
