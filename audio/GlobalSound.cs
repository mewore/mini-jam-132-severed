using Godot;

public class GlobalSound : Node
{
    private AudioStreamPlayer clearLevel;
    private AudioStreamPlayer lose;

    private readonly AudioFader musicForegroundFader = new BusFader("MusicForeground");
    public bool MusicForeground { set => musicForegroundFader.Enabled = value; }

    private AudioFader mainMenuMusicFader;
    public bool MainMenuMusic { set => mainMenuMusicFader.Enabled = value; }

    private AudioFader ambientNoiseLabFader;
    public bool AmbienetNoiseLab { set => ambientNoiseLabFader.Enabled = value; }

    public override void _Ready()
    {
        clearLevel = GetNode<AudioStreamPlayer>("ClearLevel");
        lose = GetNode<AudioStreamPlayer>("Lose");
        mainMenuMusicFader = new AudioPlayerFader(GetNode<AudioStreamPlayer>("MainMenuMusic"));
        ambientNoiseLabFader = new AudioPlayerFader(GetNode<AudioStreamPlayer>("AmbientNoiseLab"));
    }

    public void PlayClearLevel()
    {
        clearLevel.Play();
    }

    public void PlayEnterLevel()
    {
        GetNode<AudioStreamPlayer>("EnterLevel").Play();
    }

    public void PlayLose()
    {
        lose.Play();
    }

    public static GlobalSound GetInstance(Node node)
    {
        return node.GetNode<GlobalSound>("/root/" + nameof(GlobalSound));
    }
}
