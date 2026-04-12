
using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Data;
using MyGame.Utility;

namespace MyGame.Systems;

public class FourDirectionAnimSystem : MoonTools.ECS.System
{
    MoonTools.ECS.Filter DirectionFilter;
    public FourDirectionAnimSystem(World world) : base(world)
    {
        DirectionFilter = FilterBuilder
        .Include<LastDirection>()
        .Include<FourDirectionAnim>()
        .Build();
    }

    public override void Update(TimeSpan delta)
    {
        // if(Some<ControlledByPlayer>()) {
        //     var player = GetSingletonEntity<ControlledByPlayer>();
        //     Console.WriteLine($"four:{Has<FourDirectionAnim>(player)} last:{Has<LastDirection>(player)}");
        // }
        // else {
        //     Console.WriteLine(
        //         "no player for you!"
        //     );
        // }
        
        foreach (var entity in DirectionFilter.Entities)
        {
            int x, y = 0;
            if(Has<FacingDirection>(entity)) {
                (x, y) = Get<FacingDirection>(entity);
            }
            else{
                (x, y) = Get<LastDirection>(entity);
            }
            // (int x, int y) = Has<FacingDirection>(entity) ? Get<FacingDirection>(entity) : Get<LastDirection>(entity);
            // if(TryGet<MoveToTile>(entity, out MoveToTile moveToTile)) {
            //     x = moveToTile.X - moveToTile.PrevX;
            //     y = moveToTile.Y - moveToTile.PrevY;
            //     // Set(entity, new FacingDirection(x, y));
            // }
            
            var animations = Get<FourDirectionAnim>(entity);

            SpriteAnimationInfo animation;

            if (x > 0)
            {
                animation = SpriteAnimationInfo.FromID(animations.Right);
            }
            else if (x < 0)
            {
                animation = SpriteAnimationInfo.FromID(animations.Left);
            }
            else
            {
                if (y > 0)
                {
                    animation = SpriteAnimationInfo.FromID(animations.Down);
                }
                else if (y < 0)
                {
                    animation = SpriteAnimationInfo.FromID(animations.Up);
                }
                else
                {
                    animation = Get<SpriteAnimation>(entity).SpriteAnimationInfo;
                }
            }
            // Console.WriteLine($"{animation.Name}");

            // var velocity = Has<Velocity>(entity) ? (Vector2)Get<Velocity>(entity) : Vector2.Zero;

            // int framerate = Get<SpriteAnimation>(entity).FrameRate;

            // if (Has<AdjustFramerateToSpeed>(entity))
            // {
            //     framerate = (int)(velocity.Length() / 20f);
            //     if (Has<FunnyRunTimer>(entity))
            //     {
            //         framerate = 25;
            //     }
            // }

            if (Has<MoveToTile>(entity) || Has<TempTileProgress>(entity) || (Has<Velocity>(entity) && Get<Velocity>(entity).Value != Vector2.Zero))
            {
                // Console.WriteLine($"moveToTile: {Has<MoveToTile>(entity)} temp: {Has<TempTileProgress>(entity)}");
                Set(entity, new SetAnimation(
                    new SpriteAnimation(animation),
                    PreserveFrame:true
                ));
                // Console.WriteLine("walk frame");
                // Send(new SetAnimationMessage(
                //     entity,
                //     new SpriteAnimation(animation, framerate, true)
                // ));
            }
            else
            {
                // Console.WriteLine("stopped!");
                // set pause frame to 2nd frame, rpgmaker sprites work like that
                // int targetFrame = animation.Frames.Length > 1 ? 1 : 0;
                // Set(entity, new SetAnimation(
                //     new SpriteAnimation(animation, 0, true, targetFrame),
                //     ForceUpdate: true
                // ));
                EntityUtils.SetStandAnim(World, entity, animation);

                // Send(new SetAnimationMessage(
                //     entity,
                //     new SpriteAnimation(animation, framerate, true, 0),
                //     true
                // ));
            }
        }
    }

}