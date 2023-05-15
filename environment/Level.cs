using Godot;
using System;

[Tool]
public class Level : Node2D
{
    [Export]
    private PackedScene mainMenuScene = null;

    [Export]
    private int levelNumber;

    private CanvasLayer pauseLayer;

    private Overlay overlay;


    private bool paused = false;
    private bool Paused
    {
        get => paused;
        set
        {
            GD.Print("PAUSED: ", paused, " -> ", value && !overlay.Transitioning);
            paused = value && !overlay.Transitioning;
            GetTree().Paused = paused || !overlay.CompletelyTransparent;
            pauseLayer.Visible = paused;
            overlay.PauseMode = paused ? PauseModeEnum.Stop : PauseModeEnum.Process;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        overlay = GetNode<Overlay>("Overlay");
        if (Engine.EditorHint)
        {
            levelNumber = Global.GetLevelPathNumber(GetTree().EditedSceneRoot.Filename.ToString());
            if (Global.CurrentLevel <= 0 && levelNumber > 0)
            {
                Global.CurrentLevel = levelNumber;
            }
            else
            {
                GetNode<Button>("PauseLayer/Control/PauseMenu/RestartButton").Disabled = true;
            }
        }
        pauseLayer = GetNode<CanvasLayer>("PauseLayer");
    }

    public void _on_ContinueButton_pressed() => Paused = false;

    public void _on_RestartButton_pressed()
    {
        GlobalSound.GetInstance(this).PlayEnterLevel();
        Paused = false;
        overlay.RequestTransition("");
    }

    public void _on_MainMenuButton_pressed()
    {
        GlobalSound.GetInstance(this).PlayEnterLevel();
        Paused = false;
        overlay.RequestTransition(mainMenuScene);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("force_restart"))
        {
            _on_RestartButton_pressed();
        }
        else if (@event.IsActionPressed("pause"))
        {
            Paused = !Paused;
        }
    }

    public void _on_Game_GameOver()
    {
        GlobalSound.GetInstance(this).PlayEnterLevel();
        overlay.RequestTransition(mainMenuScene);
    }

    public void _on_Game_RestartRequested()
    {
        GlobalSound.GetInstance(this).PlayEnterLevel();
        overlay.RequestTransition("");
    }
}
