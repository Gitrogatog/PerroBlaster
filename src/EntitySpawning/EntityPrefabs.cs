
using System;
using System.Numerics;
using MoonTools.ECS;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Relations;
using MyGame.Systems;
using MyGame.Utility;

namespace MyGame.Spawn;

public static class EntityPrefabs
{
    static EntityManipulator manipulator;
    static World World;
    public static void Init(World world)
    {
        World = world;
        manipulator = new EntityManipulator(world);
    }

    // public static Entity ChangeLevel(int levelID) => manipulator.CreateLoadSceneMessage(levelID);
    public static Entity CreatePlayer(int x, int y) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new Velocity());
        // Set(entity, new DrawAsRectangle());
        Set(entity, new Rectangle(10, 10));
        Set(entity, new ControlledByPlayer());
        Set(entity, new CanInteract());
        Set(entity, new CollidesWithSolids());
        Set(entity, new Facing());
        Set(entity, new Gravity(MoveConsts.GRAVITY));
        float moveSpeed = 50;
        
        Set(entity, new MoveSpeed(moveSpeed));
        Set(entity, EffectedFlags.CanTakeDamage);
        Set(entity, EffectorFlags.CanTouchWall | EffectorFlags.CanTouchDownPlatform);
        Set(entity, new DrawAsRectangle());
        Set(entity, new Depth(0.0000001f));
        Set(entity, new ColorBlend(new Color(255, 0, 0)));
        // Set(entity, new RiseFallAnimation(SpriteAnimations.perro_jump, SpriteAnimations.perro_fall));
        // Set(entity, CreateWalkSpeedAnim(SpriteAnimations.perro_walk, moveSpeed));
        // Set(entity, new IdleAnimation(SpriteAnimations.perro_idle));
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity CreateTestHitbox(int x, int y) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, new DrawAsRectangle());
        Set(entity, new Rectangle(16, 16));
        Set(entity, new CanInteract());
        Set(entity, EffectedFlags.None);
        Set(entity, EffectorFlags.CanDamage);
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity CreateTile(int x, int y, Sprite sprite, float depth) => manipulator.CreateTile(x, y, sprite, depth);
    public static Entity CreateAnimatedTile(int x, int y, SpriteAnimationInfo sprite, float depth) => manipulator.CreateAnimatedTile(x, y, new SpriteAnimation(sprite), depth);
    public static Entity AddSolidCollision(Entity entity, Rectangle rect, EffectedFlags flags) => manipulator.AddSolidCollision(entity, rect, flags);
    public static Entity CreateEntityOnTileGrid(int x, int y) => manipulator.CreateEntityOnTileGrid(x, y);
    public static Entity CreateTextbox(int textId) => manipulator.CreateTextbox(textId);
    public static Entity CreateDialogText(int textId, int x, int y) {
        var entity = manipulator.CreateText(x + 10 - Dimensions.GAME_W / 2, y - 20, 12, Fonts.RM2000AltID, textId);
        World.Set(entity, new DestroyOnDialogBoxClose());
        return entity;
    }
    public static Entity ScreenFadeToBlack(float time) => manipulator.CreateScreenFade(0, 1, time);
    public static Entity ScreenFadeToClear(float time) => manipulator.CreateScreenFade(1, 0, time);
    public static Entity ScreenStayBlack(float time) {
        var entity = manipulator.CreateScreenFade(1, 1, time);
        World.Set(entity, new Timer(time));
        return entity;
    }
    public static void ScreenStayBlackThenClear(float blackTime, float clearTime) {
        ScreenStayBlack(blackTime);
        manipulator.CreateOffsetScreenFade(1, 0, blackTime, clearTime);
    }
    public static Entity CreateThing(ThingType thing, int x, int y) => manipulator.CreateThing(thing, x, y);
    public static void CreateStartMenu() => manipulator.CreateStartMenu();
    public static Entity CreateTextWithDelay(int x, int y, int textID, float delay, float charsPerSecond, Color color) {
        var entity = manipulator.CreateText(x, y, 12, Fonts.RM2000AltID, textID);
        World.Remove<AdvanceCharCount>(entity);
        World.Set(entity, new AddAfterTime<AdvanceCharCount>(delay, new AdvanceCharCount(charsPerSecond)));
        World.Set(entity, color);
        return entity;
    }
    public static Entity CreateText(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left) 
        => manipulator.CreateTextEntity(x, y, size, fontID, text, horizontalAlignment);
    public static Entity CreateDestroyOnLoad() {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity CreateTimer<T>(Entity target, float time, T relation) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new Timer(time));
        World.Relate(entity, target, relation);
        return entity;
    }
    public static Entity CreateMessage<T>(T component) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyAtStartOfFrame());
        World.Set(entity, component);
        return entity;
    }
    public static Entity CreateMessageEndOfFrame<T>(T component) where T : unmanaged {
        var entity = World.CreateEntity();
        World.Set(entity, new DestroyAtEndOfFrame());
        World.Set(entity, component);
        return entity;
    }
    public static Entity CreateTimedMessage<T>(T component, float time) where T : unmanaged
    {
        var entity = World.CreateEntity();
        World.Set(entity, new AddAfterTime<T>(time, component));
        return entity;
    }
    public static Entity CreateVisual(int x, int y, SpriteAnimationInfo animation, float depth = 0.5f) => 
        CreateVisual(x, y, new SpriteAnimation(animation), depth);
    public static Entity CreateVisual(int x, int y, SpriteAnimation animation, float depth = 0.5f) {
        var entity = World.CreateEntity();
        World.Set(entity, new Position(x, y));
        World.Set(entity, animation);
        World.Set(entity, new Depth(depth));
        World.Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static void ChangeSceneFadeoutDelay(GameSceneType gameScene, float delay, float screenFadeTime) {
        manipulator.CreateOffsetScreenFade(0, 1, delay, screenFadeTime);
        CreateTimedMessage(new ChangeGameScene(gameScene), delay + screenFadeTime);
        // CreatePreventInputEntity();
    }
    public static void ChangeSceneFadeout(GameSceneType gameScene) {
        ScreenFadeToBlack(0.1f);
        CreateTimedMessage(new ChangeGameScene(gameScene), 7f / 6f);
        CreatePreventInputEntity();
        // Console.WriteLine("faindig out!");
        // Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void ChangeLevelFadeout(int levelId, int entityUUID) {
        ScreenFadeToBlack(0.5f);
        // CreateTimedMessage(new ChangeLevel(levelId, entityUUID), 2f);
        CreateTimedMessage(new ChangeLevel(levelId, entityUUID), 0.5f + 1f / 6f);
        CreatePreventInputEntity();
        Console.WriteLine("faindig out!");
        Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void EnterLevelFadein() {
        ScreenStayBlackThenClear(0.1f, 0.5f);
        // ScreenFadeToClear(0.5f);
        World.Set(CreatePreventInputEntity(), new Timer(0.6f));
    }
    private static Entity CreatePreventInputEntity() {
        var entity = World.CreateEntity();
        Set(entity, new PreventInput());
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity PlaySFX(StaticSoundID StaticSoundID,
        SoundCategory Category = SoundCategory.Generic,
        float Volume = 1,
        float Pitch = 0,
        float Pan = 0)
    {
        var entity = CreateEntity();
        Set(entity, new PlayStaticSFX(StaticSoundID, Category, Volume, Pitch, Pan));
        return entity;
    }
    public static WalkSpeedModAnimation CreateWalkSpeedAnim(SpriteAnimationInfo anim, float walkSpeed) =>
        new WalkSpeedModAnimation(anim.ID, anim.FrameRate / walkSpeed);
    public static bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(Has<T>(source)) {
            Set(target, World.Get<T>(source));
            return true;
        }
        return false;
    }

    static string GetTag(in Entity entity) => World.GetTag(entity);
	static bool Has<T>(in Entity Entity) where T : unmanaged => World.Has<T>(Entity);
	static bool Some<T>() where T : unmanaged => World.Some<T>();
	static ref T Get<T>(in Entity Entity) where T : unmanaged => ref World.Get<T>(Entity);
	static bool TryGet<T>(in Entity Entity, out T component) where T : unmanaged
	{
		if (Has<T>(Entity))
		{
			component = Get<T>(Entity);
			return true;
		}
		component = default;
		return false;
	}
	static ref T GetSingleton<T>() where T : unmanaged => ref World.GetSingleton<T>();
	static Entity GetSingletonEntity<T>() where T : unmanaged => World.GetSingletonEntity<T>();

	static ReverseSpanEnumerator<(Entity, Entity)> Relations<T>() where T : unmanaged => World.Relations<T>();
	static bool Related<T>(in Entity entityA, in Entity entityB) where T : unmanaged => World.Related<T>(entityA, entityB);
	static T GetRelationData<T>(in Entity entityA, in Entity entityB) where T : unmanaged => World.GetRelationData<T>(entityA, entityB);

	static ReverseSpanEnumerator<Entity> OutRelations<T>(in Entity entity) where T : unmanaged => World.OutRelations<T>(entity);
	static Entity OutRelationSingleton<T>(in Entity entity) where T : unmanaged => World.OutRelationSingleton<T>(entity);
	static bool HasOutRelation<T>(in Entity entity) where T : unmanaged => World.HasOutRelation<T>(entity);
	static int OutRelationCount<T>(in Entity entity) where T : unmanaged => World.OutRelationCount<T>(entity);
	static Entity NthOutRelation<T>(in Entity entity, int n) where T : unmanaged => World.NthOutRelation<T>(entity, n);

	static ReverseSpanEnumerator<Entity> InRelations<T>(in Entity entity) where T : unmanaged => World.InRelations<T>(entity);
	static Entity InRelationSingleton<T>(in Entity entity) where T : unmanaged => World.InRelationSingleton<T>(entity);
	static bool HasInRelation<T>(in Entity entity) where T : unmanaged => World.HasInRelation<T>(entity);
	static int InRelationCount<T>(in Entity entity) where T : unmanaged => World.InRelationCount<T>(entity);
	static Entity NthInRelation<T>(in Entity entity, int n) where T : unmanaged => World.NthInRelation<T>(entity, n);

    static Entity CreateEntity(string tag = "") => World.CreateEntity(tag);
	static void Tag(Entity entity, string tag) => World.Tag(entity, tag);
	static void Set<TComponent>(in Entity entity, in TComponent component) where TComponent : unmanaged => World.Set<TComponent>(entity, component);
	static void Set<TComponent>(in Entity entity) where TComponent : unmanaged => World.Set<TComponent>(entity, new TComponent());

	static void Remove<TComponent>(in Entity entity) where TComponent : unmanaged => World.Remove<TComponent>(entity);
	static void DestroyAll<TComponent>() where TComponent : unmanaged {
		while(Some<TComponent>()) {
			Destroy(GetSingletonEntity<TComponent>());
		}
	}
	static void RemoveAll<TComponent>() where TComponent : unmanaged {
		while(Some<TComponent>()) {
			Remove<TComponent>(GetSingletonEntity<TComponent>());
		}
	}

	static void Relate<TRelationKind>(in Entity entityA, in Entity entityB, TRelationKind relationData) where TRelationKind : unmanaged => World.Relate(entityA, entityB, relationData);
	static void Unrelate<TRelationKind>(in Entity entityA, in Entity entityB) where TRelationKind : unmanaged => World.Unrelate<TRelationKind>(entityA, entityB);
	static void UnrelateAll<TRelationKind>(in Entity entity) where TRelationKind : unmanaged => World.UnrelateAll<TRelationKind>(entity);
	static void Destroy(in Entity entity) => World.Destroy(entity);
}

internal class EntityManipulator : Manipulator
{
    T GetDefault<T>(Entity entity, T other) where T : unmanaged => Has<T>(entity) ? Get<T>(entity) : other;
    public Entity CreateAnimation(float x, float y, SpriteAnimation animation, float timer)
    {
        Entity entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, animation);
        Set(entity, new Timer(timer));
        return entity;
    }
    public Entity CreateThing(ThingType thing, int x, int y) {
        switch(thing) {
            case ThingType.DennyMenuOpen: {
                var entity = CreateEntity();
                Set(entity, new Position(x, y));
                Set(entity, new DestroyOnAnimationFinish());
                Set(entity, new SpawnOnAnimationFinish(ThingType.StartMenu));
                Set(entity, new Depth(0.8f));
                return entity;
            }
            case ThingType.StartMenu: {
                var textEntity = CreateEntity();
                Set(textEntity, new Position(x, y));
                // Set(textEntity, new SpriteAnimation(SpriteAnimations.start_menu_text));
                Set(textEntity, new DestroyOnLoad());
                Set(textEntity, new Position(x, y));
                Set(textEntity, new Depth(0.5f));
                var backgroundEntity = CreateEntity();
                Set(backgroundEntity, new Position(x, y));
                // Set(backgroundEntity, new SpriteAnimation(SpriteAnimations.start_menu_background));
                Set(backgroundEntity, new DestroyOnLoad());
                Set(backgroundEntity, new Depth(0.6f));
                Set(backgroundEntity, new ColorBlend(new Color(255, 255, 255, 159)));
                var startOption = CreateEntity();
                int xOffset = 0;
                int yOffsetInit = -16;
                int yOffsetEach = 16;
                Set(startOption, new Position(x + xOffset, y + yOffsetInit));
                Set(startOption, new ChangeSceneOnSelect(GameSceneType.Level));
                Set(startOption, new PlaySFXOnSelect(StaticAudio.Decision1));
                Set(startOption, new DrawAsRectangle());
                // Set(startOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                Set(startOption, new UIOption());
                Set(startOption, new DestroyOnLoad());
                var continueOption = CreateEntity();
                Set(continueOption, new Position(x + xOffset, y + yOffsetInit + yOffsetEach));
                Set(continueOption, new PlaySFXOnSelect(StaticAudio.Buzzer1));
                Set(continueOption, new UIOption());
                Set(continueOption, new DestroyOnLoad());
                Set(continueOption, new DrawAsRectangle());
                // Set(continueOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                var quitOption = CreateEntity();
                Set(quitOption, new Position(x + xOffset, y + yOffsetInit + yOffsetEach * 2));
                Set(quitOption, new CloseWindowOnSelect());
                Set(quitOption, new PlaySFXOnSelect(StaticAudio.Decision1));
                Set(quitOption, new UIOption());
                Set(quitOption, new DestroyOnLoad());
                Set(quitOption, new DrawAsRectangle());
                // Set(quitOption, new Rectangle(10, 6, EffectorFlags.None, EffectedFlags.None));
                int selectX = 0;
                int selectY = 0;
                var selectHighlight = CreateEntity();
                // Set(selectHighlight, new SpriteAnimation(SpriteAnimations.ui_blink2));
                Set(selectHighlight, new Position(x + xOffset + selectX, y + yOffsetInit + selectY));
                Set(selectHighlight, new DestroyOnLoad());
                Set(selectHighlight, new SelectHighlight(selectX, selectY));
                Set(selectHighlight, new Depth(0.55f));
                return textEntity;
            }
            case ThingType.PisonSprite: {
                var entity = CreateEntity();
                Set(entity, new Position(x + Globals.CameraX, y + Globals.CameraY));
                // Set(entity, new SpriteAnimation(SpriteAnimations.pison_idle));
                Set(entity, new ColorOverlayTimer(Color.White, 0.25f));
                Set(entity, new DestroyOnLoad());
                Set(entity, new IsPison());
                Set(entity, new Depth(0.05f));
                // Set(entity, new FollowCameraWithOffset());
                // Console.WriteLine("creating pison battle sptire");
                return entity;
            }
            case ThingType.DaisyBattleSprite: {
                var entity = CreateEntity();
                // Set(entity, new FollowCameraWithOffset());
                Set(entity, new Position(x + Globals.CameraX, y + Globals.CameraY));
                Set(entity, SpriteAnimations.daisy_up.Frames[1]);
                Set(entity, new ColorOverlayTimer(Color.White, 0.15f));
                Set(entity, new DestroyOnLoad());
                Set(entity, new SpriteScale(2));
                Set(entity, new Depth(0.05f));
                Set(entity, new IsDaisy());
                // Console.WriteLine("creating daisy battle sprite!");
                return entity;
            }
        }
        return default;
    }

    public Entity CreatePlayer(int x, int y)
    {
        Entity entity = CreateEntityOnTileGrid(x, y);
        Set(entity, new ControlledByPlayer());
        Set(entity, new Rectangle(10, 10));
        Set(entity, EffectorFlags.CanTouchWall);
        Set(entity, EffectedFlags.CanTakeDamage);
        // , EffectorFlags.CanTouchWall, EffectedFlags.CanTakeDamage
        
        Set(entity, new FourDirectionAnim(SpriteAnimations.daisy_up, SpriteAnimations.daisy_down, SpriteAnimations.daisy_left, SpriteAnimations.daisy_right));
        var lastData = Some<LastPlayerData>() ? GetSingleton<LastPlayerData>() : new LastPlayerData(new SpriteAnimation(SpriteAnimations.daisy_down), new FacingDirection(0, 1));
        EntityUtils.SetStandAnim(World, entity, lastData.Animation.SpriteAnimationInfo);
        Set(entity, new DestroyOnLoad());
        Set(entity, new MoveSpeed(120));
        Set(entity, new CameraFollow());
        Set(entity,lastData.Direciton);
        Set(entity, new Depth(0.4f));
        // Relate(entity, , new Offset());
        return entity;
    }
    public Entity CreateEntityOnTileGrid(int x, int y) {
        Entity entity = CreateEntity();
        AddEntityToTile(entity, x, y);
        Set(entity, EntityUtils.TileToWorld(x, y));
        Set(entity, new DestroyOnLoad());
        return entity;
    }
    void AddEntityToTile(Entity entity, int x, int y) {
        Set(entity, new TilePosition(x, y));
        Set(entity, new UnprocessedTilePosition());
    }
    public Entity CreateTextbox(int textId) {
        var entity = CreateEntity();
        Set(entity, new Position(Dimensions.GAME_W / 2 + Globals.CameraX, 200 + Globals.CameraY));
        // Set(entity, new GrowRectToSize(80, 6, 0));
        // Set(entity, new Rectangle(Dimensions.GAME_W, 12, EffectorFlags.None, EffectedFlags.None));
        // Set(entity, new NineSlice(SpriteAnimations.ui_nine_slice.Frames[0]));
        Set(entity, new IsDialogBox());
        // Set(entity, new SpriteAnimation(SpriteAnimations.dialogbox, loop:false));
        Set(entity, new EnableAdvanceCharCount());
        Set(entity, new CreateDialogTextOnAnimFinish(textId));
        Set(entity, new Depth(0.02f));

        int textX = 8;
        int textY = 16;
        // var textEntity = CreateText(textX, textY + 160, 10, Fonts.RM2000AltID, textId);
        // Set(textEntity, new DestroyOnDialogBoxClose());
        return entity;
    }
    // public Entity CreateTextbox(int textId) {
    //     var entity = CreateEntity();
    //     Set(entity, new DestroyOnLoad());
    //     Set(entity, new NineSlice(SpriteAnimations.ui_nine_slice.Frames[0]));
    //     Set(entity, new Rectangle(0, 0, 300, 12, EffectorFlags.None, EffectedFlags.None));
    //     Set(entity, new GrowRectToSize(80, 6, 0));
    //     var text = CreateText(textId);
    //     Relate(entity, text, new DontDraw());
    //     return entity;
    // }
    public Entity CreateText(int x, int y, int size, FontID fontID, int textID) {
        var entity = CreateEntity();
        Set(entity, new Text(fontID, size, textID));
        Set(entity, new Position(x, y));
        Set(entity, new DisplayCharCount(0));
        Set(entity, new DestroyOnLoad());
        Set(entity, new AdvanceCharCount(120));
        Set(entity, new WordWrap(270));
        return entity;
    }
    void CreateTextTest()
    {
        var entity = CreateEntity();
        Set(entity, new Position(100, 100));
        Set(entity, new Text(Fonts.RM2000ID, 24, "HELLO World Dude!"));
        Set(entity, new DisplayCharCount(0));
        Set(entity, new DestroyOnLoad());
        Set(entity, new AdvanceCharCount(1, 0));
        Set(entity, new WordWrap(5));
    }
    public Entity CreateTile(int x, int y, Sprite tileSprite, float depth)
    {
        var entity = CreateEntity("TILE");
        Set(entity, new Position(x, y));
        Set(entity, tileSprite);
        Set(entity, new DestroyOnLoad());
        Set(entity, new Depth(depth));
        return entity;
    }
    public Entity CreateAnimatedTile(int x, int y, SpriteAnimation tileAnim, float depth) {
        var entity = CreateEntity();
        Set(entity, new Position(x, y));
        Set(entity, tileAnim);
        Set(entity, new DestroyOnLoad());
        Set(entity, new Depth(depth));
        return entity;
    }
    public Entity AddSolidCollision(Entity entity, Rectangle rect, EffectedFlags flags)
    {
        Set(entity, new Solid());
        Set(entity, new DrawAsRectangle());
        // Set(entity, new CanInteract());
        Set(entity, rect);
        Set(entity, flags);
        return entity;
    }
    public void CreateStartMenu() {
        // CreateTextEntity(60, 20, 8, Fonts.KosugiID, "BUBBA");
        // var subTitle = CreateTextEntity(70, 65, 12, Fonts.PixeltypeID, "AXE");
        var title = CreateEntity();
        Set(title, new DestroyOnLoad());
        Set(title, new Position(Dimensions.GAME_W / 2, Dimensions.GAME_H / 2));
        // Set(title, new SpriteAnimation(SpriteAnimations.dennys_start));
        Set(title, new Depth(1f));
        EntityPrefabs.ScreenStayBlackThenClear(0.2f, 0.5f);
        int x = 128 + 64 / 2;
        int y = 148 + 64 / 2;
        var timer1 = CreateEntity();
        Set(timer1, new Timer(0.7f));
        Set(timer1, new SpawnOnTimerEnd(ThingType.DennyMenuOpen));
        Set(timer1, new Position(x, y));
        // var timer2 = CreateEntity();
        // Set(timer2, new Timer(1.6f));
        // Set(timer2, new SpawnOnTimerEnd(ThingType.StartMenu));
        // Set(timer2, new Position(x, y));

        // CreateTextEntity(50, 110, 12, Fonts.PixeltypeID, "Instructions:");
        // CreateTextEntity(50, 130, 12, Fonts.PixeltypeID, "WASD/Arrow keys: Move");
        // CreateTextEntity(50, 150, 12, Fonts.PixeltypeID, "Left Click: Throw/Recall Axe");
        // CreateTextEntity(50, 170, 12, Fonts.PixeltypeID, "Right Click: Teleport To Axe");
        // CreateTextEntity(50, 190, 12, Fonts.PixeltypeID, "-/= : Decrease/Increase Window Size");
        // CreateTextEntity(50, 210, 12, Fonts.PixeltypeID, "Right Click to Start!");
    }
    public void CreateLastMenu(){
        // CreateTextEntity(Dimensions.GAME_W / 2, 20, 10, Fonts.KosugiID, "YOU WIN!", HorizontalAlignment.Center);
        // CreateTextEntity(Dimensions.GAME_W / 2, 80, 12, Fonts.PixeltypeID, "Good job Bubba, you saved the day!!!", HorizontalAlignment.Center);
        // CreateTextEntity(Dimensions.GAME_W / 2, 110, 12, Fonts.PixeltypeID, $"you died a total of {Globals.DeathCount} times.", HorizontalAlignment.Center);
        // CreateTextEntity(Dimensions.GAME_W / 2, 140, 12, Fonts.PixeltypeID, "press Escape to close the game.", HorizontalAlignment.Center);
    }
    public Entity CreateTextEntity(int x, int y, int size, FontID fontID, string text, HorizontalAlignment horizontalAlignment=HorizontalAlignment.Left){
        var entity = CreateEntity();
        Set(entity, new DestroyOnLoad());
        Set(entity, new Position(x, y));
        Set(entity, new Text(fontID, size, TextStorage.GetID(text), horizontalAlignment));
        return entity;
    }
    public bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(Has<T>(source)) {
            Set(target, Get<T>(source));
            return true;
        }
        return false;
    }
    public Entity CreateScreenFade(float startAlpha, float endAlpha, float time) {
        var entity = BaseScreenFade(startAlpha);
        Set(entity, new LerpAlpha(startAlpha, endAlpha, time, 0));
        return entity;
    }
    public Entity CreateOffsetScreenFade(float startAlpha, float endAlpha, float offsetTime, float fadeTime) {
        var entity = BaseScreenFade(startAlpha);
        Set(entity, new AddAfterTime<LerpAlpha>(offsetTime, new LerpAlpha(startAlpha, endAlpha, fadeTime, 0)));
        return entity;
    }
    private Entity BaseScreenFade(float startAlpha) {
        var entity = CreateEntity();
        Set(entity, new DestroyOnLoad());
        Set(entity, new Position());
        Set(entity, new Rectangle(800, 600));
        Set(entity, new FollowCameraWithOffset(0, 0));
        Set(entity, new DrawAsRectangle());
        Set(entity, new Depth(0.01f));
        Set(entity, new ColorBlend(new Color(0, 0, 0, startAlpha)));
        // Console.WriteLine($"set color blend: {}")
        return entity;
    }
    public void AddEnemyHitbox(Entity entity, int width, int length, EffectorFlags extraFlags = EffectorFlags.None) {
        Set(entity, new Rectangle(width, length));
    }
    // public Entity CreateLoadSceneMessage(int levelID)
    // {
    //     var entity = CreateEntity();
    //     Set(entity, new DestroyAtEndOfFrame());
    //     Set(entity, new ChangeLevel(levelID));
    //     return entity;
    // }
    
    public EntityManipulator(World world) : base(world)
    {
    }

}