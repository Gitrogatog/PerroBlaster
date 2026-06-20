using System;
using System.Numerics;
using MoonTools.ECS;
using MyGame.Components;
using MyGame.Data;

namespace MyGame.Utility;

public static class ComponentUtils {
    public static float AnimSpeedMult(SpriteAnimationInfo anim, float walkSpeed) => anim.FrameRate / walkSpeed;
    public static SpriteAnimation SetAnimSpeed(SpriteAnimationInfoID anim, float animSpeedMult, float speed) => new SpriteAnimation(SpriteAnimationInfo.FromID(anim), animSpeedMult * MathF.Abs(speed));
}
