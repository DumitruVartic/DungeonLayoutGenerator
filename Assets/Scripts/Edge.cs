using UnityEngine;

namespace Graph
{
    public class Edge
    {
        private Vertex first;
        private Vertex second;
        private double distance;

        public Edge(Vertex first, Vertex second)
        {
            this.first = first;
            this.second = second;
            CalculateDistance();
        }

        public Edge(Edge edge)
        {
            this.first = edge.first;
            this.second = edge.second;
            this.distance = edge.distance;
        }

        public Vertex GetFirst() { return first; }
        public Vertex GetSecond() { return second; }
        public double GetDistance() { return distance; }

        private void CalculateDistance()
        {
            distance = Vector3.Distance(first.GetPosition(), second.GetPosition());
        }
    }
}