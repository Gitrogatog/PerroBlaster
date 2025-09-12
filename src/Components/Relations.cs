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
public readonly record struct DontDraw();
public readonly record struct Colliding();
public readonly record struct Throwing(float Progress);
public readonly record struct Offset(float X, float Y)
{
    public Offset(Vector2 input) : this(input.X, input.Y) { }
    public Vector2 Value => new Vector2(X, Y);
}