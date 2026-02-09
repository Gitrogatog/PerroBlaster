using System;
using System.Dynamic;
using System.Numerics;
using ImGuiNET;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MyGame;
using MyGame.Components;
using MyGame.Spawn;


public static class ImguiController
{
    static Window Window;
    static World World;
    static Inputs Inputs;
    public static void Init(Window window, World world, Inputs inputs)
    {
        Window = window;
        World = world;
        Inputs = inputs;
    }
    static int input0 = 16;
    static int input1 = 0;
    public static void UpdateOrClear(float delta, bool clear) {
        if(!clear){
            Update(delta);
        }
        else{
            Clear();
        }
    }
    public static void Update(float delta)
    {
        var io = ImGui.GetIO();

        io.AddMousePosEvent(Inputs.Mouse.X, Inputs.Mouse.Y);
        io.AddMouseButtonEvent(0, Inputs.Mouse.LeftButton.IsDown);
        io.AddMouseButtonEvent(1, Inputs.Mouse.RightButton.IsDown);
        io.AddMouseButtonEvent(2, Inputs.Mouse.MiddleButton.IsDown);
        io.AddMouseWheelEvent(0f, Inputs.Mouse.Wheel);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(24f / 255f, 18f / 255f, 18f / 255f, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
        ImGui.NewFrame();

        ImGui.SetNextWindowSize(new Vector2(Window.Width, Window.Height));
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(0, 0));
        ImGui.Begin("Main", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);


        ImGui.Text("hello world");
        ImGui.Spacing();
        ImGui.Text("goodbye!");

        if (ImGui.Button("Click Me!"))
        {
            ImGui.Text("I got clicked!");
            Console.WriteLine("I got clicked!");
        }
        if (ImGui.InputInt("text size", ref input0) && World.Some<Text>())
        {
            var entity = World.GetSingletonEntity<Text>();
            var text = World.Get<Text>(entity);
            World.Set(entity, new Text(text.FontID, input0, text.TextID, text.HorizontalAlignment, text.VerticalAlignment));

        }

        if (ImGui.InputInt("level", ref input1))
        {
            // var entity = World.GetSingletonEntity<DisplayCharCount>();
            // World.Set(entity, new DisplayCharCount(input1));
            // Console.WriteLine("did updat display char count");
            EntityPrefabs.ChangeLevel(input1);
        }

        // ImGui.PopStyleVar(5);
        // ImGui.PopStyleColor(14);
        ImGui.End();
        ImGui.EndFrame();
    }
    public static void Clear(){
        ImGui.NewFrame();
        ImGui.End();
        ImGui.EndFrame();
    }
}