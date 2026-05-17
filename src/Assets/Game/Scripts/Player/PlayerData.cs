using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Combat/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Health & Posture")]
    public float maxHealth = 100f;
    public float maxConcentration = 100f;
    public float concentrationRegenRate = 15f;
    
    [Tooltip("Множитель замедления регенерации концентрации при низком HP")]
    public AnimationCurve postureRegenCurve;
    
    [Header("Offensive Stats")]
    public float attackDamage = 20f;         // Урон по здоровью
    public float concentrationDamage = 15f;  // Урон по концентрации врага
    public float chipDamagePercentage = 0.1f;// Процент урона, проходящий сквозь обычный блок

    [Header("Defensive Stats")]
    public float physicalResistance = 0f;
    public int deathblowNodes = 2;           // Количество жизней / точек для смертельного удара
    
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.0f;
    public float rotationSpeed = 10f;        // Скорость поворота персонажа к направлению движения

    [Header("Dodge")]
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 0.3f;
    public float dodgeCost = 20f;            // Затраты концентрации на кувырок

    [Header("Combat Timings")]
    public float parryWindowDuration = 0.2f; // Окно идеального парирования (в секундах)
    public float attackRecoveryTime = 0.4f;  // Время возврата меча, после которого можно идти
    public float comboResetTime = 1.2f;      // Время, через которое комбо сбрасывается к 1 удару
}