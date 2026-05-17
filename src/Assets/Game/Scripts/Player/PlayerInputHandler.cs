using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    [Header("Movement")]
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool BlockHeld { get; private set; }
    public bool ParryPressed { get; private set; }[Header("Actions")]
    public bool DodgePressed { get; private set; }

    private void Awake() => inputActions = new PlayerInputActions();
    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (Keyboard.current == null) return;

        MoveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        BlockHeld = inputActions.Player.Parry.IsPressed();
        
        SprintHeld = Keyboard.current.leftShiftKey.isPressed; 

        if (!AttackPressed) AttackPressed = inputActions.Player.Attack.WasPressedThisFrame();
        if (!ParryPressed) ParryPressed = inputActions.Player.Parry.WasPressedThisFrame();
        if (!DodgePressed) DodgePressed = inputActions.Player.Dodge.WasPressedThisFrame();
    }

    public void ConsumeAttack() => AttackPressed = false;
    public void ConsumeParry() => ParryPressed = false;
    public void ConsumeDodge() => DodgePressed = false;
}