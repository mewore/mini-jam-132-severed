using Godot;
using System;

[Tool]
public class Overlay : CanvasLayer
{
    [Signal]
    public delegate void DoneAppearing();

    [Signal]
    public delegate void DoneDisappearing();

    [Export]
    private float speed = 1f;

    [Export]
    private bool show = true;

    [Export]
    private bool disintegrateOnLoad = true;

    [Export]
    private bool pauseTreeIfNotTransparent = true;

    private float _visibility = 1f;

    private bool isAtTarget = true;

    [Export]
    private float visibility
    {
        get => _visibility; set
        {
            if (value != _visibility)
            {
                isAtTarget = false;
                _visibility = value;
                if (shaderMaterial != null)
                {
                    shaderMaterial.SetShaderParam("dissolve_state", (1f - visibility) * 1.01f);
                }
            }
            else if (isAtTarget == false)
            {
                isAtTarget = true;
                EmitSignal(CompletelyOpaque ? nameof(DoneAppearing) : nameof(DoneDisappearing));
                if (requestedScene != null)
                {
                    GetTree().Paused = false;
                    GetTree().ChangeSceneTo(requestedScene);
                }
                else if (requestedScenePath != null)
                {
                    GetTree().Paused = false;
                    if (requestedScenePath == "")
                    {
                        GetTree().ReloadCurrentScene();
                    }
                    else
                    {
                        GetTree().ChangeScene(requestedScenePath);
                    }
                }
            }
            if (!Engine.EditorHint && pauseTreeIfNotTransparent)
            {
                GetTree().Paused = !CompletelyTransparent;
            }
        }
    }
    public bool CompletelyOpaque => visibility >= 1f - Mathf.Epsilon;
    public bool CompletelyTransparent => visibility <= Mathf.Epsilon;
    public bool Transitioning => requestedScene != null || requestedScenePath != null;

    private ShaderMaterial shaderMaterial;

    private string requestedScenePath;
    private PackedScene requestedScene;

    public override void _Ready()
    {
        shaderMaterial = GetNode<Sprite>("OverlayColor").Material as ShaderMaterial;
        shaderMaterial.SetShaderParam("dissolve_state", (1f - visibility) * 1.01f);
        if (!Engine.EditorHint && disintegrateOnLoad)
        {
            show = false;
            Visible = true;
        }
    }

    public override void _Process(float delta)
    {
        visibility = Mathf.MoveToward(visibility, show ? 1f : 0f, speed * delta);
    }

    public void RequestTransition(string scenePath)
    {
        requestedScenePath = scenePath;
        show = true;
    }

    public void RequestTransition(PackedScene scene)
    {
        requestedScene = scene;
        show = true;
    }
}
