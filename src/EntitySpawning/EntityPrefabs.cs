
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
    public static Entity CreatePlayer(int x, int y) => manipulator.CreatePlayer(x, y);
    public static Entity CreateTile(int x, int y, Sprite sprite, float depth) => manipulator.CreateTile(x, y, sprite, depth);
    public static Entity CreateAnimatedTile(int x, int y, SpriteAnimationInfo sprite, float depth) => manipulator.CreateAnimatedTile(x, y, new SpriteAnimation(sprite), depth);
    public static Entity AddSolidCollision(Entity entity, Rectangle rect) => manipulator.AddSolidCollision(entity, rect);
    public static void AddSolidTileCollision(Entity entity, int x, int y) => manipulator.AddSolidTileCollision(entity, x, y);
    public static Entity CreateEntityOnTileGrid(int x, int y) => manipulator.CreateEntityOnTileGrid(x, y);
    public static Entity CreateTextbox(int textId) => manipulator.CreateTextbox(textId);
    public static Entity CreateDialogText(int textId, int x, int y) {
        var entity = manipulator.CreateText(10, y - 20, 12, Fonts.RM2000AltID, textId);
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
    public static void ChangeSceneFadeout(GameSceneType gameScene) {
        ScreenFadeToBlack(0.5f);
        CreateTimedMessage(new ChangeGameScene(gameScene), 0.5f);
        CreatePreventInputEntity();
        // Console.WriteLine("faindig out!");
        // Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void ChangeLevelFadeout(int levelId, int entityUUID) {
        ScreenFadeToBlack(2f);
        CreateTimedMessage(new ChangeLevel(levelId, entityUUID), 2f);
        CreatePreventInputEntity();
        Console.WriteLine("faindig out!");
        Console.WriteLine($"levelId:{levelId} entityuuid:{entityUUID}");
    }
    public static void EnterLevelFadein() {
        ScreenFadeToClear(2f);
        World.Set(CreatePreventInputEntity(), new Timer(2f));
    }
    private static Entity CreatePreventInputEntity() {
        var entity = World.CreateEntity();
        World.Set(entity, new PreventInput());
        World.Set(entity, new DestroyOnLoad());
        return entity;
    }
    public static Entity PlaySFX(StaticSoundID StaticSoundID,
        SoundCategory Category = SoundCategory.Generic,
        float Volume = 1,
        float Pitch = 0,
        float Pan = 0)
    {
        var entity = World.CreateEntity();
        World.Set(entity, new PlayStaticSFX(StaticSoundID, Category, Volume, Pitch, Pan));
        return entity;
    }
    public static bool Mirror<T>(Entity source, Entity target) where T : unmanaged {
        if(World.Has<T>(source)) {
            World.Set(target, World.Get<T>(source));
            return true;
        }
        return false;
    }
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
                Set(entity, new SpriteAnimation(SpriteAnimations.dennys_menu_init, loop:false));
                Set(entity, new Position(x, y));
                Set(entity, new DestroyOnAnimationFinish());
                Set(entity, new SpawnOnAnimationFinish(ThingType.StartMenu));
                Set(entity, new Depth(0.8f));
                return entity;
            }
            case ThingType.CloudMenuOpen: {
                var entity = CreateEntity();
                Set(entity, new SpriteAnimation(SpriteAnimations.start_menu_init, loop:false));
                Set(entity, new Position(x, y));
                // Set(entity, new DestroyOnAnimationFinish());
                // Set(entity, new SpawnOnAnimationFinish(ThingType.StartMenu));
                return entity;
            }
            case ThingType.StartMenu: {
                var textEntity = CreateEntity();
                Set(textEntity, new Position(x, y));
                Set(textEntity, new SpriteAnimation(SpriteAnimations.start_menu_text));
                Set(textEntity, new DestroyOnLoad());
                Set(textEntity, new Position(x, y));
                Set(textEntity, new Depth(0.5f));
                var backgroundEntity = CreateEntity();
                Set(backgroundEntity, new Position(x, y));
                Set(backgroundEntity, new SpriteAnimation(SpriteAnimations.start_menu_background));
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
                Set(selectHighlight, new SpriteAnimation(SpriteAnimations.ui_blink2));
                Set(selectHighlight, new Position(x + xOffset + selectX, y + yOffsetInit + selectY));
                Set(selectHighlight, new DestroyOnLoad());
                Set(selectHighlight, new SelectHighlight(selectX, selectY));
                Set(selectHighlight, new Depth(0.55f));
                return textEntity;
            }
        }
        return default;
    }

    public Entity CreatePlayer(int x, int y)
    {
        Entity entity = CreateEntityOnTileGrid(x, y);
        Set(entity, new ControlledByPlayer());
        Set(entity, new Rectangle(10, 10, EffectorFlags.CanTouchWall, EffectedFlags.CanTakeDamage));
        
        Set(entity, new FourDirectionAnim(SpriteAnimations.daisy_up, SpriteAnimations.daisy_down, SpriteAnimations.daisy_left, SpriteAnimations.daisy_right));
        var anim = Some<LastPlayerData>() ? GetSingleton<LastPlayerData>().Animation.SpriteAnimationInfo : SpriteAnimations.daisy_down;
        EntityUtils.SetStandAnim(World, entity, anim);
        Set(entity, new DestroyOnLoad());
        Set(entity, new MoveSpeed(120));
        Set(entity, new CameraFollow());
        Set(entity, new FacingDirection(0, 1));
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
        Set(entity, new Position(Dimensions.GAME_W / 2, 200));
        // Set(entity, new GrowRectToSize(80, 6, 0));
        // Set(entity, new Rectangle(Dimensions.GAME_W, 12, EffectorFlags.None, EffectedFlags.None));
        // Set(entity, new NineSlice(SpriteAnimations.ui_nine_slice.Frames[0]));
        Set(entity, new IsDialogBox());
        Set(entity, new SpriteAnimation(SpriteAnimations.dialogbox, loop:false));
        Set(entity, new EnableAdvanceCharCount());
        Set(entity, new CreateDialogTextOnAnimFinish(textId));

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
        Set(entity, new WordWrap(300));
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
        var entity = CreateEntity();
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
    public void AddSolidTileCollision(Entity entity, int x, int y) {
        Set(entity, new Solid());
        AddEntityToTile(entity, x, y);
    }
    public Entity AddSolidCollision(Entity entity, Rectangle rect)
    {
        Set(entity, new Solid());
        Set(entity, new CanInteract());
        Set(entity, rect);
        return entity;
    }
    public void CreateStartMenu(){
        // CreateTextEntity(60, 20, 8, Fonts.KosugiID, "BUBBA");
        // var subTitle = CreateTextEntity(70, 65, 12, Fonts.PixeltypeID, "AXE");
        var title = CreateEntity();
        Set(title, new DestroyOnLoad());
        Set(title, new Position(Dimensions.GAME_W / 2, Dimensions.GAME_H / 2));
        Set(title, new SpriteAnimation(SpriteAnimations.dennys_start));
        Set(title, new Depth(1f));
        EntityPrefabs.ScreenStayBlackThenClear(1f, 0.5f);
        int x = 128 + 64 / 2;
        int y = 148 + 64 / 2;
        var timer1 = CreateEntity();
        Set(timer1, new Timer(1.5f));
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
        Set(entity, new Rectangle(0, 0, Dimensions.GAME_W, Dimensions.GAME_H, EffectorFlags.None, EffectedFlags.None));
        Set(entity, new FollowCameraWithOffset(0, 0));
        Set(entity, new DrawAsRectangle());
        Set(entity, new Depth(0.01f));
        Set(entity, new ColorBlend(new Color(0, 0, 0, startAlpha)));
        return entity;
    }
    public void AddEnemyHitbox(Entity entity, int width, int length, EffectorFlags extraFlags = EffectorFlags.None) {
        Set(entity, new Rectangle(width, length, EffectorFlags.CanTouchWall | extraFlags, EffectedFlags.CanTakeDamage));
    }
    // public Entity CreateLoadSceneMessage(int levelID)
    // {
    //     var entity = CreateEntity();
    //     Set(entity, new DestroyAtEndOfFrame());
    //     Set(entity, new ChangeLevel(levelID));
    //     return entity;
    // }
    public Entity CreateTimedMessage<T>(T component, float time) where T : unmanaged
    {
        var entity = CreateEntity();
        Set(entity, new AddAfterTime<T>(time, component));
        return entity;
    }
    public Entity CreateSpriteText(string input, SpriteAnimationInfo info, int textSizeX, int textSizeY, int separationX, int separationY, int worldX, int worldY, int scale, bool centeredX, bool centeredY, bool dontDraw = false)
    {
        Sprite sprite = info.Frames[0];
        var parent = CreateEntity();
        Set(parent, new DestroyOnLoad());
        Set(parent, new TextSpriteParent());

        if (dontDraw)
        {
            // Set(parent, new AdvanceCharSpeed(0.06f));
            Set(parent, new AdvanceCharCount(1f, 1f));
        }
        int x = 0, y = 0, maxLength = 0;
        int deltaX = separationX + textSizeX * scale;
        int deltaY = separationY + textSizeY * scale;
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '\n')
            {
                x += deltaX;
                var blank = CreateEntity();
                Set(blank, new DestroyOnLoad());
                Set(blank, new Position(worldX + x, worldY + y));
                Relate(parent, blank, new Child());
                Relate(parent, blank, new DontDraw());

                maxLength = Math.Max(x, maxLength);
                y += deltaY;
                x = 0;
                continue;
            }
            if (c == ' ')
            {
                x += deltaX;
                var blank = CreateEntity();
                Set(blank, new DestroyOnLoad());
                Set(blank, new Position(worldX + x, worldY + y));
                Relate(parent, blank, new Child());
                Relate(parent, blank, new DontDraw());
                continue;
            }
            int offset = TextUtils.CharToSF2TextPos(c);
            if (offset < 0)
            {
                continue;
            }
            Sprite textSprite = sprite.Slice(offset * textSizeX, 0, textSizeX, textSizeY);
            Position pos = new Position(worldX + x, worldY + y);
            var textEntity = CreateEntity();
            Set(textEntity, textSprite);
            Set(textEntity, pos);
            Set(textEntity, new DestroyOnLoad());
            Set(textEntity, new Depth(0.01f));
            Relate(parent, textEntity, new Child());
            if (dontDraw)
            {
                Relate(parent, textEntity, new DontDraw());
            }
            if (scale != 1)
            {
                Set(textEntity, new SpriteScale(scale));
            }
            x += deltaX;
        }
        if (centeredX || centeredY)
        {
            if (centeredX)
            {
                x -= deltaX;
                maxLength = Math.Max(x, maxLength) / 2;
            }
            if (centeredY)
            {
                y /= 2;
            }
            foreach (var child in OutRelations<Child>(parent))
            {
                var pos = Get<Position>(child);
                if (centeredX)
                {
                    pos = pos.SetX(pos.X - maxLength);
                }
                if (centeredY)
                {
                    pos = pos.SetY(pos.Y - y);
                }
                Set(child, pos);
            }
        }
        return parent;

    }

    public EntityManipulator(World world) : base(world)
    {
    }

}