using UnityEngine;
using System;

public abstract class Entity : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    public float maxFocus = 100f;
    public float currentFocus;
    public float focusDecayRate = 5f; // Скорость убывания концентрации

    public IInteractable equippedItem;

    public event Action OnDeath;
    public event Action<float> OnFocusChanged;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        currentFocus = 0f; // В начале боя концентрация на нуле
    }

    protected virtual void Update()
    {
        HandleFocusDecay();
    }

    private void HandleFocusDecay()
    {
        if (currentFocus > 0)
        {
            currentFocus -= focusDecayRate * Time.deltaTime;
            currentFocus = Mathf.Clamp(currentFocus, 0, maxFocus);
            OnFocusChanged?.Invoke(currentFocus / maxFocus);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void  AddFocus(float amount)
    {
        currentFocus += amount;
        currentFocus = Mathf.Clamp(currentFocus, 0, maxFocus);
        OnFocusChanged?.Invoke(currentFocus / maxFocus);
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} погиб.");
    }
}