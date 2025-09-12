namespace MyGame.Components;

public interface TimedComponent<T> where T : unmanaged, TimedComponent<T>
{
    public float Time { get; }
    public T Update(float Time);
}