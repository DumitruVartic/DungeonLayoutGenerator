using System;
using UnityEngine;

namespace Graph
{
    public class Vertex
    {
        public Vector3Int Position { get; private set; }
        public int Index { get; private set; }

        public Vertex(Vector3Int Position) { this.Position = Position; }

        public Vertex(Vertex vertex)
        {
            this.Position = vertex.Position;
            this.Index = vertex.Index;
        }

        public void SetIndex(int value) { Index = value; }

        public override bool Equals(object obj)
        {
            return obj is Vertex vertex &&
                   Position.Equals(vertex.Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position);
        }
    }
}