using UnityEngine;

namespace Map
{
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
            return pos.x + size.x * pos.y + size.x * size.y * pos.z;
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

        // Methods for Debuging
        public void ViewArea(int duration)
        {
            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(size.x, 0, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, 0, size.z), Color.white, duration);

            Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(size.x, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, size.y, 0), new Vector3Int(0, size.y, size.z), Color.white, duration);

            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(0, 0, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, 0, 0), Color.white, duration);

            Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(0, size.y, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, size.y, size.z), new Vector3Int(size.x, size.y, 0), Color.white, duration);

            Debug.DrawLine(new Vector3Int(0, 0, 0), new Vector3Int(0, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, 0), new Vector3Int(size.x, size.y, 0), Color.white, duration);
            Debug.DrawLine(new Vector3Int(0, 0, size.z), new Vector3Int(0, size.y, size.z), Color.white, duration);
            Debug.DrawLine(new Vector3Int(size.x, 0, size.z), new Vector3Int(size.x, size.y, size.z), Color.white, duration);
        }
    }
}