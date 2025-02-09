// Assets/Scripts/FSM/SnakeIdleState.cs
using UnityEngine;

public class SnakeIdleState : State
{
    private SnakeAI snakeAI;

    public SnakeIdleState(SnakeAI ai)
    {
        snakeAI = ai;
    }

    public override void OnEnter()
    {
        Debug.Log("Snake: Idle State'e girildi.");
    }

    public override void OnUpdate()
    {
        // Idle'da her frame yem arayıp path bulmaya çalışabilir
        snakeAI.FindFoodAndMove();
    }

    public override void OnExit()
    {
        Debug.Log("Snake: Idle State'ten çıkıldı.");
    }
}
