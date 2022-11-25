using UnityEngine;

namespace Graph
{
    public class Edge
    {
        public Vertex Source { get; private set; }
        public Vertex Destination { get; private set; }
        private double weight;
        public double Weight => weight;

        public Edge(Vertex Source, Vertex Destination)
        {
            this.Source = Source;
            this.Destination = Destination;
            CalculateDistance();
        }

        public Edge(Edge edge)
        {
            this.Source = edge.Source;
            this.Destination = edge.Destination;
            this.weight = edge.weight;
        }

        private void CalculateDistance()
        {
            weight = Vector3.Distance(Source.Position, Destination.Position);
        }
    }
}