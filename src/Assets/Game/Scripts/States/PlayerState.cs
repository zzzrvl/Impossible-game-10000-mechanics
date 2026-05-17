using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player; 
    protected PlayerStateMachine stateMachine;
    protected float stateTimer;
    
    public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {
        stateTimer = 0f;
    }

    public virtual void LogicUpdate() // every frame
    {
        stateTimer += Time.deltaTime; 
    } 
    //public virtual void PhysicsUpdate(){ //every fixedUpdate
    public virtual void Exit() { }
}