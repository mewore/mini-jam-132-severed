using System;
using Godot;

public class MainMenu : VBoxContainer
{
    private float now = 0f;

    public override void _Ready()
    {
        Global.LoadGameData();
        Global.SaveGameData();
    }

    public override void _Process(float delta)
    {
        now += delta;
    }
}
