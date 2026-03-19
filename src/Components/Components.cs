using System;
using System.IO;
using System.Numerics;
using MoonTools.ECS;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MyGame.Content;
using MyGame.Data;
using MyGame.Utility;

namespace MyGame.Components;

public readonly record struct PlayInteractSounds;
public readonly record struct DoNotPlayInteractSounds;
public readonly record struct PlayMusic(StreamingSoundID ID, bool IgnoreIfAlreadyPlaying=true);
public readonly record struct StopMusic(StreamingSoundID ID);
public readonly record struct StopMusicUnless(StreamingSoundID ID);
public readonly record struct StopAllMusic();
// public readonly record struct StartMusic(StreamingSoundID ID);
public readonly record struct SetMusicVolume(float Value, StreamingSoundID ID);
public readonly record struct StartMusicAndSetVolume(float Value, StreamingSoundID ID);
public readonly record struct ChangeGameScene(GameSceneType Scene);
public readonly record struct Collision(Entity Effector, Entity Effected);
public readonly record struct EntityCollisionHash(Entity Entity, EffectorFlags EffectorFlags, EffectedFlags EffectedFlags);
public readonly record struct CollisionFlags(EffectorFlags EffectorFlags, EffectedFlags EffectedFlags);
public readonly record struct EnemySpawnPoint(int X, int Y, EnemyType EnemyType);
public readonly record struct RectangleSpawnPoint(int X, int Y, int Width, int Height, RectThingType Type);
public readonly record struct IsCheckpoint;
public readonly record struct CollisionForceMoveForOneFrame(Vector2 Direction);
public readonly record struct FinishStepThisFrame();
public readonly record struct AttemptTalkThisFrame();
public readonly record struct TilePosition(int X, int Y);
public readonly record struct MoveToTile(int X, int Y, int PrevX, int PrevY, float Speed, float Progress);
public readonly record struct AttemptMoveToTile(int TargetX, int TargetY);
public readonly record struct UnprocessedTilePosition();
public readonly record struct TempTileProgress(float Value);
public readonly record struct CurrentOption;
public readonly record struct DisableAllUISelect;
public readonly record struct PlaySFXOnInteract(StaticSoundID ID);
public readonly record struct PlaySFXOnSelect(StaticSoundID ID);
public readonly record struct UIOption;
public readonly record struct ChangeSceneOnSelect(GameSceneType Scene);
public readonly record struct SetCharacterTypeOnSelect(CharacterType Value);
public readonly record struct CloseWindowOnSelect;
public readonly record struct CloseGameWindow;
public readonly record struct LoadVideo;
public readonly record struct PlayVideo;
public readonly record struct FourDirectionAnim(SpriteAnimationInfoID Up, SpriteAnimationInfoID Down, SpriteAnimationInfoID Left, SpriteAnimationInfoID Right) {
    public FourDirectionAnim(SpriteAnimationInfo Up, SpriteAnimationInfo Down, SpriteAnimationInfo Left, SpriteAnimationInfo Right)
        : this(Up.ID, Down.ID, Left.ID, Right.ID) {}
}
public readonly record struct StartFakeBattle;
public readonly record struct IsPison;
public readonly record struct IsDaisy;
public readonly record struct SetDaisyColorOverlay(Color Color);
public readonly record struct PisonPlayAnim(SpriteAnimation anim) {
    public PisonPlayAnim(SpriteAnimationInfo animationInfo) : this(new SpriteAnimation(animationInfo, loop: false)) {}
}
public readonly record struct TouchingMouse();
public readonly record struct SliceAnim(SpriteAnimationInfoID Value);
public readonly record struct Clickable(ClickableState Prev, ClickableState Current);
public readonly record struct TextOffset(int X, int Y);
// public readonly record struct HasSkillList(int ID);
public readonly record struct Health(int Current, int Max)
{
    public Health(int health) : this(health, health) { }
}
public readonly record struct OwnedByEnemy;
public readonly record struct OwnedByPlayer;
public readonly record struct IsDead();
public readonly record struct TookDamageThisFrame();
public readonly record struct TookDamageLastFrame();
public readonly record struct CanBeThrown;
public readonly record struct CanBeStuck;
public readonly record struct CanBeRecalled;
public readonly record struct CanBeHeld;
public readonly record struct CantMoveTimer(float Time) : TimedComponent<CantMoveTimer>
{
    public CantMoveTimer Update(float t) => new CantMoveTimer(t);
}
public readonly record struct CantShootTimer(float Time) : TimedComponent<CantShootTimer>
{
    public CantShootTimer Update(float t) => new CantShootTimer(t);
}

public readonly record struct ReturnToPlayerWhenTouched;
public readonly record struct MoveTowardPlayer;
public readonly record struct MoveToPosition(Position Position);
public readonly record struct AimAtPlayer;
public readonly record struct IsAxe();
public readonly record struct SelectHighlight(int XOffset, int YOffset);
// public readonly record struct ApplyDamage(DamageType Type, int Amount);
// public readonly record struct ApplyStatus(StatusType Type, int Value);
// public readonly record struct ApplyHeal(int Amount);
public readonly record struct ControlledByPlayer();
public readonly record struct LastPlayerData(SpriteAnimation Animation, FacingDirection Direciton);
public readonly record struct PressedThisFrame();
public readonly record struct SelectTargetOnClick();
public readonly record struct DestroyOnMessage<T>() where T : unmanaged;
public readonly record struct DestroyAtEndOfFrame();
public readonly record struct DestroyAtStartOfFrame();
public readonly record struct LockCamera(int X, int Y);
public readonly record struct CameraFollow();
public readonly record struct FollowCameraWithOffset(int X, int Y);
public readonly record struct SpinOffset(float Distance, float Speed, float Progress);
public readonly record struct CreateHealthUI(int X, int Y);
public readonly record struct DisplayHealthUI(int X, int Y);
public readonly record struct RequireParent<T>() where T : unmanaged;
public readonly record struct RequireChild<T>() where T : unmanaged;
public readonly record struct UIBoxContainer(int MaxPerLine, int XOffset, int YOffset, bool ExpandVertical);
public readonly record struct UpdateUIThisFrame();
public readonly record struct Cursor();
public readonly record struct InvincibleOnDamage(float Time);
public readonly record struct ReturnToCheckpoint;
public readonly record struct ColorOverlayTimer(Color Color, float Time) : TimedComponent<ColorOverlayTimer>
{
    public ColorOverlayTimer Update(float t) => new ColorOverlayTimer(Color, t);
}
public readonly record struct Rotation(float Value);
public readonly record struct RotateSpeed(float Value);
public readonly record struct RotateSpriteToAimAngle;
public readonly record struct AimAngle(Vector2 Angle) {
    public AimAngle(float X, float Y) : this(new Vector2(X, Y)) {}
}

public readonly record struct Rectangle(int X, int Y, int Width, int Height, EffectorFlags EffectorFlags, EffectedFlags EffectedFlags)
{
    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;

    public Rectangle(int Width, int Height, EffectorFlags EffectorFlags, EffectedFlags EffectedFlags) : this(-Width / 2, -Height / 2, Width, Height, EffectorFlags, EffectedFlags) { }

    public bool Intersects(Rectangle other)
    {
        return
           ((int)EffectorFlags & (int)other.EffectedFlags) != 0 &&
            other.Left < Right &&
            Left < other.Right &&
            other.Top < Bottom &&
            Top < other.Bottom;
    }

    public static bool TestOverlap(Rectangle a, Rectangle b){
        return b.Left < a.Right && a.Left < b.Right && b.Top < a.Bottom && a.Top < b.Bottom;
    }

    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        var x = int.Min(a.X, a.X);
        var y = int.Min(a.Y, b.Y);
        return new Rectangle(
            x,
            y,
            int.Max(a.Right, b.Right) - x,
            int.Max(a.Bottom, b.Bottom) - y,
            a.EffectorFlags,
            a.EffectedFlags
        );
    }

    public Rectangle Inflate(int horizontal, int vertical)
    {
        return new Rectangle(
            X - horizontal,
            Y - vertical,
            Width + horizontal * 2,
            Height + vertical * 2,
            EffectorFlags,
            EffectedFlags
        );
    }
}
public readonly record struct TouchingWall(bool Right);
public readonly record struct ShouldPerformReset();
public readonly record struct SpawnOnTimerEnd(ThingType Thing);
public readonly record struct CauseOfDeath(ThingType Thing);
public readonly record struct DeathScreen();
public readonly record struct PreventInput();
public readonly record struct UUID(int ID);
public readonly record struct StopInteract();
public readonly record struct CanBeStepped();
public readonly record struct CanBeTalked();
public readonly record struct ChangeLevelOnInteract(int LevelID, int EntityUIID);
public readonly record struct DisplayDialogOnInteract(int DialogID, CloseDialogAction CloseDialogAction);
public readonly record struct DisplayDialog(int DialogID, CloseDialogAction CloseDialogAction);
public readonly record struct PlaySFXOnClose();
public readonly record struct SpawnNonTilePlayer;
// public readonly record struct ChangeLevelOnTalk(int LevelID, int EntityUIID);
public readonly record struct InitialPlayerSpawn(int X, int Y);
public readonly record struct ChangeLevel(int LevelID, int EntityUUID);
public readonly record struct DestroyOnLoad();
public readonly record struct DestroyOnSceneChange();
public readonly record struct DestroyOnPlayerRespawn;
public readonly record struct DamageOnContact;
public readonly record struct DestroyOnContact;
public readonly record struct TakeDamageOnContact();
public readonly record struct DontRepeatDamageUntilStateChange;
public readonly record struct IgnoreCollision();
// public readonly record struct ChangeLevel(int LevelID);
public readonly record struct AddAfterTime<T>(float Time, T Component) : TimedComponent<AddAfterTime<T>> where T : unmanaged
{
    public AddAfterTime<T> Update(float t)
    {
        return new AddAfterTime<T>(t, Component);
    }
}
// public readonly record struct AddAfterTimeFade<T>(float CurrentTime, float MaxTime, T Component, Color StartColor, Color ) where T : unmanaged {
//     public AddAfterTimeFade<T> Update(float t) {
//         return new AddAfterTimeFade<T>(t, MaxTime, Component);
//     }
// }
// public readonly record struct AccelParams(float groundAccel, float groundTurnAccel, float airAccel, float airTurnAccel)
// {
//     public float GetAccel(bool isGrounded, bool isTurning) => isGrounded ? (isTurning ? groundTurnAccel : groundAccel) : (isTurning ? airTurnAccel : airAccel);
// }
public readonly record struct AccelParams(float Value);
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
// public readonly record struct PlayContinuousSFX(
//     StaticSoundID StaticSoundID,
//     SoundCategory Category = SoundCategory.Generic,
//     float Volume = 1,
//     float Pitch = 0,
//     float Pan = 0
// )
// {
//     public AudioBuffer Sound => StaticAudio.Lookup(StaticSoundID);
// }
public readonly record struct PlayContinuousSFX(StaticSoundID StaticSoundID,
        SoundCategory Category = SoundCategory.Generic,
        float Volume = 1,
        float Pitch = 0,
        float Pan = 0, int VoiceID = -1)
{
    public AudioBuffer Sound => StaticAudio.Lookup(StaticSoundID);
    public PlayContinuousSFX SetID(int id)
    {
        return new PlayContinuousSFX(StaticSoundID, Category, Volume, Pitch, Pan, id);
    }
}
public readonly record struct SetAnimation(SpriteAnimation Animation, bool PreserveFrame = false, bool ForceUpdate = false)
{
    public SetAnimation(SpriteAnimationInfo animInfo, bool preserveFrame = false, bool forceUpdate = false) : this(new SpriteAnimation(animInfo), preserveFrame, forceUpdate) { }
}
public readonly record struct Facing(bool Right);
public readonly record struct RushAtPlayer(Cardinal Direction, float Distance, float Speed);
public readonly record struct TextSpriteParent();
public readonly record struct AdvanceCharSpeed(float CharPerSecond);
public readonly record struct Grounded();
public readonly record struct Gravity();
public readonly record struct IntendedMove(Vector2 Value) {
    public IntendedMove(float X, float Y) : this(new Vector2(X, Y)) {}
}
public readonly record struct IntendedMoveOneFrame(Vector2 Value);
public readonly record struct BecomeSolidWhenNotColliding;
public readonly record struct MoveSpeed(float Value);
public readonly record struct GroundAirMoveSpeed(float Ground, float Air);
public readonly record struct AttemptJumpThisFrame();
public readonly record struct CanJump(float Value);
public readonly record struct MaxSpeedJump(float Value);
public readonly record struct BouncesOffWalls(float MinSpeed);
public readonly record struct BouncesOffWallsConsistent(float MinSpeed, float BounceSpeed);
public readonly record struct BounceOffWallsConsistent2(BouncesOffWallsConsistent LowSpeed, BouncesOffWallsConsistent HighSpeed);
public readonly record struct MoveDir(Vector2 Value)
{
    public MoveDir(float x, float y) : this(new Vector2(x, y)) { }
}
public readonly record struct CollidesWithSolids();
public readonly record struct AmbushPoint(int X, int Y, AmbushTrigger TriggerType);
public readonly record struct SpawnOnAmbush(int X, int Y, ThingType Thing);
public readonly record struct Player(int Index);
public readonly record struct Orientation(float Angle);
public readonly record struct CanInteract();
public readonly record struct CanInspect();
public readonly record struct TryHold();
public readonly record struct Solid();
public readonly record struct TouchingSolid();
public readonly record struct SetAdvanceCharCountOnDialogBoxOpen(float CharPerSecond);
public readonly record struct DisplayCharCount(int Value);
public readonly record struct DestroyOnOtherAnimClose;
public readonly record struct DestroyOnDialogBoxClose;
public readonly record struct DestroyOnDialogBoxFullyClose;
public readonly record struct PerformDialogBoxFullyClose;
public readonly record struct EnableAdvanceCharCount;
public readonly record struct IsDialogBox;
public readonly record struct CanAdvanceDialog;
public readonly record struct FlickerDontDraw(float Time, float MaxTime) : TimedComponent<FlickerDontDraw>
{
    public FlickerDontDraw(float time) : this(time, time) {}
    public FlickerDontDraw Update(float t) => new FlickerDontDraw(t, MaxTime);
}
public readonly record struct CreateDialogTextOnAnimFinish(int TextID);
public readonly record struct AdvanceCharCount(float NumChars, float CharPerSecond)
{
    public AdvanceCharCount(float charsPerSecond) : this(0, charsPerSecond) {}
    public AdvanceCharCount Update(float t)
    {
        return new AdvanceCharCount(t, CharPerSecond);
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
public readonly record struct SpawnOnAnimationFinish(ThingType Thing);
public readonly record struct KillTrigger : ITrigger;
public readonly record struct SpawnThingWhen<T>(ThingType Thing) where T : ITrigger;
public readonly record struct Name(int TextID);
public readonly record struct WordWrap(int Max);
public readonly record struct GrowRectToSize(int TargetY, int Frames, int CurrentFrame);
public readonly record struct ReverseAnimOnClose();
public readonly record struct ColorBlend(Color Color);
public readonly record struct ColorOverlay(Color Color);
public readonly record struct ClearColor(Color Color);

public readonly record struct Depth(float Value);
public readonly record struct DrawAsRectangle();
public readonly record struct AccelerateDir(Vector2 Value);
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
public readonly record struct LerpAlpha(float StartAlpha, float EndAlpha, float TotalTime, float Progress);
public readonly record struct LastDirection(int X, int Y);
public readonly record struct FacingDirection(int X, int Y);
public readonly record struct SlowDownAnimation(int BaseSpeed, int step);

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

public readonly record struct AccelerateToPosition(Position Target, float Acceleration, float MotionDampFactor);
public readonly record struct DestroyAtGameEnd();
public readonly record struct ColorFlicker(int ElapsedFrames, Color Color);
public readonly record struct MotionDamp(float Damping);
public readonly record struct SpriteScale(System.Numerics.Vector2 Scale)
{
    public SpriteScale(float scale) : this(new Vector2(scale, scale)) { }
}
// public readonly record struct LerpScale(float Start, float End, float MaxTime, float Progress)
//     : LerpComponent<LerpScale, SpriteScale>
// {
//     public SpriteScale Apply(float value) => new SpriteScale(value);
//     public LerpScale Update(float t) => new LerpScale(Start, End, MaxTime, t);
// }
public readonly record struct LerpValue<T>(float Start, float End, float MaxTime, float Progress) where T : unmanaged
{
    public LerpValue (float start, float end, float maxTime) : this(start, end, maxTime, 0) {}
}
public readonly record struct GrowSpriteScale(float Start, float End, float MaxTime, float Progress);
// public readonly record struct (float Start, float End, float MaxTime, float Progress);
public readonly record struct SpriteOffset(int X, int Y);
public readonly record struct LastValue(int value);