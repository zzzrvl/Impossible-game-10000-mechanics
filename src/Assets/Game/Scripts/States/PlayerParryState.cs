using UnityEngine;

public class PlayerParryState : PlayerState
{
    public PlayerParryState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.ConsumeParry();
        Debug.Log("--- Состояние: ПАРИРОВАНИЕ (Окно открыто) ---");
        // player.Animator.CrossFade("Parry", 0.05f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Если время идеального парирования вышло
        if (stateTimer >= player.Data.parryWindowDuration)
        {
            // Проверяем: держит ли игрок кнопку блока?
            if (player.InputHandler.BlockHeld)
            {
                // Если держит, значит он просто переходит в глухой блок (или остается в нем)
                // Так как у нас пока нет отдельного BlockState, мы просто делаем Debug, 
                // но в будущем здесь будет ChangeState(player.BlockState);
                Debug.Log("...Окно закрыто. Держим глухой БЛОК...");
            }
            else
            {
                // Если кнопку отпустили - возвращаемся в стойку
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    // Этот публичный метод будет читать скрипт Брони (HitReceiver)
    public bool IsParryWindowActive()
    {
        return stateTimer <= player.Data.parryWindowDuration;
    }
}