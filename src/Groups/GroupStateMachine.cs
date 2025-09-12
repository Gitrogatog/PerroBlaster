

using System;
using System.Collections.Generic;
using MoonTools.ECS;
using MyGame.Systems;

public class GroupStateMachine<T> : MoonTools.ECS.System where T : unmanaged, Enum
{
    public List<MoonTools.ECS.System>[] States2;
    public Dictionary<T, List<MoonTools.ECS.System>> States;
    T CurrState;
    T tempStateForAddingSystems;
    Filter EntityFilter;
    public GroupStateMachine(World world) : base(world)
    {
        EntityFilter = FilterBuilder.Include<T>().Build();
        var enumVals = Enum.GetValues<T>();
        States = new Dictionary<T, List<MoonTools.ECS.System>>(enumVals.Length);
        foreach (T enumVal in enumVals)
        {
            States[enumVal] = new List<MoonTools.ECS.System>();
        }

        // States = new List<MoonTools.ECS.System>[enumVals.Length];
        // for (int i = 0; i < States.Length; i++)
        // {
        //     States[i] = new List<MoonTools.ECS.System>();
        // }
    }
    public List<MoonTools.ECS.System> GetStateList(T state)
    {
        return States[state];
    }
    public GroupStateMachine<T> SetTempState(T state)
    {
        tempStateForAddingSystems = state;
        return this;
    }
    public GroupStateMachine<T> Add(MoonTools.ECS.System system)
    {
        States[tempStateForAddingSystems].Add(system);
        return this;
    }
    public override void Update(TimeSpan delta)
    {
        if (!EntityFilter.Empty)
        {
            var state = GetSingleton<T>();
            EnterState(state);
            CurrState = state;
        }
        var systemList = States[CurrState];
        foreach (var system in systemList)
        {
            system.Update(delta);
        }
    }
    public void EnterState(T state)
    {

    }
}

public enum BasicState
{
    Start,
    Middle,
    End
}