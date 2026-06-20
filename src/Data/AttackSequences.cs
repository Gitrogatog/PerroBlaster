// using System;
// using System.Collections.Generic;
// using System.Numerics;
// using MyGame.Components;
// using MyGame.Content;
// namespace MyGame.Data;


// public record struct AttackAction(float Time, SpriteAnimation? Animation, ShootAction? Shoot, JumpAction? Jump, VelocityAction? Velocity);
// public record struct AttackSequence(AttackAction[] Elements);
// public enum AttackSequenceType {
//     MoleThrow, Boomerang
// }

// public static class AttackSequences {
//     public static Dictionary<AttackSequenceType, AttackAction[]> Attacks = new Dictionary<AttackSequenceType, AttackAction[]>{
//         [AttackSequenceType.MoleThrow] = [
//             Animation(SpriteAnimations.mole_charge, 0),
//             AddAnimation(SpriteAnimations.mole_throw, Shoot(ShotType.Rock, AimType.AimAngle, Time.SINGLE_FRAME * 30))
            
//         ],
//         [AttackSequenceType.Boomerang] = [
//             Animation(SpriteAnimations.dragon_flower_charge, 0),
//             AddAnimation(SpriteAnimations.dragon_flower_attack, Shoot(ShotType.Flower, AimType.AimAngle, Time.SINGLE_FRAME * 45))
//         ],
//     };

//     private static AttackAction AddAnimation(SpriteAnimation anim, AttackAction action) => new AttackAction(action.Time, anim, action.Shoot, action.Jump, action.Velocity);
//     private static AttackAction AddAnimation(SpriteAnimationInfo anim, AttackAction action) => new AttackAction(action.Time, new SpriteAnimation(anim), action.Shoot, action.Jump, action.Velocity);
//     private static AttackAction AddAnimation(SpriteAnimationInfoID anim, AttackAction action) => new AttackAction(action.Time, new SpriteAnimation(SpriteAnimationInfo.FromID(anim)), action.Shoot, action.Jump, action.Velocity);
//     private static AttackAction Animation(SpriteAnimation anim, float time) => new AttackAction(time, anim, null, null, null);
//     private static AttackAction Animation(SpriteAnimationInfo anim, float time) => new AttackAction(time, new SpriteAnimation(anim), null, null, null);
//     private static AttackAction Animation(SpriteAnimationInfoID anim, float time) => new AttackAction(time, new SpriteAnimation(SpriteAnimationInfo.FromID(anim)), null, null, null);
//     private static AttackAction Shoot(ShotType ShotType, AimType AimType, float time) => new AttackAction(time, null, new ShootAction(ShotType, AimType), null, null);
//     private static AttackAction Jump(Vector2 Speed, float time) => new AttackAction(time, null, null, new JumpAction(Speed), null);
//     private static AttackAction Jump(float YSpeed, float time) => new AttackAction(time, null, null, new JumpAction(YSpeed), null);
//     private static AttackAction Velocity(Vector2 Speed, float time) => new AttackAction(time, null, null, null, new VelocityAction(Speed));
//     private static AttackAction End(float time) => new AttackAction(time, null, null, null, null);
// }