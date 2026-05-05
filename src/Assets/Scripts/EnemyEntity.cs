using UnityEngine;

public class EnemyEntity : Entity
{
    public bool isStunned = false;
    private float stunTimer = 0f;

    protected override void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
            }

            return; // Если оглушен, не выполняем логику (движение/атаку)
        }

        base.Update();
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        Debug.Log($"{gameObject.name} оглушен предметом!");
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        // Можно добавить эффект "взрыва головы" при смерти, если это последний удар
    }
}