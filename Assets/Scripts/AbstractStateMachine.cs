using System;

public abstract class AbstractStateMachine<T>
    where T : Enum
{
    public AbstractStateMachine() => currentState = default;
    public AbstractStateMachine(T value) : this() => currentState = value;
    
    public T currentState { get; protected set; }
    
    public event Action<T> onStateChange;

    public bool MoveTo(T next_state)
    {
        var res = CheckTransition(next_state);
        
        if (res)
        {
            currentState = next_state;
            if (onStateChange != null)
            {
                onStateChange.Invoke(currentState);
            }
        }

        return res;
    }

    public abstract bool CheckTransition(T next_state);
}
