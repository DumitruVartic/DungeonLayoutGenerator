using UnityEngine;

public class Grid<T>
{
    private T[] data;
    private Vector3Int size;

    public Grid(Vector3Int size)
    {
        this.size = size;

        data = new T[size.x * size.y * size.z];
    }

    private int GetIndex(Vector3Int pos)
    {
        return pos.x + (size.x * pos.y) + (size.x * size.y * pos.z);
    }

    public bool InBounds(Vector3Int pos)
    {
        return new BoundsInt(Vector3Int.zero, size).Contains(pos);
    }

    public T this[Vector3Int pos]
    {
        get
        {
            return data[GetIndex(pos)];
        }
        set
        {
            data[GetIndex(pos)] = value;
        }
    }
}
