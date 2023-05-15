using Godot;
using System;

[Tool]
public class VolumeSlider : HSlider
{
    [Export]
    private string busName = GameSettings.MASTER_BUS_NAME;

    private int busIndex;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        busIndex = AudioServer.GetBusIndex(busName);
        Global.LoadSettings();
        Value = Global.Settings.Volume[busName];
        AudioServer.SetBusVolumeDb(busIndex, GameSettings.PercentageToDb(Value));
        Connect("value_changed", this, nameof(_on_value_changed));
    }

    private void _on_value_changed(float value)
    {
        Global.Settings.Volume[busName] = Mathf.RoundToInt(value);
        AudioServer.SetBusVolumeDb(busIndex, GameSettings.PercentageToDb(value));
        Global.SaveSettings();
    }
}
