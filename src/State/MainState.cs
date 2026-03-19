using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Xml;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Video;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
using MyGame.Systems;
using MyGame.Utility;
using RollAndCash.Systems;
namespace MyGame;

public class MainState : GameState
{
    CyclesGame Game;
    GraphicsDevice GraphicsDevice;
    GameState TransitionState;

    SpriteBatch HiResSpriteBatch;
    Sampler LinearSampler;

    Texture RenderTexture;
    AudioDevice AudioDevice;
    PersistentVoice MusicVoice;
    AudioDataQoa Music;

    MyGame.Renderer Renderer;
    // ImguiRenderer imguiRenderer;
    World World;
    // Input Input;
    // Motion Motion;
    // Collision Collision;
    // UpdateUIContainer UpdateUIContainer;
    // DisplayMoveListSystem DisplayMoveListSystem;
    // PlayerMoveInput PlayerMoveInput;
    // PlayerExpandUI PlayerExpandUI;
    SystemGroup updateGroup;
    LoadSceneSystem loadSystem;
    SystemGroup postLoadGroup;
    LoadLevelJSON loadLevelJSON;
    Audio audio;
    VideoSystem videoSystem;
    int currentLevel = 0;
    static uint windowSize = 3;
    static bool hideImgui => true;
    GameSceneType gameScene;
    bool isFullscreen = false;
    // Input Input;

    public MainState(CyclesGame game, GameState transitionState = null)
    {

        // AudioDevice = game.AudioDevice;
        Game = game;
        // GraphicsDevice = game.GraphicsDevice;
        TransitionState = transitionState;


        // LinearSampler = Sampler.Create(GraphicsDevice, SamplerCreateInfo.LinearClamp);
        // HiResSpriteBatch = new SpriteBatch(GraphicsDevice, Game.RootTitleStorage, game.MainWindow.SwapchainFormat);
        // RenderTexture = Texture.Create2D(GraphicsDevice, Dimensions.GAME_W, Dimensions.GAME_H, game.MainWindow.SwapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler);
    }
    public override void Start()
    {
        World = new World();
        EntityPrefabs.Init(World);
        audio = new Audio(World, Game.AudioDevice);
        videoSystem = new VideoSystem(World, Game);
        updateGroup = new SystemGroup(World)
            .Add(new DestroySystem<DestroyAtStartOfFrame>(World))
            .Add(new TimedComponentSystem<ColorOverlayTimer>(World))
            .Add(new TimedComponentSystem<CantShootTimer>(World))
            .Add(new TimedComponentSystem<CantMoveTimer>(World))
            .Add(new AddAfterTimeSystem<PlayStaticSFX>(World, true))
            .Add(new AddAfterTimeSystem<PlayMusic>(World))
            .Add(new AddAfterTimeSystem<StopAllMusic>(World))
            .Add(new AddAfterTimeSystem<ChangeLevel>(World))
            .Add(new AddAfterTimeSystem<ChangeGameScene>(World))
            .Add(new AddAfterTimeSystem<AdvanceCharCount>(World))
            .Add(new AddAfterTimeSystem<LerpAlpha>(World))
            .Add(new AddAfterTimeSystem<PisonPlayAnim>(World))
            .Add(new AddAfterTimeSystem<SetDaisyColorOverlay>(World))
            .Add(new AddAfterTimeSystem<AccelerateDir>(World))
            .Add(new AddAfterTimeSystem<RotateSpeed>(World))
            .Add(new AddAfterTimeSystem<LoadVideo>(World))
            .Add(new AddAfterTimeSystem<PlayVideo>(World))
            .Add(new AddAfterTimeSystem<CloseGameWindow>(World))
            .Add(new AddAfterTimeSystem<DisplayDialog>(World))
            .Add(new AddAfterTimeSystem<SpriteAnimation>(World))
            .Add(new AddAfterTimeSystem<PlayInteractSounds>(World))
            .Add(new AddAfterTimeSystem<DoNotPlayInteractSounds>(World))
            .Add(new LerpSystem<SpriteScale>(World, static x => new SpriteScale(x)))
            .Add(new LerpSystem<Rotation>(World, static x => new Rotation(x)))
            .Add(new TimerSystem(World))
            .Add(new Input2(World, Game.Inputs))
            .Add(new PlayerController(World))
            // .Add(new UpdateRotationSystem(World))
            // .Add(new EnemyBehaviorSystem(World))
            .Add(new TileMotion(World))
            .Add(new Motion(World))
            .Add(new CollisionSystem(World))
            .Add(new PostCollision(World))
            .Add(new FourDirectionAnimSystem(World))
            .Add(new TileCollision(World))

            .Add(new GrowUIBoxSystem(World))
            .Add(new UIOptionSystem(World))
            .Add(new DialogSystem(World))
            .Add(new FakeBattleSystem(World))
            .Add(new AnimationSystem(World))
            .Add(new SetSpriteAnimationSystem(World))
            .Add(new UpdateSpriteAnimationSystem(World))
            .Add(new UpdateRotationSystem(World))
            .Add(new MiscSpriteUpdateSystem(World))
            .Add(new CameraSystem(World))
            .Add(new AdvanceCharCountSystem(World))
            .Add(new FlickerSystem(World))


            .Add(videoSystem)
            .Add(audio)
            
            .Add(new LerpAlphaSystem(World))
            .Add(new DestroySystem<DestroyAtEndOfFrame>(World))
            .Add(new RemoveSystem<TookDamageLastFrame>(World))
            .Add(new RemoveSystem<FinishStepThisFrame>(World))
            .Add(new RemoveSystem<AttemptTalkThisFrame>(World))
            .Add(new ReplaceSystem<TookDamageThisFrame, TookDamageLastFrame>(World));
        ;
        postLoadGroup = new SystemGroup(World).Add(new EnterLevelSystem(World));
        loadSystem = new LoadSceneSystem(World);
        gameScene = GameSceneType.StartMenu;

        // Input = new Input(World, Game.Inputs, Game.MainWindow);
        // PlayerExpandUI = new PlayerExpandUI(World);
        // Motion = new Motion(World, Game.Inputs);
        // Collision = new Collision(World);
        // DisplayMoveListSystem = new DisplayMoveListSystem(World);
        // UpdateUIContainer = new UpdateUIContainer(World);
        // PlayerMoveInput = new PlayerMoveInput(World);

        // ImGui.CreateContext();

        Renderer = new MyGame.Renderer(World, Game.GraphicsDevice, Game.RootTitleStorage, Game.MainWindow, Game.MainWindow.SwapchainFormat);
        // ImguiController.Init(Game.MainWindow, World, Game.Inputs);
        loadLevelJSON = new LoadLevelJSON(World, new Vector2I(TileConsts.TILE_SIZE), TileConsts.TILE_MULT);
        //d StartGame();
        ChangeGameScene(gameScene);
        // ImguiController.UpdateOrClear(0, hideImgui);

        Console.WriteLine("entered MainState!");
        // ChangeWindowSize();

        // var player = EntityPrefabs.CreatePlayer(100, 100);
        // var rectangle = World.CreateEntity();
        // World.Set(rectangle, new Position(200, Dimensions.GAME_H));
        // World.Set(rectangle, new Rectangle(Dimensions.GAME_W, 100));
        // World.Set(rectangle, new DrawAsRectangle());
        // World.Set(rectangle, new Solid());
    }

    void ChangeGameScene(GameSceneType scene){
        Globals.CameraX = 0;
        Globals.CameraY = 0;
        loadSystem.Update(default);
        while(World.Some<DestroyOnSceneChange>()){
            World.Destroy(World.GetSingletonEntity<DestroyOnSceneChange>());
        }
        gameScene = scene;
        switch(scene){
            case GameSceneType.Intro: {
                float startBlack = 0.5f;
                float fadeIn = 8f / 60f;
                float stayClear = 8f / 6f;
                float fadeOut = 8f / 60f;
                EntityPrefabs.CreateVisual(Dimensions.GAME_W / 2, Dimensions.GAME_H / 2, SpriteAnimations.game_studio);
                EntityPrefabs.ScreenStayBlackThenClear(startBlack, fadeIn);
                EntityPrefabs.ChangeSceneFadeoutDelay(GameSceneType.StartMenu, stayClear + startBlack + fadeIn, fadeOut);
                break;
            }
            case GameSceneType.StartMenu:{
                float startBlack = 0f;
                float fadeIn = 0.5f;
                // EntityPrefabs.CreateMessageEndOfFrame(new StartMusicAndSetVolume(1f, StreamingAudio.rm_opening1));
                EntityPrefabs.CreateTimedMessage(new PlayMusic(StreamingAudio.saga_opening), startBlack + fadeIn + 0.5f);
                EntityPrefabs.CreateStartMenu();
                // EntityPrefabs.CreateVisual(100, 50, SpriteAnimations.dialogbox, 0.05f);
                
                var title = World.CreateEntity();
                World.Set(title, new Position(Dimensions.GAME_W * 0.55f, Dimensions.GAME_H * 0.42f));
                World.Set(title, new SpriteScale(0.8f));
                World.Set(title, new DestroyOnLoad());
                World.Set(title, new SpriteAnimation(SpriteAnimations.saga_title));

                // Globals.CameraX = 1000;
                // Globals.CameraY = 1000;

                // var rect = World.CreateEntity();
                // World.Set(rect, new Position(Dimensions.GAME_W / 2, 0));
                // World.Set(rect, new Rectangle(0, 10, 100, 150, EffectorFlags.None, EffectedFlags.None));
                // World.Set(rect, new DrawAsRectangle());
                // var text = EntityPrefabs.CreateDialogText(10, 140, 12, Fonts.RM2000AltID, "hello there\nhello there\n\nhello there\nhello there\n\n\n\n hello there");
                // int textId = TextStorage.GetID("It's me, Pison. I have returned from the Dark World and become Pisons.");
                // var text = EntityPrefabs.CreateDialogText(textId, 8, 140);
                // var pison = World.CreateEntity();
                // World.Set(title, new Position(Dimensions.GAME_W * 0.65f, Dimensions.GAME_H * 0.6f));
                // World.Set(title, new DestroyOnLoad());
                // World.Set(title, new SpriteAnimation(SpriteAnimations.pison_attack));


                // var flickerArrow = World.CreateEntity();
                // World.Set(flickerArrow, new Position(Dimensions.GAME_W / 2, Dimensions.GAME_H - 4));
                // World.Set(flickerArrow, new SpriteAnimation(SpriteAnimations.ui_arrow_down));
                // EntityPrefabs.CreateText(60, 60, 12, Fonts.RM2000AltID, "WOAHHHHH");
                // EntityPrefabs.CreateDialogText(TextStorage.GetID("dialog text"), 10, 10);
                // var e1 = World.CreateEntity();
                // World.Set(e1, new DestroyOnLoad());
                // World.Set(e1, new SpriteAnimation(SpriteAnimations.daisy_down));
                // World.Set(e1, SpriteAnimations.daisy_down.Frames[0]);
                // World.Set(e1, new Position(100, 100));
                // World.Set(e1, new Depth(0.4f));
                // World.Set(e1, new ColorBlend(new Color(1, 1, 1, 0.5f)));
                // var e2 = World.CreateEntity();
                // World.Set(e2, new DestroyOnLoad());
                // World.Set(e2, new SpriteAnimation(SpriteAnimations.water));
                // World.Set(e2, new Position(106, 100));
                // World.Set(e2, new Depth(0.5f));
                break;
            }
            case GameSceneType.Level:{
                StartGame();
                // EntityPrefabs.CreateMessage(new StartMusicAndSetVolume(2f, 1 ));
                // EntityPrefabs.CreateMessageEndOfFrame(new PlayMusic(StreamingAudio.rm_opening1));
                var clearColorEntity = World.CreateEntity();
                // World.Set(clearColorEntity, new ClearColor(new Color(243, 205, 172)));
                // World.Set(clearColorEntity, new ClearColor(new Color(0, 0, 0, 255))); // new Color(71, 50, 75)
                World.Set(clearColorEntity, new DestroyOnSceneChange());
                var testTextEntity = World.CreateEntity();
                World.Set(testTextEntity, new Position(10, 176));
                // World.Set(testTextEntity, new SpriteAnimation(SpriteAnimations.start_menu_text));
                break;
            }
            case GameSceneType.GameOver: {
                World.Set(World.CreateEntity(), new StopAllMusic());
                // World.Set(World.CreateEntity(), new PlayStaticSFX(StaticAudio.game_over));
                EntityPrefabs.CreateTimedMessage(new PlayStaticSFX(StaticAudio.game_over), 0.1f);
                EntityPrefabs.CreateVisual(Dimensions.GAME_W / 2, Dimensions.GAME_H / 2, SpriteAnimations.game_over);
                EntityPrefabs.ScreenFadeToClear(1f);
                EntityPrefabs.ChangeSceneFadeoutDelay(GameSceneType.DarkWorld, 5f, 1f);

                break;
            }
            case GameSceneType.DarkWorld: {
                StartGame(6);
                var background = EntityPrefabs.CreateVisual(Dimensions.GAME_W, Dimensions.GAME_H, SpriteAnimations.creepy_background, 0.95f);
                World.Set(background, new RotateSpeed(0.2f * MathF.PI));
                EntityPrefabs.CreateTimedMessage(new PlayMusic(StreamingAudio.dark_world_voice), 2f);
                int textID = TextStorage.GetID("Welcome to the Dark World");
                EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W / 4, Dimensions.GAME_H * 3 / 4, textID, 0.5f, 3.4f, new Color(189, 223, 255));
                float timeOffset = 0.5f;
                for(int i = 0; i < 24; i++) {
                    int x = Rando.Int(-40, 290);
                    int y = Rando.Int(-4, Dimensions.GAME_H + 4);
                    float speed = Rando.Range(2f, 13f);
                    EntityPrefabs.CreateTextWithDelay(x, y, textID, timeOffset, speed, new Color(189, 223, 255));
                    timeOffset += Rando.Range(0.5f, 1.5f);
                }
                EntityPrefabs.ChangeSceneFadeoutDelay(GameSceneType.EndMenu, timeOffset, 2f);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W / 4 - 14, Dimensions.GAME_H / 2, textID, 1.7f, 6, Color.Green);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W * 3 / 4 + 10, 210, textID, 2f, 4, Color.Green);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W * 3 / 4 + 10, 210, textID, 2.5f, 2, Color.Green);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W * 3 / 4 + 10, 210, textID, 3f, 8, Color.Green);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W * 3 / 4 + 10, 210, textID, 3.1f, 12, Color.Green);
                // EntityPrefabs.CreateTextWithDelay(Dimensions.GAME_W * 3 / 4 + 10, 210, textID, 3.4f, 5, Color.Green);
                // EntityPrefabs.CreateText(Dimensions.GAME_W / 4, Dimensions.GAME_H * 3 / 4, 12, Fonts.RM2000AltID, )

                break;
            }
            case GameSceneType.EndMenu:{
                // for(int i = 0; i <= 2; i++){
                //     EntityPrefabs.CreateMessageEndOfFrame(new StopMusic(i));
                // }
                EntityPrefabs.ScreenStayBlackThenClear(6f, 0.5f);
                EntityPrefabs.CreateThing(ThingType.CloudMenuOpen, Dimensions.GAME_W / 2, Dimensions.GAME_H / 2);
                // EntityPrefabs.CreateThing()
                World.Set(World.CreateEntity(), new StopAllMusic());
                EntityPrefabs.CreateTimedMessage(new PlayMusic(StreamingAudio.rm_open01), 6.5f);
                EntityPrefabs.CreateTimedMessage(new PlayInteractSounds(), 6.8f);

                break;
            }
            case GameSceneType.VideoTest: {
                World.Set(World.CreateEntity(), new LoadVideo());
                EntityPrefabs.CreateTimedMessage(new PlayVideo(), 3f);
                break;
            }
        }
    }

    void Reload()
    {
        loadSystem.Update(default);
        StartGame();
    }
    void ReloadCurrentLevel()
    {
        StartGame(currentLevel);
    }
    void RestartCurrentLevel()
    {
        ChangeLevel(currentLevel);
        // var textParent = EntityPrefabs.CreateSF2SpriteText("KKKK", 100, 100, 0, 0, true, true, 3);
    }
    void StartGame(int level)
    {
        loadLevelJSON.ReadFile(Path.Combine("ContentStatic", "Data", "cookie_levels.json"));
        ChangeLevel(level);
    }
    void StartGame()
    {
        loadLevelJSON.ReadFile(Path.Combine("ContentStatic", "Data", "cookie_levels.json"));
        ChangeLevel(3);
        // loadLevelJSON.ReadLevel(0);
        // currentLevel = 0;
    }
    void ChangeLevel(int level)
    {
        Globals.ShouldExistPlayer = true;
        loadSystem.Update(default);
        loadLevelJSON.ReadLevel(level);
        Globals.CurrentRoomX = -1000;
        Globals.CurrentRoomY = -1000;
        currentLevel = level;
        postLoadGroup.Update(default);
    }

    public override void Update(TimeSpan delta)
    {
        long startTime = Stopwatch.GetTimestamp();
        Globals.CurrentTime += delta.TotalSeconds;

        if (World.Some<ChangeLevel>())
        {
            int level = World.GetSingleton<ChangeLevel>().LevelID;
            Console.WriteLine($"main chaning level: {level}");
            ChangeLevel(level);
        }
        else if (World.Some<ShouldPerformReset>())
        {
            RestartCurrentLevel();
        }

        #if DEBUG
            if (GlobalInput.Current.Reload.IsPressed)
            {
                Console.WriteLine("Reloading!");
                ReloadCurrentLevel();
            }
            else if (GlobalInput.Current.Refresh.IsPressed)
            {
                // RestartCurrentLevel();
                // ChangeGameScene(gameScene);
                World.Dispose();
                Renderer.Dipose();
                audio.Dispose();
                videoSystem.Dispose();
                Console.WriteLine("resetting world!");
                Start();
            }
#endif
        
        // if(GlobalInput.Current.Interact.IsPressed && gameScene == GameSceneType.StartMenu){
        //     ChangeGameScene(GameSceneType.Level);
        // }
        else if(World.Some<ChangeGameScene>()){
            ChangeGameScene(World.GetSingleton<ChangeGameScene>().Scene);
            while(World.Some<ChangeGameScene>()){
                World.Destroy(World.GetSingletonEntity<ChangeGameScene>());
            }
        }
        if(World.Some<CloseGameWindow>()){
            Game.Quit();
        }
        if(gameScene == GameSceneType.EndMenu && World.Some<PlayInteractSounds>() && !World.Some<DoNotPlayInteractSounds>()) {
            if(GlobalInput.Current.Interact.IsPressed) {
                EntityPrefabs.PlaySFX(StaticAudio.Decision1);
            }
            if(GlobalInput.Current.Up.IsPressed || GlobalInput.Current.Down.IsPressed) {
                EntityPrefabs.PlaySFX(StaticAudio.Cursor1);
            }
        }
        updateGroup.Update(delta);

        // ImguiController.UpdateOrClear((float)delta.TotalSeconds, hideImgui);
        double elapsed = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
        if(GlobalInput.Current.Fullscreen.IsPressed) {
            isFullscreen = !isFullscreen;
            Game.MainWindow.SetScreenMode(isFullscreen ? ScreenMode.Fullscreen : ScreenMode.Windowed);
        }

        // if(GlobalInput.Current.WindowPlus.IsPressed && windowSize < 6){
        //     windowSize++;
        //     ChangeWindowSize();
        // }
        // else if(GlobalInput.Current.WindowMinus.IsPressed && windowSize > 1){
        //     windowSize--;
        //     ChangeWindowSize();
        // }
        // Console.WriteLine($"entity count: {World.Debug_GetEntities(typeof(Position)).Count()}");
        // Console.WriteLine($"exists global zombie: {World.Some<GlobalZombieMode>()}");
        // Console.WriteLine();
        // Console.Write($"\nupdate took {elapsed} seconds!");
    }
    void ChangeWindowSize(){
        Game.MainWindow.SetSize(Dimensions.GAME_W * windowSize, Dimensions.GAME_H * windowSize);
    }
    
    public override void Draw(Window window, double alpha)
    {
        long startTime = Stopwatch.GetTimestamp();
        Renderer.Render(Game.MainWindow);
        double elapsed = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
        // Console.Write($" draw took {elapsed} seconds!");
        // Console.WriteLine("drawing from ui!");
        // imguiRenderer.Draw(1);
    }
    public override void End()
    {
        World.Dispose();
    }
}