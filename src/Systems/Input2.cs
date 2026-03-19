using System;
using System.Collections.Generic;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Input;

namespace MyGame.Systems;

public enum InputActions {
    Left, Right, Up, Down,
    Interact, Cancel,
    Reload, Refresh, Fullscreen
}

public struct InputState
{
    public ButtonState Left { get; set; }
    public ButtonState Right { get; set; }
    public ButtonState Up { get; set; }
    public ButtonState Down { get; set; }
    public ButtonState Interact { get; set; }
    public ButtonState Cancel { get; set; }
// #if DEBUG
//     public ButtonState Reload { get; set; }
// #endif
    public ButtonState Reload { get; set; }
    public ButtonState Refresh { get; set; }
    public ButtonState Fullscreen {get; set;}
}
public struct LastInputTime {
    public double Left;
    public double Right;
    public double Up;
    public double Down;
    public double Interact;
    public double Cancel;
}

public class Input2 : MoonTools.ECS.System
{
    static Inputs Inputs { get; set; }

    InputActions RebindState = (InputActions)0;


    public static Dictionary<InputActions, List<KeyboardButton>> Keyboard;
    public static Dictionary<InputActions, GamepadButton> Gamepad;

    // public static string GetButtonName(InputActions action)
    // {
    //     if (Inputs.GamepadExists(0))
    //     {
    //         return Gamepad[action].Code.ToString();
    //     }
    //     else
    //     {
    //         return Keyboard[action].KeyCode.ToString();
    //     }
    // }



    public Input2(World world, Inputs inputs) : base(world)
    {
        Inputs = inputs;
        Keyboard = new();


        // if (save.Keyboard == null)
        {
            Keyboard[InputActions.Up] = CreateKeyboardButtons([KeyCode.Up, KeyCode.K, KeyCode.Keypad8]);
            Keyboard[InputActions.Down] = CreateKeyboardButtons([KeyCode.Down, KeyCode.J, KeyCode.Keypad2]);
            Keyboard[InputActions.Left] = CreateKeyboardButtons([KeyCode.Left, KeyCode.H, KeyCode.Keypad4]);
            Keyboard[InputActions.Right] = CreateKeyboardButtons([KeyCode.Right, KeyCode.L, KeyCode.Keypad6]);
            Keyboard[InputActions.Interact] = CreateKeyboardButtons([KeyCode.Z, KeyCode.Y, KeyCode.KeypadEnter, KeyCode.Space]);
            Keyboard[InputActions.Cancel] = CreateKeyboardButtons([KeyCode.Escape, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.Keypad0]);
            Keyboard[InputActions.Refresh] = CreateKeyboardButtons([KeyCode.R]);
            Keyboard[InputActions.Reload] = CreateKeyboardButtons([KeyCode.T]);
            Keyboard[InputActions.Fullscreen] = CreateKeyboardButtons([KeyCode.F4]);
// #if DEBUG
//             Keyboard[InputActions.Restart] = Inputs.Keyboard.Button(KeyCode.R);
// #endif
//             Keyboard[InputActions.Dash] = Inputs.Keyboard.Button(KeyCode.LeftShift);
        }
        // else
        // {
        //     foreach (var (action, code) in save.Keyboard)
        //     {
        //         Keyboard[action] = Inputs.Keyboard.Button(code);
        //     }
        // }

        Gamepad = new();

        // if (save.Gamepad == null)
//         {
//             Gamepad[InputActions.Up] = Inputs.GetGamepad(0).DpadUp;
//             Gamepad[InputActions.Down] = Inputs.GetGamepad(0).DpadDown;
//             Gamepad[InputActions.Left] = Inputs.GetGamepad(0).DpadLeft;
//             Gamepad[InputActions.Right] = Inputs.GetGamepad(0).DpadRight;
//             Gamepad[InputActions.Launch] = Inputs.GetGamepad(0).A;
//             Gamepad[InputActions.Cancel] = Inputs.GetGamepad(0).B;
//             Gamepad[InputActions.Start] = Inputs.GetGamepad(0).Start;
// #if DEBUG
//             Gamepad[InputActions.Restart] = Inputs.GetGamepad(0).Guide;
// #endif
//             Gamepad[InputActions.Dash] = Inputs.GetGamepad(0).RightShoulder;
//         }
        // else
        // {
        //     foreach (var (action, code) in save.Gamepad)
        //     {
        //         Gamepad[action] = Inputs.GetGamepad(0).Button(code);
        //     }
        // }

        var inputEntity = CreateEntity();
        var inputState = InputState();
        Set(inputEntity, inputState);
        Set(inputEntity, GetLastInputTime(inputState, new LastInputTime(), Globals.CurrentTime));
        GlobalInput.Current = inputState;
    }

    public override void Update(TimeSpan delta)
    {
        InputState inputState = InputState();
        var singleton = GetSingletonEntity<InputState>();
        Set(singleton, inputState);
        Set(singleton, GetLastInputTime(inputState, Get<LastInputTime>(singleton), Globals.CurrentTime));
        GlobalInput.Prev = GlobalInput.Current;
        GlobalInput.Current = inputState;
        // var rebinding = Some<RebindControls>() && GetSingleton<RebindControls>().Rebinding == true;

        // // if (rebinding)
        // {
        //     var display = InRelationSingleton<SettingControls>(GetSingletonEntity<RebindControls>());

        //     Set(display,
        //         new Text(
        //             Fonts.HeaderFont,
        //             Some<Player>() ? Fonts.PromptSize : Fonts.BodySize,
        //             Stores.TextStorage.GetID($"Press {RebindState} (Current: {Keyboard[RebindState].KeyCode} | {Gamepad[RebindState].Code})"),
        //             MoonWorks.Graphics.Font.HorizontalAlignment.Left,
        //             MoonWorks.Graphics.Font.VerticalAlignment.Middle));

        //     if (Inputs.AnyPressed)
        //     {
        //         if (Inputs.Keyboard.AnyPressed)
        //         {
        //             Keyboard[RebindState] = Inputs.Keyboard.AnyPressedButton;
        //             RebindState++;
        //         }
        //         if (Inputs.GetGamepad(0).AnyPressed)
        //         {
        //             if (Inputs.GetGamepad(0).AnyPressedButton.GetType() == typeof(GamepadButton))
        //             {
        //                 Gamepad[RebindState] = (GamepadButton)Inputs.GetGamepad(0).AnyPressedButton;
        //                 RebindState++;

        //             }
        //         }
        //     }

        //     // if (RebindState > InputActions.Dash)
        //     // {
        //     //     RebindState = 0;
        //     //     Set(GetSingletonEntity<RebindControls>(), new RebindControls(false));

        //     //     Set(display,
        //     //         new Text(
        //     //             Fonts.HeaderFont,
        //     //             Some<Player>() ? Fonts.PromptSize : Fonts.BodySize,
        //     //             Stores.TextStorage.GetID("rebind controls"),
        //     //             MoonWorks.Graphics.Font.HorizontalAlignment.Left,
        //     //             MoonWorks.Graphics.Font.VerticalAlignment.Middle));

        //     //     SaveGame.Save();
        //     // }
        // }
    
    }

    private static LastInputTime GetLastInputTime(InputState currentInput, LastInputTime prevTime, double currentTime) {
        return new LastInputTime {
            Left = currentInput.Left.IsPressed ? currentTime : prevTime.Left,
            Right = currentInput.Right.IsPressed ? currentTime : prevTime.Right,
            Up = currentInput.Up.IsPressed ? currentTime : prevTime.Up,
            Down = currentInput.Down.IsPressed ? currentTime : prevTime.Down,
            Interact = currentInput.Interact.IsPressed ? currentTime : prevTime.Interact,
            Cancel = currentInput.Cancel.IsPressed ? currentTime : prevTime.Cancel,
        };
    }

    private static InputState InputState()
    {
        return new InputState
        {
            Left = CheckButtonState(Keyboard[InputActions.Left]),
            Right = CheckButtonState(Keyboard[InputActions.Right]),
            Up = CheckButtonState(Keyboard[InputActions.Up]),
            Down = CheckButtonState(Keyboard[InputActions.Down]),
            Interact = CheckButtonState(Keyboard[InputActions.Interact]),
            Cancel = CheckButtonState(Keyboard[InputActions.Cancel]),
            Reload = CheckButtonState(Keyboard[InputActions.Reload]),
            Refresh = CheckButtonState(Keyboard[InputActions.Refresh]),
            Fullscreen = CheckButtonState(Keyboard[InputActions.Fullscreen])
//             Launch = CheckButtonState(Keyboard[InputActions.Launch]),
//             Start = Keyboard[InputActions.Start].State | Gamepad[InputActions.Start].State,
// #if DEBUG
//             Restart = Keyboard[InputActions.Restart].State | Gamepad[InputActions.Restart].State,
// #endif
//             Dash = Keyboard[InputActions.Dash].State | Gamepad[InputActions.Dash].State,
        };
    }
    private static ButtonState CheckButtonState(List<KeyboardButton> buttonList) {
        if(buttonList.Count == 0) return default;
        ButtonState state = buttonList[0].State;
        for(int i = 1; i < buttonList.Count; i++) {
            state |= buttonList[i].State;
        }
        return state;
    }
    private static List<KeyboardButton> CreateKeyboardButtons(KeyCode[] inputs) {
        List<KeyboardButton> buttons = new List<KeyboardButton>(inputs.Length);
        foreach(var input in inputs) {
            buttons.Add(Inputs.Keyboard.Button(input));
        }
        return buttons;
    }
    // private static List<GamepadButton> CreateKeyboardButtons(GamepadButtonCode[] inputs) {
    //     List<GamepadButton> buttons = new List<GamepadButton>(inputs.Length);
    //     foreach(var input in inputs) {
    //         buttons.Add(Inputs.GetGamepad(0).Button());
    //     }
    //     return buttons;
    // }
}