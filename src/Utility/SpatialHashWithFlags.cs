using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MoonTools.ECS;
using MyGame;
using MyGame.Components;

/// <summary>
/// Used to quickly check if two shapes are potentially overlapping.
/// </summary>
/// <typeparam name="T">The type that will be used to uniquely identify shape-transform pairs.</typeparam>

public record struct Entry(Entity Entity, Rectangle Rectangle, EffectorFlags EffectorFlags, EffectedFlags EffectedFlags);
public class SpatialHashWithFlags
    // <T> where T : unmanaged, System.IEquatable<T>
{
    protected readonly int CellSize;

    protected readonly List<Entry>[][] Cells;
    protected readonly Dictionary<Entity, Entry> IDBoxLookup = new Dictionary<Entity, Entry>();

    protected readonly int X;
    protected readonly int Y;
    protected readonly int Width;
    protected readonly int Height;
    protected readonly int RowCount;
    protected readonly int ColumnCount;

    private Queue<HashSet<Entity>> hashSetPool = new Queue<HashSet<Entity>>();
    private HashSet<Collision> Pairs = new HashSet<Collision>();
    public List<Collision>[] Collisions;
    public SpatialHashWithFlags(int x, int y, int width, int height, int cellSize)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        RowCount = width / cellSize;
        ColumnCount = height / cellSize;
        CellSize = cellSize;

        Collisions = new List<Collision>[GlobalCollision.effectorFlags.Length];
        Cells = new List<Entry>[RowCount][];
        for (var i = 0; i < RowCount; i += 1)
        {
            Cells[i] = new List<Entry>[ColumnCount];

            for (var j = 0; j < ColumnCount; j += 1)
            {
                Cells[i][j] = new List<Entry>();
            }
        }
    }

    protected (int, int) Hash(int x, int y)
    {
        return (x / CellSize, y / CellSize);
    }

    // TODO: we could speed this up with a proper Update check
    // that checks the difference between the two hash key ranges

    /// <summary>
    /// Inserts an element into the SpatialHash.
    /// Rectangles outside of the hash range will be ignored!
    /// </summary>
    /// <param name="id">A unique ID for the shape-transform pair.</param>
    public virtual void Insert(Entity parentId, Rectangle rectangle, EffectorFlags effectorFlags, EffectedFlags effectedFlags)
    {
        var relativeX = rectangle.X - X;
        var relativeY = rectangle.Y - Y;
        var rowRangeStart = Math.Clamp(relativeX / CellSize, 0, RowCount - 1);
        var rowRangeEnd = Math.Clamp((relativeX + rectangle.Width) / CellSize, 0, RowCount - 1);
        var columnRangeStart = Math.Clamp(relativeY / CellSize, 0, ColumnCount - 1);
        var columnRangeEnd = Math.Clamp((relativeY + rectangle.Height) / CellSize, 0, ColumnCount - 1);

        for (var i = rowRangeStart; i <= rowRangeEnd; i += 1)
        {
            for (var j = columnRangeStart; j <= columnRangeEnd; j += 1)
            {
                foreach (var other in Cells[i][j]){
                    if (parentId != other.Entity)
                    {
                        if (((long) effectorFlags & (long) other.EffectedFlags) != 0 && Rectangle.TestOverlap(rectangle, other.Rectangle))
                        {
                            AddCollision(parentId, other.Entity, effectorFlags, other.EffectedFlags);
                        }

                        if (((long) effectedFlags & (long) other.EffectorFlags) != 0 && Rectangle.TestOverlap(rectangle, other.Rectangle))
                        {
                            AddCollision(other.Entity, parentId, other.EffectorFlags, effectedFlags);
                        }
                    }
                }
                Cells[i][j].Add(new Entry(parentId, rectangle, effectorFlags, effectedFlags));
            }
        }

        IDBoxLookup[parentId] = new Entry(parentId, rectangle, effectorFlags, effectedFlags);
    }

    private void AddCollision(Entity effector, Entity effected, EffectorFlags aFlags, EffectedFlags bFlags)
    {
        var collision = new Collision(effector, effected);

        if (Pairs.Contains(collision)) { return; }

        var bits = (long) aFlags & (long) bFlags;

        for (var i = 0; i < 64; i += 1)
        {
            if ((bits & (1L << i)) != 0)
            {
                Collisions[i].Add(collision);
            }
        }

        Pairs.Add(collision);
    }

    /// <summary>
    /// Retrieves all the potential collisions of a shape-transform pair. Excludes any shape-transforms with the given ID.
    /// </summary>
    public RetrieveEnumerator Retrieve(Entity id, Rectangle rectangle)
    {
        var relativeX = rectangle.X - X;
        var relativeY = rectangle.Y - Y;
        var rowRangeStart = Math.Clamp(relativeX / CellSize, 0, RowCount - 1);
        var rowRangeEnd = Math.Clamp((relativeX + rectangle.Width) / CellSize, 0, RowCount - 1);
        var columnRangeStart = Math.Clamp(relativeY / CellSize, 0, ColumnCount - 1);
        var columnRangeEnd = Math.Clamp((relativeY + rectangle.Height) / CellSize, 0, ColumnCount - 1);

        return new RetrieveEnumerator(
            this,
            Keys(rowRangeStart, columnRangeStart, rowRangeEnd, columnRangeEnd),
            id
        );
    }

    /// <summary>
    /// Retrieves objects based on a pre-transformed AABB.
    /// </summary>
    /// <param name="aabb">A transformed AABB.</param>
    /// <returns></returns>
    public RetrieveEnumerator Retrieve(Rectangle rectangle)
    {
        var relativeX = rectangle.X - X;
        var relativeY = rectangle.Y - Y;
        var rowRangeStart = Math.Clamp(relativeX / CellSize, 0, RowCount - 1);
        var rowRangeEnd = Math.Clamp((relativeX + rectangle.Width) / CellSize, 0, RowCount - 1);
        var columnRangeStart = Math.Clamp(relativeY / CellSize, 0, ColumnCount - 1);
        var columnRangeEnd = Math.Clamp((relativeY + rectangle.Height) / CellSize, 0, ColumnCount - 1);

        return new RetrieveEnumerator(
            this,
            Keys(rowRangeStart, columnRangeStart, rowRangeEnd, columnRangeEnd)
        );
    }

    /// <summary>
    /// Removes everything that has been inserted into the SpatialHash.
    /// </summary>
    public virtual void Clear()
    {
        for (var i = 0; i < RowCount; i += 1)
        {
            for (var j = 0; j < ColumnCount; j += 1)
            {
                Cells[i][j].Clear();
            }
        }
        foreach(List<Collision> collisionList in Collisions){
            collisionList.Clear();
        }
        Pairs.Clear();

        IDBoxLookup.Clear();
    }

    internal static KeysEnumerator Keys(int minX, int minY, int maxX, int maxY)
    {
        return new KeysEnumerator(minX, minY, maxX, maxY);
    }

    private HashSet<Entity> AcquireHashSet()
    {
        if (hashSetPool.Count == 0)
        {
            hashSetPool.Enqueue(new HashSet<Entity>());
        }

        var hashSet = hashSetPool.Dequeue();
        hashSet.Clear();
        return hashSet;
    }

    private void FreeHashSet(HashSet<Entity> hashSet)
    {
        hashSetPool.Enqueue(hashSet);
    }
    internal ref struct KeysEnumerator
    {
        private int MinX;
        private int MinY;
        private int MaxX;
        private int MaxY;
        private int i, j;

        public KeysEnumerator GetEnumerator() => this;

        public KeysEnumerator(int minX, int minY, int maxX, int maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
            i = minX;
            j = minY - 1;
        }

        public bool MoveNext()
        {
            if (j < MaxY)
            {
                j += 1;
                return true;
            }
            else if (i < MaxX)
            {
                i += 1;
                j = MinY;
                return true;
            }

            return false;
        }

        public (int, int) Current => (i, j);
    }

    public ref struct RetrieveEnumerator
    {
        public SpatialHashWithFlags SpatialHash;
        private KeysEnumerator KeysEnumerator;
        private Span<Entry>.Enumerator SpanEnumerator;
        private bool HashSetEnumeratorActive;
        private HashSet<Entity> Duplicates;
        private Entity? ID;

        public RetrieveEnumerator GetEnumerator() => this;

        internal RetrieveEnumerator(
            SpatialHashWithFlags spatialHash,
            KeysEnumerator keysEnumerator,
            Entity id
        )
        {
            SpatialHash = spatialHash;
            KeysEnumerator = keysEnumerator;
            SpanEnumerator = default;
            HashSetEnumeratorActive = false;
            Duplicates = SpatialHash.AcquireHashSet();
            ID = id;
        }

        internal RetrieveEnumerator(
            SpatialHashWithFlags spatialHash,
            KeysEnumerator keysEnumerator
        )
        {
            SpatialHash = spatialHash;
            KeysEnumerator = keysEnumerator;
            SpanEnumerator = default;
            HashSetEnumeratorActive = false;
            Duplicates = SpatialHash.AcquireHashSet();
            ID = null;
        }

        public bool MoveNext()
        {
            if (!HashSetEnumeratorActive || !SpanEnumerator.MoveNext())
            {
                if (!KeysEnumerator.MoveNext())
                {
                    return false;
                }

                var (i, j) = KeysEnumerator.Current;
                SpanEnumerator = CollectionsMarshal.AsSpan(SpatialHash.Cells[i][j]).GetEnumerator();
                HashSetEnumeratorActive = true;

                return MoveNext();
            }

            // conditions
            var t = SpanEnumerator.Current;

            if (Duplicates.Contains(t.Entity))
            {
                return MoveNext();
            }

            if (ID.HasValue)
            {
                if (ID.Value.Equals(t))
                {
                    return MoveNext();
                }
            }

            Duplicates.Add(t.Entity);
            return true;
        }

        public (Entity, Rectangle, EffectorFlags, EffectedFlags) Current
        {
            get
            {
                var t = SpanEnumerator.Current;
                // var rect = SpatialHash.IDBoxLookup[t.Entity].Rectangle;
                return (t.Entity, t.Rectangle, t.EffectorFlags, t.EffectedFlags);
            }
        }

        public void Dispose()
        {
            SpatialHash.FreeHashSet(Duplicates);
        }
    }

}
