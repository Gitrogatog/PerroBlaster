using System;
using MoonTools.ECS;
using MyGame.Components;
namespace MyGame.Systems;

public class EnemyStateMachine : MoonTools.ECS.System
{
    private Filter EntityFilter;

    public EnemyStateMachine(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<EnemyState>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        foreach (var entity in EntityFilter.Entities)
        {
            var state = Get<EnemyState>(entity);
            var type = Get<EnemyType>(entity);
            if(Has<ChangeEnemyState>(entity)) {
                var newState = Get<ChangeEnemyState>(entity).State;
                ChangeState(entity, type, state, newState);
                state = newState;
            }
            Tick(entity, type, state);
        }
    }

    public void Tick(Entity entity, EnemyType type, EnemyState state) {
        switch(state) {
            case EnemyState.Idle: {
                break;
            }
            case EnemyState.Hurt: {

                break;
            }
            case EnemyState.Fight: {
                break;
            }
        }
    }
    
    private void OnEnter(Entity entity, EnemyType type, EnemyState oldState, EnemyState newState) {
        switch(newState) {
            case EnemyState.Idle: {
                break;
            }
            case EnemyState.Hurt: {
                
                break;
            }
            case EnemyState.Fight: {
                break;
            }
        }
    }
    private void OnExit(Entity entity, EnemyType type, EnemyState oldState, EnemyState newState) {
        switch(newState) {
            case EnemyState.Idle: {
                break;
            }
            case EnemyState.Hurt: {
                break;
            }
            case EnemyState.Fight: {
                break;
            }
        }
    }
    private bool ChangeState(Entity entity, EnemyType type, EnemyState oldState, EnemyState newState) {
        OnExit(entity, type, oldState, newState);
        OnEnter(entity, type, oldState, newState);
        Set(entity, newState);
        return true;
    }
}