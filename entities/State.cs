using Godot;

public partial class State : Node
{
    protected string targetState;
    public string TargetState { get => targetState; }

    public bool IsActive = false;

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }

    public virtual void PhysicsProcess(float delta)
    {
    }

    public virtual void Process(float delta)
    {
    }

    public virtual void UnhandledInput(InputEvent @event)
    {
    }

    public void ClearTargetState()
    {
        targetState = null;
    }
}
