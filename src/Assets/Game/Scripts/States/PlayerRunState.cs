using UnityEngine;

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("--- Состояние: RUN ---");
        // player.Animator.CrossFade("Run", 0.1f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.InputHandler.DodgePressed) { stateMachine.ChangeState(player.DodgeState); return; }
        if (player.InputHandler.ParryPressed) { stateMachine.ChangeState(player.ParryState); return; }
        if (player.InputHandler.AttackPressed) { stateMachine.ChangeState(player.AttackState); return; }

        if (player.InputHandler.MoveInput.sqrMagnitude <= 0.01f) { stateMachine.ChangeState(player.IdleState); return; }
        if (!player.InputHandler.SprintHeld) { stateMachine.ChangeState(player.WalkState); return; }

        Vector3 moveDir = new Vector3(player.InputHandler.MoveInput.x, 0, player.InputHandler.MoveInput.y);
        player.CC.Move(moveDir * player.Data.runSpeed * Time.deltaTime);
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(moveDir), player.Data.rotationSpeed * Time.deltaTime);
    }
}