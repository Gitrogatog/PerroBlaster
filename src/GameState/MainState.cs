using System;
using System.Diagnostics;
using System.IO;
// using System.Drawing;
using System.Numerics;
using ImGuiNET;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Audio;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MyGame;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Spawn;
using MyGame.Systems;



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
    ImguiRenderer imguiRenderer;
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
    LoadLevelJSON loadLevelJSON;
    int currentLevel = 0;
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
        updateGroup = new SystemGroup(World)
            .Add(new TimerSystem(World))
            .Add(new Input(World, Game.Inputs, Game.MainWindow))
            .Add(new PlayerController(World))
            .Add(new ProximityToPlayerSystem(World))
            .Add(new Motion(World))
            .Add(new Collision(World))
            .Add(new ThrowSystem(World))
            .Add(new OffsetSystem(World))
            .Add(new UpdateSpriteAnimationSystem(World))
            .Add(new AdvanceCharCountSystem(World))
            .Add(new PlayerDeathSystem(World))
            .Add(new Audio(World, Game.AudioDevice))
            .Add(new DestroySystem<DestroyAtEndOfFrame>(World))
        ;
        loadSystem = new LoadSceneSystem(World);


        // Input = new Input(World, Game.Inputs, Game.MainWindow);
        // PlayerExpandUI = new PlayerExpandUI(World);
        // Motion = new Motion(World, Game.Inputs);
        // Collision = new Collision(World);
        // DisplayMoveListSystem = new DisplayMoveListSystem(World);
        // UpdateUIContainer = new UpdateUIContainer(World);
        // PlayerMoveInput = new PlayerMoveInput(World);

        // ImGui.CreateContext();

        Renderer = new MyGame.Renderer(World, Game.GraphicsDevice, Game.RootTitleStorage, Game.MainWindow, Game.MainWindow.SwapchainFormat);
        ImguiController.Init(Game.MainWindow, World, Game.Inputs);
        loadLevelJSON = new LoadLevelJSON(World, new Vector2I(TileConsts.TILE_SIZE), TileConsts.TILE_MULT);
        StartGame();
        ImguiController.Update(0);

        Console.WriteLine("entered MainState!");

        // var player = EntityPrefabs.CreatePlayer(100, 100);
        // var rectangle = World.CreateEntity();
        // World.Set(rectangle, new Position(200, Dimensions.GAME_H));
        // World.Set(rectangle, new Rectangle(Dimensions.GAME_W, 100));
        // World.Set(rectangle, new DrawAsRectangle());
        // World.Set(rectangle, new Solid());
    }

    void Reload()
    {
        loadSystem.Update(default);
        StartGame();
    }
    void RestartCurrentLevel()
    {
        ChangeLevel(currentLevel);
        // var textParent = EntityPrefabs.CreateSF2SpriteText("KKKK", 100, 100, 0, 0, true, true, 3);
    }

    void StartGame()
    {
        loadLevelJSON.ReadFile("ContentStatic/Data/levels.json");
        ChangeLevel(0);
        // loadLevelJSON.ReadLevel(0);
        // currentLevel = 0;
    }
    void ChangeLevel(int level)
    {
        GlobalState.ShouldExistPlayer = true;
        loadSystem.Update(default);
        loadLevelJSON.ReadLevel(level);
        currentLevel = level;
    }

    void ImguiUpdate(float dt)
    {
        var io = ImGui.GetIO();
        if (io.WantCaptureKeyboard)
        {
            Game.MainWindow.StartTextInput();
        }
        else if (!io.WantCaptureKeyboard)
        {
            Game.MainWindow.StopTextInput();
        }
        io.AddMousePosEvent(Game.Inputs.Mouse.X, Game.Inputs.Mouse.Y);
        io.AddMouseButtonEvent(0, Game.Inputs.Mouse.LeftButton.IsDown);
        io.AddMouseButtonEvent(1, Game.Inputs.Mouse.RightButton.IsDown);
        io.AddMouseButtonEvent(2, Game.Inputs.Mouse.MiddleButton.IsDown);
        io.AddMouseWheelEvent(0f, Game.Inputs.Mouse.Wheel);

        ImGui.NewFrame();

        ImGui.SetNextWindowSize(io.DisplaySize);
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.Begin("Main", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);

        ImGui.End();
        ImGui.EndFrame();
    }

    public override void Update(TimeSpan delta)
    {
        long startTime = Stopwatch.GetTimestamp();
        if (World.Some<ChangeLevel>())
        {
            int level = World.GetSingleton<ChangeLevel>().LevelID;
            ChangeLevel(level);
        }
        else if (World.Some<ShouldPerformReset>())
        {
            RestartCurrentLevel();
        }
        else if (World.Some<DisplayDeathScreen>())
        {
            Console.WriteLine("display death screen!");
            var cause = World.GetSingleton<DisplayDeathScreen>().CauseOfDeath;
            loadSystem.Update(default);
            EntityPrefabs.CreateDeathScreen(cause);
        }
        else if (World.Some<DeathScreen>() && GlobalInput.Current.Shoot.IsPressed)
        {
            RestartCurrentLevel();
        }

        updateGroup.Update(delta);
        if (GlobalInput.Current.Reload.IsPressed)
        {
            Reload();
        }
        else if (GlobalInput.Current.Refresh.IsPressed)
        {
            RestartCurrentLevel();
        }
        ImguiController.Update((float)delta.TotalSeconds);
        double elapsed = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
        // Console.WriteLine();
        // Console.Write($"\nupdate took {elapsed} seconds!");
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