// Assets/Scripts/FSM/FiniteStateMachine.cs
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour
{
    private State currentState;

    public void ChangeState(State newState)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState = newState;
        currentState.OnEnter();
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
    }
}
