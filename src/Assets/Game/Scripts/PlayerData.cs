using UnityEngine;

public class PlayerData : ScriptableObject
{
    [Header("Health")]
    public float maxHealth;

    [Header("Concentration")]
    public float maxConcentration;
    public float concentrationRegenRate;
    
    [Tooltip("Множитель замедления регенерации концентрации при низком HP")]
    public AnimationCurve postureRegenCurve; 

    [Header("Offensive Stats")]
    public float attackDamage;
    public float concentrationDamage;
    public float chipDamagePercentage; // процент урона сквозь блок

    [Header("Defensive Stats")]
    public float physicalResistance;
    public int deathblowNodes; // Количество "точек" для смертельного удара
    
    [Header("Movement")]
    public float moveSpeed;
}