using System.Numerics;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MyGame.Content;
using MyGame.Data;

namespace MyGame.Components;

public readonly record struct TouchingMouse();
public readonly record struct SliceAnim(SpriteAnimationInfoID Value);
public readonly record struct Clickable(ClickableState Prev, ClickableState Current);
public readonly record struct TextOffset(int X, int Y);
// public readonly record struct HasSkillList(int ID);
public readonly record struct Health(int Current, int Max)
{
    public Health(int health) : this(health, health) { }
}
public readonly record struct IsDead();
// public readonly record struct ApplyDamage(DamageType Type, int Amount);
// public readonly record struct ApplyStatus(StatusType Type, int Value);
// public readonly record struct ApplyHeal(int Amount);
public readonly record struct ControlledByPlayer();
public readonly record struct PressedThisFrame();
public readonly record struct SelectTargetOnClick();
public readonly record struct DestroyOnMessage<T>() where T : unmanaged;
public readonly record struct DestroyAtEndOfFrame();
// public readonly record struct Moveset(int ID);
// public readonly record struct SkillMoveset(int ID);
public readonly record struct UIBoxContainer(int MaxPerLine, int XOffset, int YOffset, bool ExpandVertical);
public readonly record struct UpdateUIThisFrame();
// public readonly record struct 


public readonly record struct Rectangle(int X, int Y, int Width, int Height)
{
    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public Rectangle(int Width, int Height) : this(-Width / 2, -Height / 2, Width, Height) { }

    public bool Intersects(Rectangle other)
    {
        return
            other.Left < Right &&
            Left < other.Right &&
            other.Top < Bottom &&
            Top < other.Bottom;
    }

    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        var x = int.Min(a.X, a.X);
        var y = int.Min(a.Y, b.Y);
        return new Rectangle(
            x,
            y,
            int.Max(a.Right, b.Right) - x,
            int.Max(a.Bottom, b.Bottom) - y
        );
    }

    public Rectangle Inflate(int horizontal, int vertical)
    {
        return new Rectangle(
            X - horizontal,
            Y - vertical,
            Width + horizontal * 2,
            Height + vertical * 2
        );
    }
}
public readonly record struct IsJumping();
public readonly record struct CanWallJump();
public readonly record struct TouchingWall(bool Right);
public readonly record struct ShouldPerformReset();
public readonly record struct SpawnOnTimerEnd(ThingType Thing);
public readonly record struct CauseOfDeath(ThingType Thing);
public readonly record struct DeathScreen();
public readonly record struct PreventInput();
public readonly record struct DestroyOnLoad();
public readonly record struct CanBeHeld();
public readonly record struct CanHold();
public readonly record struct CanBeThrown();
public readonly record struct CanPerformThrow();
public readonly record struct Kissable();
public readonly record struct DamageOnContact();
public readonly record struct TakeDamageOnContact();
public readonly record struct IgnoreCollision();
public readonly record struct ChangeLevel(int LevelID);
public readonly record struct CanCoyoteJump();
public readonly record struct CoyoteGrounded();
public readonly record struct CanPivot(float Decel, float MinSpeedToPivot);
public readonly record struct IsPivoting();
public readonly record struct DisplayDeathScreen(ThingType CauseOfDeath);
public readonly record struct AddAfterTime<T>(float Time, T Component) : TimedComponent<AddAfterTime<T>> where T : unmanaged
{
    public AddAfterTime<T> Update(float t)
    {
        return new AddAfterTime<T>(t, Component);
    }
}
public readonly record struct AccelParams(float groundAccel, float groundTurnAccel, float airAccel, float airTurnAccel)
{
    public float GetAccel(bool isGrounded, bool isTurning) => isGrounded ? (isTurning ? groundTurnAccel : groundAccel) : (isTurning ? airTurnAccel : airAccel);
}
public readonly record struct PlayStaticSFX(
    StaticSoundID StaticSoundID,
    SoundCategory Category = SoundCategory.Generic,
    float Volume = 1,
    float Pitch = 0,
    float Pan = 0
)
{
    public AudioBuffer Sound => StaticAudio.Lookup(StaticSoundID);
}
public readonly record struct SetAnimation(SpriteAnimation Animation, bool ForceUpdate = false)
{
    public SetAnimation(SpriteAnimationInfo animInfo, bool forceUpdate = false) : this(new SpriteAnimation(animInfo), forceUpdate) { }
}
public readonly record struct Facing(bool Right);
public readonly record struct RushAtPlayer(Cardinal Direction, float Distance, float Speed);
public readonly record struct TextSpriteParent();
public readonly record struct AdvanceCharSpeed(float TimePerCharacter);
public readonly record struct Grounded();
public readonly record struct Gravity();
public readonly record struct IntendedMove(float Value);
public readonly record struct MoveSpeed(float Value);
public readonly record struct AttemptJumpThisFrame();
public readonly record struct CanJump(float Value);
public readonly record struct MaxSpeedJump(float Value);
public readonly record struct BouncesOffWalls(float MinSpeed);
public readonly record struct CollidesWithSolids();
public readonly record struct Player(int Index);
public readonly record struct Orientation(float Angle);
public readonly record struct CanInteract();
public readonly record struct CanInspect();
public readonly record struct TryHold();
public readonly record struct Solid();
public readonly record struct TouchingSolid();
public readonly record struct DisplayCharCount(int Value);
public readonly record struct AdvanceCharCount(float Time) : TimedComponent<AdvanceCharCount>
{
    public AdvanceCharCount Update(float t)
    {
        return new AdvanceCharCount(t);
    }
}
public readonly record struct CreateMessageTimer<T>(float Time, T Message) : TimedComponent<CreateMessageTimer<T>> where T : unmanaged
{
    public CreateMessageTimer<T> Update(float t)
    {
        return new CreateMessageTimer<T>(t, Message);
    }
}
public readonly record struct CreateAnimationEntityOnTimerEnd(SpriteAnimation Animation, bool DeleteOnFinish = true, bool RelativeToSpawner = true, int X = 0, int Y = 0)
{
    public CreateAnimationEntityOnTimerEnd(SpriteAnimationInfo info, bool looping, bool deleteOnFinish = true, bool relativeToSpawner = true, int x = 0, int y = 0) : this(new SpriteAnimation(info, looping), deleteOnFinish, relativeToSpawner, x, y) { }
}
public readonly record struct DestroyOnAnimationFinish();
public readonly record struct Name(int TextID);
public readonly record struct WordWrap(int Max);
public readonly record struct Score(int Value);
public readonly record struct DisplayScore(int Value);

public readonly record struct Price(float Value);
public readonly record struct TickerText(float Width);
public readonly record struct ColorBlend(Color Color);
public readonly record struct CanFillOrders();
public readonly record struct CanGiveOrders();
public readonly record struct IsOrder();

public readonly record struct ColorSpeed(float RedSpeed, float GreenSpeed, float BlueSpeed);

public readonly record struct Depth(float Value);
public readonly record struct DrawAsRectangle();
public readonly struct NineSlice
{
    public readonly Sprite TopLeft, TopMid, TopRight, CenterLeft, CenterMid, CenterRight, BottomLeft, BottomMid, BottomRight, Original;
    public int Width => Original.SliceRect.W / 3;
    public int Height => Original.SliceRect.H / 3;
    public NineSlice(Sprite sprite)
    {
        int spriteWidth = (int)(sprite.SliceRect.W / 3);
        int spriteHeight = (int)(sprite.SliceRect.H / 3);
        Original = sprite;
        TopLeft = sprite.Slice(0, 0, spriteWidth, spriteHeight);
        TopMid = sprite.Slice(spriteWidth, 0, spriteWidth, spriteHeight);
        TopRight = sprite.Slice(spriteWidth * 2, 0, spriteWidth, spriteHeight);
        CenterLeft = sprite.Slice(0, spriteHeight, spriteWidth, spriteHeight);
        CenterMid = sprite.Slice(spriteWidth, spriteHeight, spriteWidth, spriteHeight);
        CenterRight = sprite.Slice(spriteWidth * 2, spriteHeight, spriteWidth, spriteHeight);
        BottomLeft = sprite.Slice(0, spriteHeight * 2, spriteWidth, spriteHeight);
        BottomMid = sprite.Slice(spriteWidth, spriteHeight * 2, spriteWidth, spriteHeight);
        BottomRight = sprite.Slice(spriteWidth * 2, spriteHeight * 2, spriteWidth, spriteHeight);
    }
}

public readonly record struct TextDropShadow(int OffsetX, int OffsetY);
public readonly record struct ForceIntegerMovement();
public readonly record struct MaxSpeed(float Value);

public readonly record struct AdjustFramerateToSpeed();
public readonly record struct FunnyRunTimer(float Time); //Scooby doo style quick run when starting to move
public readonly record struct CanFunnyRun();

public readonly record struct LastDirection(System.Numerics.Vector2 Direction);
public readonly record struct SlowDownAnimation(int BaseSpeed, int step);

public readonly record struct IsPopupBox(); // jank because we cant check relation type count
public readonly record struct CanSpawn(int Width, int Height);
public readonly record struct FallSpeed(float Speed);
public readonly record struct DestroyAtScreenBottom();

public readonly record struct IsScoreScreen(); // sorry
public readonly record struct GameInProgress(); // yaaargh

public readonly record struct DirectionalSprites(
    SpriteAnimationInfoID Up,
    SpriteAnimationInfoID UpRight,
    SpriteAnimationInfoID Right,
    SpriteAnimationInfoID DownRight,
    SpriteAnimationInfoID Down,
    SpriteAnimationInfoID DownLeft,
    SpriteAnimationInfoID Left,
    SpriteAnimationInfoID UpLeft
    );

public readonly record struct CanTalk();
public readonly record struct DontSpawnNPCs();
public readonly record struct StoreExit();
public readonly record struct AccelerateToPosition(Position Target, float Acceleration, float MotionDampFactor);
public readonly record struct DestroyAtGameEnd();

public readonly record struct CanBeStolenFrom();
public readonly record struct CanStealProducts();
public readonly record struct CanTargetProductSpawner();
public readonly record struct DestroyWhenOutOfBounds();

public readonly record struct WaitingForProductRestock();
public readonly record struct DestroyForDebugTestReasons();
public readonly record struct ColorFlicker(int ElapsedFrames, Color Color);
public readonly record struct MotionDamp(float Damping);
public readonly record struct SpriteScale(System.Numerics.Vector2 Scale)
{
    public SpriteScale(int scale) : this(new Vector2(scale, scale)) { }
}
public readonly record struct LastValue(int value);