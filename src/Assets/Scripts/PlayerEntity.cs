using UnityEngine;

public class PlayerEntity : Entity
{
    [Header("Инвентарь")]
    [Tooltip("Если не задано, предмет висит как дочерний у самого игрока.")]
    [SerializeField] private Transform itemHoldPoint;

    /// <summary>Точка, куда подвешивается подобранный предмет.</summary>
    public Transform ItemHoldPoint => itemHoldPoint != null ? itemHoldPoint : transform;

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