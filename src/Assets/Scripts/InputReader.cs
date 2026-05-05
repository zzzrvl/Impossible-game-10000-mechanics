using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    private PlayerEntity _player;
    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _player = GetComponent<PlayerEntity>();
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();

        // Подписываемся на события кнопок
        _inputActions.Player.LightAttack.performed += OnLightAttack;
        _inputActions.Player.Interact.performed += OnInteract;
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
        // Логика взаимодействия с предметами офиса
        if (_player.equippedItem != null)
        {
            _player.equippedItem.Throw();
        }
    }

    private void OnDodge(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<float>();
        Debug.Log($"Уклонение в сторону: {direction}");
        // Здесь будет вызов метода уклонения (вверх/вниз по линии)
    }
}