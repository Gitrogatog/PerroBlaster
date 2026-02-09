using System;
using System.Collections.Generic;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Input;
using MyGame;
using MyGame.Components;

namespace MyGame.Systems;

public struct InputState
{
    public ButtonState Left { get; set; }
    public ButtonState Right { get; set; }
    public ButtonState Up { get; set; }
    public ButtonState Down { get; set; }
    public ButtonState Shoot { get; set; }
    public ButtonState AltShoot {get; set;}
    public ButtonState Reload { get; set; }
    public ButtonState Refresh { get; set; }
    public ButtonState WindowPlus { get; set; }
    public ButtonState WindowMinus { get; set; }
    public ButtonState CloseWindow { get; set; }
}

public class ControlSet
{
    public VirtualButton Left { get; set; } = new EmptyButton();
    public VirtualButton Right { get; set; } = new EmptyButton();
    public VirtualButton Up { get; set; } = new EmptyButton();
    public VirtualButton Down { get; set; } = new EmptyButton();
    public VirtualButton AltShoot { get; set; } = new EmptyButton();
    public VirtualButton Shoot { get; set; } = new EmptyButton();
    public VirtualButton CloseWindow { get; set; } = new EmptyButton();
    public VirtualButton Reload { get; set; } = new EmptyButton();
    public VirtualButton Refresh { get; set; } = new EmptyButton();
    public VirtualButton WindowPlus { get; set; } = new EmptyButton();
    public VirtualButton WindowMinus { get; set; } = new EmptyButton();
}
public class ControlSetList
{
    public List<VirtualButton> Left { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> Right { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> Up { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> Down { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> AltShoot { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> Shoot { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> CloseWindow { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> Reload { get; set; } = new List<VirtualButton>();

    public List<VirtualButton> Refresh { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> WindowPlus { get; set; } = new List<VirtualButton>();
    public List<VirtualButton> WindowMinus { get; set; } = new List<VirtualButton>();
}

public class Input : MoonTools.ECS.System
{
    Inputs Inputs { get; }

    Filter PlayerFilter { get; }
    Window Window;

    ControlSet PlayerOneKeyboard = new ControlSet();
    ControlSet PlayerOneGamepad = new ControlSet();
    ControlSetList Controls = new ControlSetList();
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

        AddKeyCode(Controls.Up, KeyCode.W, KeyCode.Up);
        AddKeyCode(Controls.Down, KeyCode.S, KeyCode.Down);
        AddKeyCode(Controls.Left, KeyCode.A, KeyCode.Left);
        AddKeyCode(Controls.Right, KeyCode.D, KeyCode.Right);

        
        AddKeyCode(Controls.Shoot, KeyCode.J);
        AddMouseCode(Controls.Shoot, MouseButtonCode.Left);
        AddKeyCode(Controls.AltShoot, KeyCode.K);
        AddMouseCode(Controls.AltShoot, MouseButtonCode.Right);
       
        AddKeyCode(Controls.Reload, KeyCode.T);
        AddKeyCode(Controls.Refresh, KeyCode.R);
        AddKeyCode(Controls.WindowMinus, KeyCode.Minus);
        AddKeyCode(Controls.WindowPlus, KeyCode.Equals);
        AddKeyCode(Controls.CloseWindow, KeyCode.Escape);

        Gamepad gamepad = Inputs.GetGamepad(0);
        Controls.Up.Add(gamepad.LeftYDown);
        Controls.Down.Add(gamepad.LeftYUp);
        Controls.Left.Add(gamepad.LeftXLeft);
        Controls.Right.Add(gamepad.LeftXRight);
        Controls.Shoot.Add(gamepad.South);
        Controls.AltShoot.Add(gamepad.West);
        Controls.CloseWindow.Add(gamepad.Start);
                // PlayerOneKeyboard.WindowPlus = Inputs.Keyboard.Button(KeyCode.Equals);
        // PlayerOneKeyboard.WindowMinus = Inputs.Keyboard.Button(KeyCode.Minus);
        // PlayerOneKeyboard.CloseWindow = Inputs.Keyboard.Button(KeyCode.Escape);

        // PlayerOneKeyboard.Up = Inputs.Keyboard.Button(KeyCode.W);
        // PlayerOneKeyboard.Down = Inputs.Keyboard.Button(KeyCode.S);
        // PlayerOneKeyboard.Left = Inputs.Keyboard.Button(KeyCode.A);
        // PlayerOneKeyboard.Right = Inputs.Keyboard.Button(KeyCode.D);
        // PlayerOneKeyboard.Jump = Inputs.Keyboard.Button(KeyCode.J);
        // PlayerOneKeyboard.Shoot = Inputs.Keyboard.Button(KeyCode.K);
        // PlayerOneKeyboard.Reload = Inputs.Keyboard.Button(KeyCode.T);
        // PlayerOneKeyboard.Refresh = Inputs.Keyboard.Button(KeyCode.R);
        // PlayerOneKeyboard.WindowPlus = Inputs.Keyboard.Button(KeyCode.Equals);
        // PlayerOneKeyboard.WindowMinus = Inputs.Keyboard.Button(KeyCode.Minus);
        // PlayerOneKeyboard.CloseWindow = Inputs.Keyboard.Button(KeyCode.Escape);

        PlayerOneGamepad.Up = Inputs.GetGamepad(0).LeftYDown;
        
        // PlayerOneGamepad.Down = Inputs.GetGamepad(0).LeftYUp;
        // PlayerOneGamepad.Left = Inputs.GetGamepad(0).LeftXLeft;
        // PlayerOneGamepad.Right = Inputs.GetGamepad(0).LeftXRight;
        // PlayerOneGamepad.Jump = Inputs.GetGamepad(0).South;
        // PlayerOneGamepad.Shoot = Inputs.GetGamepad(0).West;

    }
    private void AddMouseCode(List<VirtualButton> list, MouseButtonCode key) => list.Add(Inputs.Mouse.Button(key));
    private void AddKeyCode(List<VirtualButton> list, KeyCode key) => list.Add(Inputs.Keyboard.Button(key));
    private void AddKeyCode(List<VirtualButton> list, KeyCode key1, KeyCode key2) {
        list.Add(Inputs.Keyboard.Button(key1));
        list.Add(Inputs.Keyboard.Button(key2));
    }
    private void AddKeyCode(List<VirtualButton> list, KeyCode key1, KeyCode key2, KeyCode key3) {
        list.Add(Inputs.Keyboard.Button(key1));
        list.Add(Inputs.Keyboard.Button(key2));
        list.Add(Inputs.Keyboard.Button(key3));
    }


    public override void Update(TimeSpan timeSpan)
    {
        // InputState inputState = GetInputState(PlayerOneKeyboard, PlayerOneGamepad);
        InputState inputState = GetInputState(Controls);
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

    private static InputState GetInputState(ControlSetList controlSet) {
        return new InputState {
            Left = GetButtonState(controlSet.Left),
            Right = GetButtonState(controlSet.Right),
            Up = GetButtonState(controlSet.Up),
            Down = GetButtonState(controlSet.Down),
            AltShoot = GetButtonState(controlSet.AltShoot),
            Shoot = GetButtonState(controlSet.Shoot),
            Reload = GetButtonState(controlSet.Reload),
            Refresh = GetButtonState(controlSet.Refresh),
            WindowMinus = GetButtonState(controlSet.WindowMinus),
            WindowPlus = GetButtonState(controlSet.WindowPlus),
            CloseWindow = GetButtonState(controlSet.CloseWindow),
        };
    }

    private static InputState GetInputState(ControlSet controlSet, ControlSet altControlSet)
    {
        return new InputState
        {
            Left = controlSet.Left.State | altControlSet.Left.State,
            Right = controlSet.Right.State | altControlSet.Right.State,
            Up = controlSet.Up.State | altControlSet.Up.State,
            Down = controlSet.Down.State | altControlSet.Down.State,
            AltShoot = controlSet.AltShoot.State | altControlSet.AltShoot.State,
            Shoot = controlSet.Shoot.State | altControlSet.Shoot.State,
            Reload = controlSet.Reload.State | altControlSet.Reload.State,
            Refresh = controlSet.Refresh.State | altControlSet.Refresh.State,
            WindowPlus = controlSet.WindowPlus.State | altControlSet.WindowPlus.State,
            WindowMinus = controlSet.WindowMinus.State | altControlSet.WindowMinus.State,
        };
    }
    private static ButtonState GetButtonState(List<VirtualButton> control) {
        if(control.Count == 0) return new ButtonState();
        ButtonState state = control[0].State;
        for(int i = 1; i < control.Count; i++){
            state |= control[i].State;
        }
        return state;
    }
}