using Godot;

public class BusFader : AudioFader
{
    private const float MIN_VOLUME = -40f;
    private const float MAX_VOLUME = 0f;
    private const float VOLUME_CHANGE_PER_SECOND = 2f * (MAX_VOLUME - MIN_VOLUME);

    private AudioStreamPlayer clearLevel;
    private AudioStreamPlayer lose;

    private readonly int busIndex;

    private float targetVolume;
    public bool Enabled { set => targetVolume = value ? MAX_VOLUME : MIN_VOLUME; }

    private float Volume
    {
        get => AudioServer.GetBusVolumeDb(busIndex);
        set
        {
            AudioServer.SetBusVolumeDb(busIndex, value);
            AudioServer.SetBusMute(busIndex, value <= MIN_VOLUME + Mathf.Epsilon);
        }
    }

    public BusFader(string busName)
    {
        busIndex = AudioServer.GetBusIndex(busName);
    }

    public void Process(float delta)
    {
        Volume = Mathf.MoveToward(Volume, targetVolume, VOLUME_CHANGE_PER_SECOND * delta);
    }
}
