using System.Numerics;
using MyGame.Data;

namespace MyGame.Relations;

public readonly record struct Targeting();
public readonly record struct EffectSource();
public readonly record struct UnitVisual();
public readonly record struct UnitTargetButton();
public readonly record struct PlayerChosenTarget();
public readonly record struct DisplayMoveListUI();
public readonly record struct IncludeContainer();
public readonly record struct Child();
public readonly record struct Invincible;
public readonly record struct HealthUI();
public readonly record struct DontDraw();
public readonly record struct DontDamage;
public readonly record struct DontShoot;
public readonly record struct Colliding();
public readonly record struct Throwing(float Progress);
public readonly record struct Offset(float X, float Y)
{
    public Offset(Vector2 input) : this(input.X, input.Y) { }
    public Vector2 Value => new Vector2(X, Y);
}
public readonly record struct OffsetSingleFrame(float X, float Y)
{
    public OffsetSingleFrame(Vector2 input) : this(input.X, input.Y) { }
    public Vector2 Value => new Vector2(X, Y);
}
public readonly record struct OffsetAimAngle(float Offset);
public readonly record struct FollowXWithOffset(int Offset);
public readonly record struct FollowYWithOffset(int Offset);