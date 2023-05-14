using Godot;

public interface Obstacle
{
    Gradient ColourGradient { set; }

    float Success { set; }

    void Place(float success);
}
