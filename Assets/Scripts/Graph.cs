using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public class Graph
    {
        private static int verticesCount;
        private List<Edge> edges;
        private List<Vertex> vertices;
        private double[,] graph;
        private List<Edge> MST;

        private void SetupData(List<Vertex> vertices)
        {
            edges = new List<Edge>();
            this.vertices = vertices;
            verticesCount = vertices.Count;
            for (int i = 0; i < verticesCount; i++)
                vertices[i].SetIndex(i);
            graph = new double[verticesCount, verticesCount];
            MST = new List<Edge>();
        }

        public void CreateGraph(List<Vertex> vertices)
        {
            SetupData(vertices);
            CreateEdges();
            CreateMatrix();
            CreateMST(Prim.MST(graph, verticesCount));
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

        }

        // Methods for Debuging
        public void ViewData()
        {
            Debug.Log("Vertices - " + vertices.Count);
            Debug.Log("Edges - " + edges.Count);
            Debug.Log("MST Edges - " + MST.Count);
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

                Debug.DrawLine(firstPosition, secondPosition, Color.white, duration);
            }
        }
    }
}