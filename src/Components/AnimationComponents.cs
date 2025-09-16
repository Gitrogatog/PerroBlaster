using MyGame.Data;

namespace MyGame.Components;


// public readonly record struct PivotAnimation(SpriteAnimationInfoID Animation) : AnimComponent;
// public readonly record struct IdleAnimation(SpriteAnimationInfoID Animation) : AnimComponent;
// public readonly record struct WalkAnimation(SpriteAnimationInfoID Animation) : AnimComponent;
// public readonly record struct AirAnimation(SpriteAnimationInfoID Animation) : AnimComponent;
public readonly record struct PlayerAnimationSet(SpriteAnimation Idle, SpriteAnimation Walk, SpriteAnimation Jump, SpriteAnimation Pivot)
{
    public PlayerAnimationSet(SpriteAnimationInfo idle, SpriteAnimationInfo walk, SpriteAnimationInfo jump, SpriteAnimationInfo pivot) : this(new SpriteAnimation(idle), new SpriteAnimation(walk), new SpriteAnimation(jump), new SpriteAnimation(pivot)) { }
}