using UnityEngine;

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("--- Состояние: WALK ---");
        // player.Animator.CrossFade("Walk", 0.1f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Боевые переходы
        if (player.InputHandler.DodgePressed) { stateMachine.ChangeState(player.DodgeState); return; }
        if (player.InputHandler.ParryPressed) { stateMachine.ChangeState(player.ParryState); return; }
        if (player.InputHandler.AttackPressed) { stateMachine.ChangeState(player.AttackState); return; }

        // Переход в Бег или Покой
        if (player.InputHandler.MoveInput.sqrMagnitude <= 0.01f) { stateMachine.ChangeState(player.IdleState); return; }
        if (player.InputHandler.SprintHeld) { stateMachine.ChangeState(player.RunState); return; }

        // Сама ходьба
        MoveCharacter(player.Data.walkSpeed);
    }

    private void MoveCharacter(float speed)
    {
        Vector3 moveDir = new Vector3(player.InputHandler.MoveInput.x, 0, player.InputHandler.MoveInput.y);
        player.CC.Move(moveDir * speed * Time.deltaTime);
        
        // Плавный поворот персонажа
        player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(moveDir), player.Data.rotationSpeed * Time.deltaTime);
    }
}


