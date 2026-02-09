using System.Collections.Generic;
using System.Numerics;
using MoonWorks.Audio;
using MoonWorks.Graphics.Font;

namespace MyGame.Data;

public static class Stores
{
    // public static Storage<string> TextStorage = new Storage<string>();
    public static Storage<Font> FontStorage = new Storage<Font>();
    public static Storage<AudioBuffer> SFXStorage = new Storage<AudioBuffer>();
    public static Storage<string, List<Vector2>> PathStorage = new Storage<string, List<Vector2>>();
}

// Generic class for storing managed types
public class Storage<T>
{
    Dictionary<T, int> ToID = new Dictionary<T, int>();
    T[] IDTo = new T[256];
    Stack<int> OpenIDs = new Stack<int>();
    int NextID = 0;

    public T Get(int id)
    {
        return IDTo[id];
    }

    public int GetID(T text)
    {
        if (!ToID.ContainsKey(text))
        {
            Register(text);
        }

        return ToID[text];
    }

    private void Register(T text)
    {
        if (OpenIDs.Count == 0)
        {
            if (NextID >= IDTo.Length)
            {
                System.Array.Resize(ref IDTo, IDTo.Length * 2);
            }
            ToID[text] = NextID;
            IDTo[NextID] = text;
            NextID += 1;
        }
        else
        {
            ToID[text] = OpenIDs.Pop();
        }
    }
}

public class Storage<T1, T2>
{
    Dictionary<T1, int> ToID = new Dictionary<T1, int>();
    T2[] IDToContent = new T2[256];
    Stack<int> OpenIDs = new Stack<int>();
    int NextID = 0;
    public bool IsEmpty => NextID == 0;
    public void Reset() {
        ToID.Clear();
        NextID = 0;
    }
    public T2 Get(int id)
    {
        return IDToContent[id];
    }

    public int GetID(T1 text)
    {
        // if (!ToID.ContainsKey(text))
        // {
        //     Register(text);
        // }

        return ToID[text];
    }
    public int Register(T1 text, T2 content)
    {
        if (OpenIDs.Count == 0)
        {
            if (NextID >= IDToContent.Length)
            {
                System.Array.Resize(ref IDToContent, IDToContent.Length * 2);
            }
            ToID[text] = NextID;
            IDToContent[NextID] = content;
            NextID += 1;
        }
        else
        {
            ToID[text] = OpenIDs.Pop();
        }
        return NextID - 1;
    }

}