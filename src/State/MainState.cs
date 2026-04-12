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
    GameSceneType gameScene = GameSceneType.Level;
    bool isFullscreen = false;
    Input2 inputSystem;
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
        inputSystem = new Input2(World, Game.Inputs);
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
            .Add(new PlayerController(World))
            // .Add(new UpdateRotationSystem(World))
            // .Add(new EnemyBehaviorSystem(World))
            // .Add(new TileMotion(World))
            .Add(new FacingSystem(World))
            .Add(new MotionWithFlags(World))
            .Add(new CollisionSystem(World))
            .Add(new PostCollision(World))
            // .Add(new FourDirectionAnimSystem(World))
            // .Add(new TileCollision(World))

            // .Add(new GrowUIBoxSystem(World))
            // .Add(new UIOptionSystem(World))
            // .Add(new DialogSystem(World))
            // .Add(new FakeBattleSystem(World))
            .Add(new AnimationSystem(World))
            .Add(new SetSpriteAnimationSystem(World))
            .Add(new UpdateSpriteAnimationSystem(World))
            .Add(new UpdateRotationSystem(World))
            .Add(new MiscSpriteUpdateSystem(World))
            .Add(new CameraSystem(World))
            // .Add(new AdvanceCharCountSystem(World))
            // .Add(new FlickerSystem(World))


            .Add(videoSystem)
            .Add(audio)
            
            .Add(new LerpAlphaSystem(World))
            .Add(new DestroySystem<DestroyAtEndOfFrame>(World))
            .Add(new RemoveSystem<TookDamageLastFrame>(World))
            .Add(new RemoveSystem<AttemptJumpThisFrame>(World))
            .Add(new RemoveSystem<AttemptTalkThisFrame>(World))
            .Add(new ReplaceSystem<TookDamageThisFrame, TookDamageLastFrame>(World));
        ;
        postLoadGroup = new SystemGroup(World)
            // .Add(new EnterLevelSystem(World))
            ;
        loadSystem = new LoadSceneSystem(World);
        gameScene = GameSceneType.Level;

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
        loadLevelJSON = new LoadLevelJSON(World, new Vector2I(Dimensions.TILE_SIZE), 1);
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
        switch(gameScene) {
            case GameSceneType.Level: {
                StartGame(0);
                // var rectentity = EntityPrefabs.CreateTestHitbox(40, 40);
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
    }
    void StartGame(int level)
    {
        // #if DEBUG
        //     loadLevelJSON.ReadFile(Path.Combine("~", "Documents", "Moonworks", "projects", "PerroBlaster", "ContentStatic", "Data", "perro_levels.ldtk"));
        // #else
        //     loadLevelJSON.ReadFile(Path.Combine("ContentStatic", "Data", "perro_levels.ldtk"));
        // #endif
        loadLevelJSON.ReadFile(Path.Combine("ContentStatic", "Data", "perro_levels.ldtk"));
        ChangeLevel(level);
    }
    void StartGame()
    {
        StartGame(0);
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
        inputSystem.Update(delta);
        if(GlobalInput.Current.Cancel.IsPressed) {
            updateGroup.Update(delta);
        }
        

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