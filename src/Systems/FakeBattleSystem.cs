using System;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
namespace MyGame.Systems;

public class FakeBattleSystem : MoonTools.ECS.System
{
    private MoonTools.ECS.Filter PisonFilter;
    private MoonTools.ECS.Filter DaisyFilter;

    public FakeBattleSystem(World world) : base(world)
    {
        PisonFilter = FilterBuilder
            .Include<IsPison>()
            .Build();
        DaisyFilter = FilterBuilder
            .Include<IsDaisy>()
            .Build();
    }

    public override void Update(TimeSpan delta)
    {
        if(Some<StartFakeBattle>()) {
            DestroyAll<StartFakeBattle>();
            const float battleIntroLength = 3.4f;
            var preventInput = CreateEntity();
            Set(preventInput, new DestroyOnLoad());
            Set(preventInput, new PreventInput());
            var battleBackground = CreateEntity();
            Set(battleBackground, new Position(Dimensions.GAME_W / 2 + Globals.CameraX, Dimensions.GAME_H / 2 + Globals.CameraY));
            // Set(battleBackground, new FollowCameraWithOffset());
            Set(battleBackground, new SpriteScale(0));
            Set(battleBackground, new SpriteAnimation(SpriteAnimations.battle_background));
            Set(battleBackground, new LerpValue<SpriteScale>(0, 1, battleIntroLength - 0.5f));
            Set(battleBackground, new LerpValue<Rotation>(0, 6 * MathF.PI, battleIntroLength - 0.5f));
            Set(battleBackground, new Depth(0.08f));
            Set(battleBackground, new DestroyOnLoad());
            Set(CreateEntity(), new StopAllMusic());

            // Set(battleBackground, new GrowSpriteScale(0, 1, battleIntroLength));
            Set(CreateEntity(), new PlayStaticSFX(StaticAudio.battle_intro));
            var daisyTimer = CreateEntity();
            Set(daisyTimer, new Timer(battleIntroLength + 0.5f));
            Set(daisyTimer, new SpawnOnTimerEnd(ThingType.DaisyBattleSprite));
            Set(daisyTimer, new Position(Dimensions.GAME_W / 2, Dimensions.GAME_H - 50));
            float pisonTime = battleIntroLength;
            float beatTime = 1;
            InitalPison(pisonTime, 0);
            
            pisonTime += beatTime * 8f;
            LaterPison(pisonTime, 64);
            pisonTime += beatTime * 5f;
            LaterPison(pisonTime, -58);
            pisonTime += beatTime * 3.5f;
            LaterPison(pisonTime, 32);
            pisonTime += beatTime * 2f;
            LaterPison(pisonTime, 97);
            pisonTime += beatTime * 1.2f;
            LaterPison(pisonTime, -82);
            pisonTime += beatTime * 0.8f;
            LaterPison(pisonTime, -26);
            pisonTime += beatTime * 0.6f;
            LaterPison(pisonTime, 92);
            pisonTime += beatTime * 0.4f;
            LaterPison(pisonTime, -35f);
            pisonTime += beatTime * 0.3f;
            LaterPison(pisonTime, 15);
            pisonTime += beatTime * 0.3f;
            LaterPison(pisonTime, -105);
            pisonTime += beatTime * 0.2f;
            LaterPison(pisonTime, 20);
            pisonTime += beatTime * 0.2f;
            LaterPison(pisonTime, -31);
            pisonTime += beatTime * 0.2f;
            LaterPison(pisonTime, 110);
            pisonTime += beatTime * 0.2f;
            LaterPison(pisonTime, -70);
            pisonTime += 2.1f;
            Set(CreateEntity(), new AddAfterTime<PisonPlayAnim>(pisonTime + 0.2f, new PisonPlayAnim(SpriteAnimations.pison_windup)));
            Set(CreateEntity(), new AddAfterTime<PlayStaticSFX>(pisonTime, new PlayStaticSFX(StaticAudio.whip_start, Volume: 3f)));
            pisonTime += 3.8f - 2.1f;
            Set(CreateEntity(), new AddAfterTime<PisonPlayAnim>(pisonTime, new PisonPlayAnim(new SpriteAnimation(SpriteAnimations.pison_attack, loop:false))));
            Set(CreateEntity(), new AddAfterTime<PlayStaticSFX>(pisonTime + 0.03f, new PlayStaticSFX(StaticAudio.whip_end, Volume: 2.3f)));
            Set(CreateEntity(), new AddAfterTime<SetDaisyColorOverlay>(pisonTime + 0.03f, new SetDaisyColorOverlay(Color.White)));
            Set(CreateEntity(), new AddAfterTime<ChangeGameScene>(pisonTime + 0.11f, new ChangeGameScene(GameSceneType.GameOver)));
            // play animation
            

        }
        if(Some<PisonPlayAnim>()) {
            var anim = GetSingleton<PisonPlayAnim>().anim;
            DestroyAll<PisonPlayAnim>();
            foreach(var entity in PisonFilter.Entities) {
                Set(entity, new SetAnimation(anim));
            }
        }
        if(Some<SetDaisyColorOverlay>()) {
            var color = GetSingleton<SetDaisyColorOverlay>().Color;
            foreach(var entity in DaisyFilter.Entities) {
                Set(entity, new ColorOverlay(color));
            }
        }
    }
    void InitalPison(float time, float yOffset) => DelayCreatePison(time, 1f, yOffset);
    void LaterPison(float time, float yOffset) => DelayCreatePison(time, 0.5f, yOffset);
    void DelayCreatePison(float time, float pisonSpawnTime, float yOffset) {
        Set(CreateEntity(), new AddAfterTime<PlayMusic>(time, new PlayMusic(StreamingAudio.boss_loop_short, false)));
        // Set(CreateEntity(), new AddAfterTime<PlayStaticSFX>(time, new PlayStaticSFX(StaticAudio.boss_loop_short)));
        Set(CreateEntity(), new AddAfterTime<PlayStaticSFX>(time + pisonSpawnTime, new PlayStaticSFX(StaticAudio.boss_enter, Volume:3f)));
        var pisonTimer = CreateEntity();
        Set(pisonTimer, new Timer(time + pisonSpawnTime));
        Set(pisonTimer, new SpawnOnTimerEnd(ThingType.PisonSprite));
        Set(pisonTimer, new Position(Dimensions.GAME_W / 2 - 10 + yOffset, Dimensions.GAME_H / 2));
    }
}