using System.Collections.Generic;
using MoonTools.ECS;
using MoonWorks.Input;
using MyGame.Systems;
namespace MyGame;

public static class GlobalInput
{
    public static int MouseX;
    public static int MouseY;
    public static ButtonState Left;
    public static ButtonState Right;
    public static InputState Current;
    public static InputState Prev;
}
public static class GlobalState
{
    public static bool ShouldExistPlayer = true;
}