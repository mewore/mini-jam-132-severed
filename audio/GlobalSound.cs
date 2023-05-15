using Godot;

public class GlobalSound : Node
{
    private AudioStreamPlayer clearLevel;
    private AudioStreamPlayer lose;

    private readonly AudioFader musicForegroundFader = new BusFader("MusicForeground");
    public bool MusicForeground { set => musicForegroundFader.Enabled = value; }

    public override void _Ready()
    {
        clearLevel = GetNode<AudioStreamPlayer>("ClearLevel");
        lose = GetNode<AudioStreamPlayer>("Lose");
    }

    public void PlayClearLevel()
    {
        clearLevel.Play();
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
