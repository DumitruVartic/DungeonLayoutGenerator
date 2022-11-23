using System;
using UnityEngine;

namespace Graph
{
    public class Vertex
    {
        private Vector3Int position;
        public int Index { get; private set; }

        public void SetIndex(int value) { Index = value; }

        public Vertex(Vector3Int position)
        {
            this.position = position;
        }

        public Vertex(Vertex vertex) 
        { 
            this.position = vertex.position;
            this.Index = vertex.Index;
        }

        public Vector3Int GetPosition() { return position; }

        public override bool Equals(object obj)
        {
            return obj is Vertex vertex &&
                   position.Equals(vertex.position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position);
        }

    }
}