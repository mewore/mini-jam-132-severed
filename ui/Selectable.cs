using Godot;

public interface Selectable
{
    Vector2 GlobalPosition { get; }

    string ScenePath { get; }

    Color Color { get; }

    bool Hovered { set; }

    bool Selectable { get; }

    int TargetLevel { get; }
}
