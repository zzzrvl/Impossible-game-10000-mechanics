using UnityEngine;

public class PlayerEntity : Entity
{
    public void Attack()
    {
        // При каждой атаке добавляем концентрацию, как в диздоке
        AddFocus(10f);
        Debug.Log("Игрок атакует, концентрация растет");
    }

    protected override void Die()
    {
        base.Die();
        // Логика рестарта уровня или экрана смерти
        gameObject.SetActive(false);
    }
}