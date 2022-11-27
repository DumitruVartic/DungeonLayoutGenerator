using UnityEngine;

namespace Graph
{
    public class Vertex
    {
        public Vector3Int Position { get; private set; }
        public int Index { get; private set; }
        public void SetIndex(int value) { Index = value; }
        public Vertex(Vector3Int Position) { this.Position = Position; }
    }
}