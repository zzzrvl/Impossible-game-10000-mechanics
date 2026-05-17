using UnityEngine;

public class PlayerAttackState : PlayerState
{
    private bool damageApplied;

    public PlayerAttackState(PlayerController player, PlayerStateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.ConsumeAttack();
        damageApplied = false;

        if (Time.time - player.LastAttackTime > player.Data.comboResetTime)
            player.ComboStep = 0;
        
        player.ComboStep++;
        if (player.ComboStep > 4) player.ComboStep = 1;
        player.LastAttackTime = Time.time;

        Debug.Log($"--- Состояние: УДАР (Шаг {player.ComboStep}) ---");
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (stateTimer >= 0.15f && !damageApplied)
        {
            damageApplied = true;
            // Данные об уроне берутся из PlayerData!
            Debug.Log($"*ВЖУХ* Нанесен урон: {player.Data.attackDamage}"); 
        }

        if (stateTimer >= player.Data.attackRecoveryTime)
        {
            if (player.InputHandler.AttackPressed)
                stateMachine.ChangeState(player.AttackState);
            else
                stateMachine.ChangeState(player.IdleState);
        }
    }

    // НОВАЯ ЛОГИКА ВЫХОДА
    public override void Exit()
    {
        base.Exit();
        // Если состояние прервали (например, перекатом или получением урона) ДО того, 
        // как урон был нанесен, мы гарантируем, что атака отменена.
        damageApplied = false; 
        
        // В будущем здесь будет:
        // player.Weapon.StopSwing(); // Обязательно выключаем коллайдер меча!
        
        Debug.Log("Вышли из состояния атаки.");
    }
}