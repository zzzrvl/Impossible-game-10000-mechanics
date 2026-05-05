using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerInputHandler InputHandler { get; private set; }
    public Animator Animator { get; private set; }
    public PlayerStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        InputHandler = GetComponent<PlayerInputHandler>();
        Animator = GetComponentInChildren<Animator>(); // Предполагается, что аниматор висит на дочернем объекте или на самом игроке
        
        StateMachine = new PlayerStateMachine();
    }

    private void Start()
    {
        StateMachine.Initialize(new PlayerIdleState(this, StateMachine));
    }

    private void Update()
    {
        StateMachine.CurrentState?.Update();
    }
}