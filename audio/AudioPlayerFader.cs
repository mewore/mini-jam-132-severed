using Godot;

public class AudioPlayerFader : AudioFader
{
    private const float MIN_VOLUME = -40f;
    private const float MAX_VOLUME = 0f;
    private const float VOLUME_CHANGE_PER_SECOND = 2f * (MAX_VOLUME - MIN_VOLUME);

    private readonly AudioStreamPlayer player;

    private float targetVolume;
    public bool Enabled { set => targetVolume = value ? MAX_VOLUME : MIN_VOLUME; }

    private float Volume
    {
        get => player.VolumeDb;
        set
        {
            player.VolumeDb = value;
            player.StreamPaused = value <= MIN_VOLUME + Mathf.Epsilon;
        }
    }

    public AudioPlayerFader(AudioStreamPlayer player)
    {
        this.player = player;
    }

    public void Process(float delta)
    {
        Volume = Mathf.MoveToward(Volume, targetVolume, VOLUME_CHANGE_PER_SECOND * delta);
    }
}
