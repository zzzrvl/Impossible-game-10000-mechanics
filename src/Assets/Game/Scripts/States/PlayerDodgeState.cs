using UnityEngine;

public class PlayerDodgeState : PlayerState
{
    private Vector3 dodgeDirection;

    public PlayerDodgeState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.ConsumeDodge();
        Debug.Log("--- Состояние: DODGE ---");

        if (player.InputHandler.MoveInput.sqrMagnitude > 0.01f)
            dodgeDirection = new Vector3(player.InputHandler.MoveInput.x, 0, player.InputHandler.MoveInput.y).normalized;
        else
            dodgeDirection = -player.transform.forward; 
            
        if (dodgeDirection != -player.transform.forward)
            player.transform.rotation = Quaternion.LookRotation(dodgeDirection);
            
        // Пример для будущего: Делаем игрока неуязвимым!
        // player.Stats.IsInvincible = true;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.CC.Move(dodgeDirection * player.Data.dodgeSpeed * Time.deltaTime);

        if (stateTimer >= player.Data.dodgeDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    // НОВАЯ ЛОГИКА ВЫХОДА
    public override void Exit()
    {
        base.Exit();
        
        // Пример для будущего: Отключаем неуязвимость, когда перекат закончился!
        // player.Stats.IsInvincible = false;
        
        // Сбрасываем вектор движения
        dodgeDirection = Vector3.zero;
    }
}