using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerInputHandler InputHandler { get; private set; }
    public CharacterController CC { get; private set; } 
    public Animator Animator { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerData Data; // Перетащите сюда ваш SO

    // Переменные для комбо (к ним обращается AttackState)
    public int ComboStep = 0;
    public float LastAttackTime = 0f;

    // Обновленные экземпляры состояний:
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerRunState RunState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerParryState ParryState { get; private set; }
    public PlayerDodgeState DodgeState { get; private set; }
    private void Awake()
    {
        CC = GetComponent<CharacterController>();
        InputHandler = GetComponent<PlayerInputHandler>();
        Animator = GetComponentInChildren<Animator>(); // Предполагается, что аниматор висит на дочернем объекте или на самом игроке
        StateMachine = new PlayerStateMachine();
        
        IdleState = new PlayerIdleState(this, StateMachine);
        WalkState = new PlayerWalkState(this, StateMachine);
        RunState = new PlayerRunState(this, StateMachine);
        AttackState = new PlayerAttackState(this, StateMachine);
        ParryState = new PlayerParryState(this, StateMachine);
        DodgeState = new PlayerDodgeState(this, StateMachine);
    }

    private void Start()
    {
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.CurrentState.LogicUpdate();
    }
}