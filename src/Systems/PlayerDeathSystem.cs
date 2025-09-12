using System;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Content;
namespace MyGame.Systems;

public class PlayerDeathSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    float resetTimer = 0;
    static float TimeBeforeReset => 1f;
    public PlayerDeathSystem(World world) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if (!Some<ControlledByPlayer>() && GlobalState.ShouldExistPlayer)
        {
            resetTimer += (float)delta.TotalSeconds;
            if (resetTimer >= TimeBeforeReset)
            {
                if (Some<CauseOfDeath>())
                {
                    // var message = CreateEntity();
                    // Set(message, new ShouldPerformReset());
                    // Set(message, new DestroyOnLoad());
                    GlobalState.ShouldExistPlayer = false;
                    var cause = GetSingleton<CauseOfDeath>().Thing;
                    if (cause == ThingType.Bunny)
                    {
                        var message = CreateEntity();
                        Set(message, new DestroyOnLoad());
                        Set(message, new DisplayDeathScreen(cause));
                    }
                    else
                    {
                        var message = CreateEntity();
                        Set(message, new ShouldPerformReset());
                        Set(message, new DestroyOnLoad());
                    }
                }
            }
        }
        else
        {
            resetTimer = 0;
        }
    }

}