using Godot;
using System;

public class DamageLine : Line2D
{
    public void _on_Fadeout_timeout()
    {
        QueueFree();
    }

    public void _on_Blink_timeout()
    {
        Visible = !Visible;
    }
}
