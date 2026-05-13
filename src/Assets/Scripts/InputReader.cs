using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    /// <summary>Для контекста без GetComponent у предметов (подбор, UI).</summary>
    public static InputReader Instance { get; private set; }

    private PlayerEntity _player;
    private PlayerInputActions _inputActions;

    private void Awake()
    {
        Instance = this;
        _player = GetComponent<PlayerEntity>();
        _inputActions = new PlayerInputActions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool InteractPressedThisFrame =>
        _inputActions != null && _inputActions.Player.Interact.WasPressedThisFrame();

    private void OnEnable()
    {
        _inputActions.Player.Enable();

        // Подписываемся на события кнопок
        _inputActions.Player.LightAttack.performed += OnLightAttack;
        _inputActions.Player.Interact.performed += OnInteract;
        _inputActions.Player.Throw.performed += OnThrowHeldItem;
        _inputActions.Player.Dodge.performed += OnDodge;
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        _player.Attack(); // Вызываем метод из скрипта, который мы писали ранее
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (_player == null)
            return;

        if (_player.equippedItem == null)
            PickUpItem.TryPickClosestInCursorRadius(_player);
        // Если в руке уже что-то — Interact можно оставить для NPC/дверей (TODO).
    }

    private void OnThrowHeldItem(InputAction.CallbackContext context)
    {
        if (_player.equippedItem != null)
            _player.equippedItem.Throw();
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<float>();
        Debug.Log($"Уклонение в сторону: {direction}");
        // Здесь будет вызов метода уклонения (вверх/вниз по линии)
    }
}