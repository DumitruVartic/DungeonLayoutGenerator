using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Graph
{
    public class Graph
    {
        private Random random;
        private static int verticesCount;
        private List<Edge> edges;
        private List<Vertex> vertices;
        private double[,] graph;
        private List<Edge> MST;
        private HashSet<Edge> selectedEdges;
        private double probabilityToSelect;

        public HashSet<Edge> GetSelectedEdges()
        {
            if (selectedEdges != null)
                return selectedEdges;
            return new HashSet<Edge>(MST);
        }

        private void SetupData(List<Vertex> vertices, double probabilityToSelect, int randomSeed)
        {
            this.probabilityToSelect = probabilityToSelect;
            random = new Random(randomSeed);
            edges = new List<Edge>();
            this.vertices = vertices;
            verticesCount = vertices.Count;
            graph = new double[verticesCount, verticesCount];
            MST = new List<Edge>();
            selectedEdges = null;
        }

        public void CreateGraph(List<Vertex> vertices, double probabilityToSelect, int randomSeed, bool addExtraEdges, Map.Generator.Alghoritm alghoritm)
        {
            SetupData(vertices, probabilityToSelect, randomSeed);
            CreateEdges();
            CreateMatrix();
            if (alghoritm == Map.Generator.Alghoritm.Kruskal)
                MST = new List<Edge>(Kruskal.MST(edges, verticesCount));
            else
                CreateMST(Prim.MST(graph, verticesCount));

            if (addExtraEdges)
                AddSomeExtraEdges();
        }

        private void CreateEdges()
        {
            foreach (Vertex firstVertex in vertices)
            {
                foreach (Vertex secondVertex in vertices)
                {
                    if (firstVertex != secondVertex)
                    {
                        Edge newEdge = new Edge(firstVertex, secondVertex);
                        edges.Add(newEdge);
                    }
                }
            }

            RemoveUselessEdge();
        }

        private void RemoveUselessEdge()
        {
            List<Edge> pickedEdge = new List<Edge>();
            List<Edge> removeEdges = new List<Edge>();
            foreach (Edge firstEdge in edges)
            {
                foreach (Edge secondEdge in edges)
                {
                    if (firstEdge.Source == secondEdge.Destination
                        && firstEdge.Destination == secondEdge.Source
                        && (!pickedEdge.Contains(secondEdge) || !pickedEdge.Contains(firstEdge)))
                    {
                        pickedEdge.Add(firstEdge);
                        pickedEdge.Add(secondEdge);
                        removeEdges.Add(secondEdge);
                    }
                }
            }

            foreach (Edge edge in removeEdges)
            {
                edges.Remove(edge);
            }
        }

        private void CreateMatrix()
        {
            foreach (Edge edge in edges)
            {
                graph[edge.Source.Index, edge.Destination.Index] = edge.Weight;
            }
        }

        private void CreateMST(int[] parent)
        {
            for (int i = 1; i < parent.Length; i++)
            {
                foreach (Edge edge in edges)
                {
                    if (edge.Source.Index == parent[i] && edge.Destination.Index == i)
                    {
                        Edge newEdge = new Edge(edge);
                        MST.Add(newEdge);
                    }
                }
            }
        }

        private void AddSomeExtraEdges()
        {
            selectedEdges = new HashSet<Edge>(MST);
            var remainingEdges = new HashSet<Edge>(edges);
            remainingEdges.ExceptWith(selectedEdges);

            foreach (var edge in remainingEdges)
                if (random.NextDouble() < probabilityToSelect)
                    selectedEdges.Add(edge);
        }

        // Methods for Debuging
        public void ViewData()
        {
            Debug.Log("Vertices - " + vertices.Count);
            Debug.Log("Edges - " + edges.Count);
            Debug.Log("MST Edges - " + MST.Count);
            Debug.Log("Selected Edges - " + selectedEdges.Count);
        }

        public void Clear()
        {
            graph = null;
            MST = null;
        }

        public void ViewGraph(int duration)
        {
            foreach (Edge edge in edges)
            {
                Vector3Int firstPosition = edge.Source.Position;
                Vector3Int secondPosition = edge.Destination.Position;

                Debug.DrawLine(firstPosition, secondPosition, Color.white, duration);
            }
        }

        public void ViewMST(int duration)
        {
            foreach (Edge edge in MST)
            {
                Vector3Int firstPosition = edge.Source.Position;
                Vector3Int secondPosition = edge.Destination.Position;

                Debug.DrawLine(firstPosition, secondPosition, Color.blue, duration);
            }
        }

        public void ViewSelectedEdges(int duration)
        {
            foreach (Edge edge in selectedEdges)
            {
                Vector3Int firstPosition = edge.Source.Position;
                Vector3Int secondPosition = edge.Destination.Position;

                Debug.DrawLine(firstPosition, secondPosition, Color.white, duration);
            }
        }
    }
}