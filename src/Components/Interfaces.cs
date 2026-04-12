using MyGame.Data;

namespace MyGame.Components;

public interface TimedComponent<T> where T : unmanaged, TimedComponent<T>
{
    public float Time { get; }
    public T Update(float t);
}

public interface AnimComponent
{
    public SpriteAnimationInfoID Animation { get; }
}

public interface LerpComponent<T1, T2> where T1 : unmanaged, LerpComponent<T1, T2> where T2 : unmanaged {
    public float Start {get;}
    public float End {get;}
    public float MaxTime {get;}
    public float Progress {get;}
    public T1 Update(float t);
    public T2 Apply(float value);
}
public interface ITrigger;