using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("--- Состояние: IDLE ---");
        // ЗАДЕЛ ПОД АНИМАЦИЮ:
        // player.Animator.CrossFade("Idle", 0.1f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. Проверки боевых действий (Приоритет высший)
        if (player.InputHandler.DodgePressed) { stateMachine.ChangeState(player.DodgeState); return; }
        if (player.InputHandler.ParryPressed) { stateMachine.ChangeState(player.ParryState); return; }
        if (player.InputHandler.AttackPressed) { stateMachine.ChangeState(player.AttackState); return; }

        // 2. Проверка движения
        if (player.InputHandler.MoveInput.sqrMagnitude > 0.01f)
        {
            if (player.InputHandler.SprintHeld)
                stateMachine.ChangeState(player.RunState);
            else
                stateMachine.ChangeState(player.WalkState);
        }
    }
}