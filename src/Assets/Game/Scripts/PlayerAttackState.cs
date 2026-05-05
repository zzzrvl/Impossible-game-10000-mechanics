using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerAttackState : PlayerState
{
    private int comboIndex; // Какой по счету удар мы делаем (0, 1, 2...)
    private bool nextAttackQueued; // Запомнили ли мы нажатие для следующего удара
    
    // Названия триггеров или анимаций в твоем Animator
    private readonly string[] attackAnimations = { "Attack1", "Attack2", "Attack3" };

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine, int comboIndex) : base(player, stateMachine)
    {
        this.comboIndex = comboIndex;
    }

    public override void Enter()
    {
        nextAttackQueued = false;

        // Запускаем нужную анимацию (Attack1, Attack2 или Attack3)
        player.Animator.CrossFade(attackAnimations[comboIndex], 0.1f);

        // ВАЖНО: Мы вошли в атаку, значит текущее нажатие кнопки мы "съедаем", чтобы оно не висело
        player.InputHandler.ConsumeAttack();
    }

    public override void Update()
    {
        // ФИШКА SEKIRO 1: Отмена начала атаки в парирование (Feint / Cancel)
        // Если анимация только началась (например, первые 20% времени), игрок может резко уйти в блок
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime < 0.2f)
        {
            if (player.InputHandler.ParryPressed)
            {
                // stateMachine.ChangeState(new PlayerParryState(player, stateMachine));
                // player.InputHandler.ConsumeParry();
                // return;
            }
        }

        // ФИШКА SEKIRO 2: Буферизация следующего удара (Queuing)
        // Если игрок нажал ЛКМ во время анимации, мы запоминаем это!
        if (player.InputHandler.AttackPressed && !nextAttackQueued)
        {
            nextAttackQueued = true;
            player.InputHandler.ConsumeAttack(); // "Съели" ввод, он сохранен в переменную nextAttackQueued
        }

        // Проверяем, закончилась ли анимация атаки (normalizedTime >= 0.95f означает 95% проигрывания)
        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= 0.95f)
        {
            // Если игрок успел нажать кнопку (забуферил) И мы еще не дошли до конца комбо
            if (nextAttackQueued && comboIndex < attackAnimations.Length - 1)
            {
                // Переходим к следующему удару!
                stateMachine.ChangeState(new PlayerAttackState(player, stateMachine, comboIndex + 1));
            }
            else
            {
                // Если игрок ничего не нажал или комбо кончилось — возвращаемся в Idle
                stateMachine.ChangeState(new PlayerIdleState(player, stateMachine));
            }
        }
    }

    public override void Exit()
    {
        // Очищаем всё при выходе
        nextAttackQueued = false;
    }
}