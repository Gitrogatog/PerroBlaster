using System;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Input;
using MyGame;
using MyGame;
using MyGame.Components;

namespace MyGame.Systems;

public struct InputState
{
    public ButtonState Left { get; set; }
    public ButtonState Right { get; set; }
    public ButtonState Up { get; set; }
    public ButtonState Down { get; set; }
    public ButtonState Jump { get; set; }
    public ButtonState Shoot { get; set; }
    public ButtonState Reload { get; set; }
    public ButtonState Refresh { get; set; }
}

public class ControlSet
{
    public VirtualButton Left { get; set; } = new EmptyButton();
    public VirtualButton Right { get; set; } = new EmptyButton();
    public VirtualButton Up { get; set; } = new EmptyButton();
    public VirtualButton Down { get; set; } = new EmptyButton();
    public VirtualButton Jump { get; set; } = new EmptyButton();
    public VirtualButton Shoot { get; set; } = new EmptyButton();
    public VirtualButton Reload { get; set; } = new EmptyButton();
    public VirtualButton Refresh { get; set; } = new EmptyButton();
}

public class Input : MoonTools.ECS.System
{
    Inputs Inputs { get; }

    Filter PlayerFilter { get; }
    Window Window;

    ControlSet PlayerOneKeyboard = new ControlSet();
    ControlSet PlayerOneGamepad = new ControlSet();
    // ControlSet PlayerTwoKeyboard = new ControlSet();
    // ControlSet PlayerTwoGamepad = new ControlSet();


    public Input(World world, Inputs inputs, Window window) : base(world)
    {
        Inputs = inputs;
        Window = window;
        Console.WriteLine($"Window: {Window.Width}, {Window.Height}");
        Console.WriteLine($"int: {(int)Window.Width}, {(int)Window.Height}");
        Console.WriteLine($"dimensions: {Dimensions.GAME_W},{Dimensions.GAME_H}");
        Console.WriteLine($"house divided: {(int)Window.Width / Dimensions.GAME_W},{(int)Window.Height / Dimensions.GAME_H}");

        PlayerFilter = FilterBuilder.Include<Player>().Build();


        PlayerOneKeyboard.Up = Inputs.Keyboard.Button(KeyCode.W);
        PlayerOneKeyboard.Down = Inputs.Keyboard.Button(KeyCode.S);
        PlayerOneKeyboard.Left = Inputs.Keyboard.Button(KeyCode.A);
        PlayerOneKeyboard.Right = Inputs.Keyboard.Button(KeyCode.D);
        PlayerOneKeyboard.Jump = Inputs.Keyboard.Button(KeyCode.Space);
        PlayerOneKeyboard.Shoot = Inputs.Keyboard.Button(KeyCode.J);
        PlayerOneKeyboard.Reload = Inputs.Keyboard.Button(KeyCode.T);
        PlayerOneKeyboard.Refresh = Inputs.Keyboard.Button(KeyCode.R);

        PlayerOneGamepad.Up = Inputs.GetGamepad(0).LeftYDown;
        PlayerOneGamepad.Down = Inputs.GetGamepad(0).LeftYUp;
        PlayerOneGamepad.Left = Inputs.GetGamepad(0).LeftXLeft;
        PlayerOneGamepad.Right = Inputs.GetGamepad(0).LeftXRight;
        PlayerOneGamepad.Jump = Inputs.GetGamepad(0).South;
        PlayerOneGamepad.Shoot = Inputs.GetGamepad(0).West;

    }

    public override void Update(TimeSpan timeSpan)
    {
        InputState inputState = GetInputState(PlayerOneKeyboard, PlayerOneGamepad);
        GlobalInput.Prev = GlobalInput.Current;
        GlobalInput.Current = inputState;
        foreach (var playerEntity in PlayerFilter.Entities)
        {
            Set(playerEntity, inputState);
        }

        GlobalInput.MouseX = Inputs.Mouse.X / ((int)Window.Width / Dimensions.GAME_W);
        GlobalInput.MouseY = Inputs.Mouse.Y / ((int)Window.Height / Dimensions.GAME_H);
        GlobalInput.Left = Inputs.Mouse.LeftButton.State;
        GlobalInput.Right = Inputs.Mouse.RightButton.State;
        // Console.WriteLine($"mouse: {Inputs.Mouse.X},{Inputs.Mouse.Y}");
    }

    private static InputState GetInputState(ControlSet controlSet, ControlSet altControlSet)
    {
        return new InputState
        {
            Left = controlSet.Left.State | altControlSet.Left.State,
            Right = controlSet.Right.State | altControlSet.Right.State,
            Up = controlSet.Up.State | altControlSet.Up.State,
            Down = controlSet.Down.State | altControlSet.Down.State,
            Jump = controlSet.Jump.State | altControlSet.Jump.State,
            Shoot = controlSet.Shoot.State | altControlSet.Shoot.State,
            Reload = controlSet.Reload.State | altControlSet.Reload.State,
            Refresh = controlSet.Refresh.State | altControlSet.Refresh.State,
        };
    }
}
