using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        // Включаем анимацию простоя (предположим, у вас есть такой параметр в Animator)
        // player.Animator.Play("Idle"); 
    }

    public override void Update()
    {
        // Если нажали атаку — переходим в состояние атаки и начинаем комбо с индекса 0
        if (player.InputHandler.AttackPressed)
        {
            stateMachine.ChangeState(new PlayerAttackState(player, stateMachine, 0));
        }
        
        // (Здесь же потом добавишь проверки на бег, прыжок, уворот и т.д.)
        else if (player.InputHandler.DodgePressed)
        {
            // stateMachine.ChangeState(new PlayerDodgeState(...));
        }
    }
}