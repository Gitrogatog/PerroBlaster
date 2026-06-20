using System.Numerics;
using MoonTools.ECS;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MyGame.Content;
using MyGame.Data;

namespace MyGame.Components;

public readonly record struct SpawnThingWhen<T>(ThingType Thing) where T : ITrigger;
public readonly record struct ShootWhen<T>(ShotType ShotType, AimType AimType) where T : ITrigger;
public readonly record struct ShootAfterTimeWhen<T>(ShotType ShotType, AimType AimType, float Time) where T : ITrigger;
public readonly record struct PlaySFXWhen<T>(StaticSoundID Sound) where T : ITrigger;
public readonly record struct CreateAnimWhen<T>(SpriteAnimationInfoID Anim) where T : ITrigger;
// public readonly record struct AddAfterTimeWhen<T1, T2>(T2 Component, float Time) where T1 : ITrigger where T2 : unmanaged {
//     public AddAfterTimeWhen(float Time) : this(default, Time) {}
// }

public readonly record struct JumpTrigger : ITrigger;
public readonly record struct TouchGroundTrigger : ITrigger;
public readonly record struct BumpCeilingTrigger : ITrigger;
public readonly record struct BumpWallTrigger : ITrigger;
public readonly record struct KillTrigger : ITrigger;