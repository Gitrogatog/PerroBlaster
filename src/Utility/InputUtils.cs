namespace MyGame.Utility;
public static class InputUtils {
    public static int InputToAxis(bool negative, bool positive)
    {
        if (negative && !positive) return -1;
        if (!negative && positive) return 1;
        return 0;
    }
}