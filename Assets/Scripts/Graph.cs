using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Graph
{
    public class Graph // probabil ar fi bine de creat ceva gen graful initialView etc resultatulView etc, in cazul dat va trebui de pastrat lista cu edges initiala 
    {
        private static int verticesCount;
        private List<Edge> edges;
        private List<Vertex> vertices;
        private double[,] graph;
        private List<Edge> MST;

        private void SetupData(List<Vertex> vertices)
        {
            edges = new List<Edge>();
            this.vertices = new List<Vertex>();
            this.vertices = vertices;
            verticesCount = vertices.Count;
            graph = new double[verticesCount, verticesCount];
            MST = new List<Edge>();
            for (int i = 0; i < verticesCount; i++)
                vertices[i].SetIndex(i);
        }

        public void CreateGraph(List<Vertex> vertices)
        {
            SetupData(vertices);
            CreateEdges();
            CreateMatrix();
            CreateMST(Prim.MST(graph, verticesCount));
            AddSomeExtraEdges();
            // adaugarea la edgeuri in plus, dar de facut asa ca acestea de asemeni sa fie minime

            Debug.Log("Vertices - " + vertices.Count);
            Debug.Log("Edges - " + edges.Count);
            /*StringBuilder sb = new StringBuilder();
            for (int i = 0; i < graph.GetLength(1); i++)
            {
                for (int j = 0; j < graph.GetLength(0); j++)
                {
                    sb.Append(graph[i, j]);
                    sb.Append(' ');
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
*/
            /*StringBuilder ad = new StringBuilder();
            for (int i = 1; i < parent.Length; i++)
            {
                    ad.Append(parent[i]);
                    ad.Append(" ");
                    ad.Append(i);
                    ad.Append(" ");
                    ad.Append(Convert.ToString(graph[parent[i], i]));
                    ad.Append('\n');
            }*/
           /* Debug.Log(ad.ToString());
            Debug.Log(graph[0,1]);

            for (int i = 1; i < parent.Length; i++)
            {
                    ad.Append(parent[i]);
                    ad.Append(" ");
                    ad.Append(i);
                    ad.Append(" ");
                    ad.Append(Convert.ToString(graph[parent[i], i]));
                    ad.Append('\n');

                    Edge newEdge = new Edge(firstVertex, secondVertex);
                    edges.Add(newEdge);
            }*/
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
                    if (firstEdge.GetFirst() == secondEdge.GetSecond()
                        && firstEdge.GetSecond() == secondEdge.GetFirst()
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
                graph[edge.GetFirst().Index, edge.GetSecond().Index] = edge.GetDistance();
            }
        }

        private void CreateMST(int[] parent)
        {
            for (int i = 1; i < parent.Length; i++)
            {
                foreach (Edge edge in edges)
                {
                    if (edge.GetFirst().Index == parent[i] && edge.GetSecond().Index == i)
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

        public void ViewGraph()
        {
            foreach (Edge edge in edges)
            {
                Vector3Int firstPosition = edge.GetFirst().GetPosition();
                Vector3Int secondPosition = edge.GetSecond().GetPosition();

                int time = 100;
                Debug.DrawLine(firstPosition, secondPosition, Color.white, time);
            }
        }

        public void ViewMST() 
        {
            foreach (Edge edge in MST)
            {
                Vector3Int firstPosition = edge.GetFirst().GetPosition();
                Vector3Int secondPosition = edge.GetSecond().GetPosition();

                int time = 100;
                Debug.DrawLine(firstPosition, secondPosition, Color.white, time);
            }
        }
    }
}