using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // Ссылка на сгенерированный класс настроек ввода (должен называться PlayerInputActions)
    private PlayerInputActions inputActions;

    // --- ПЕРЕМЕННЫЕ, КОТОРЫЕ БУДУТ ЧИТАТЬ ДРУГИЕ СКРИПТЫ ---
    
    [Header("Movement")]
    public Vector2 MoveInput { get; private set; }
    
    [Header("Combat")]
    public bool AttackPressed { get; private set; }
    public bool BlockHeld { get; private set; }
    public bool ParryPressed { get; private set; }
    
    [Header("Actions")]
    public bool DodgePressed { get; private set; }

    // --- ИНИЦИАЛИЗАЦИЯ ---
    
    private void Awake()
    {
        // Создаем экземпляр сгенерированного класса
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Обязательно включаем считывание кнопок
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // Обязательно выключаем, когда объект неактивен, чтобы не было ошибок
        inputActions.Disable();
    }

    // --- СЧИТЫВАНИЕ КНОПОК КАЖДЫЙ КАДР ---

    private void Update()
    {
        // 1. Движение: читаем постоянно (возвращает X и Y от -1 до 1)
        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // 2. Удержание блока: читаем постоянно (true, пока кнопка зажата)
        BlockHeld = inputActions.Player.Parry.IsPressed();

        // 3. Разовые нажатия (с буферизацией)
        // Если кнопка уже нажата (true), мы ждем, пока другой скрипт её не "съест" через Consume...()
        
        if (!AttackPressed)
        {
            AttackPressed = inputActions.Player.LightAttack.WasPressedThisFrame();
        }

        if (!ParryPressed)
        {
            ParryPressed = inputActions.Player.Parry.WasPressedThisFrame();
        }

        if (!DodgePressed)
        {
            DodgePressed = inputActions.Player.Dodge.WasPressedThisFrame();
        }
    }

    // --- МЕТОДЫ ПОГЛОЩЕНИЯ ВВОДА (ДЛЯ ДРУГИХ СКРИПТОВ) ---
    
    public void ConsumeAttack() => AttackPressed = false;
    public void ConsumeParry() => ParryPressed = false;
    public void ConsumeDodge() => DodgePressed = false;
}
