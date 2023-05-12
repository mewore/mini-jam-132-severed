using Godot;

public class Level : Node2D
{
    [Export]
    private PackedScene mainMenuScene = null;

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
        if (@event.IsActionPressed("restart"))
        {
            _on_RestartButton_pressed();
        }
    }

    public void _on_Game_GameWon()
    {
        GetTree().ChangeSceneTo(mainMenuScene);
    }
}
