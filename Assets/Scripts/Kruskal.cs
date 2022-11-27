using System.Collections.Generic;
using UnityEngine;

namespace Graph
{
    public class Kruskal
    {
        private static int[] parent;

        private struct Subset
        {
            public int Parent;
            public int Rank;
        }

        private static int Find(Subset[] subsets, int i)
        {
            if (subsets[i].Parent != i)
                subsets[i].Parent = Find(subsets, subsets[i].Parent);

            return subsets[i].Parent;
        }

        private static void Union(Subset[] subsets, int x, int y)
        {
            int xroot = Find(subsets, x);
            int yroot = Find(subsets, y);

            if (subsets[xroot].Rank < subsets[yroot].Rank)
                subsets[xroot].Parent = yroot;
            else if (subsets[xroot].Rank > subsets[yroot].Rank)
                subsets[yroot].Parent = xroot;
            else
            {
                subsets[yroot].Parent = xroot;
                ++subsets[xroot].Rank;
            }
        }

        private static void Print(List<Edge> result)
        {
            foreach (Edge edge in result)
            {
                Vector3Int firstPosition = edge.Source.Position;
                Vector3Int secondPosition = edge.Destination.Position;

                Debug.DrawLine(firstPosition, secondPosition, Color.red, 2);
            }
        }

        public static List<Edge> MST(List<Edge> edges, int verticesCount)
        {
            List<Edge> result = new List<Edge>();
            int i = 0;
            int e = 0;

            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            Subset[] subsets = new Subset[verticesCount];

            for (int v = 0; v < verticesCount; ++v)
            {
                subsets[v].Parent = v;
                subsets[v].Rank = 0;
            }

            while (e < verticesCount - 1)
            {
                Edge nextEdge = edges[i++];
                int x = Find(subsets, nextEdge.Source.Index);
                int y = Find(subsets, nextEdge.Destination.Index);

                if (x != y)
                {
                    result.Add(nextEdge);
                    Union(subsets, x, y);
                    e++;
                }
            }
            //Print(result);
            return result;
        }

        // mai jos este exact acelasi lucru (graful MST construit este fix in fix ca cel de sus)
        static int findSet(int i)
        {
            if (i == parent[i])
                return i;
            else
                return findSet(parent[i]);
        }

        static void unionSet(int u, int v)
        {
            parent[u] = parent[v];
        }

        public static List<Edge> MST2(List<Edge> edges, int verticesCount)
        {
            parent = new int[verticesCount];
            for (int i = 0; i < verticesCount; i++)
                parent[i] = i;
            List<Edge> mst = new List<Edge>();
            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            for (int i = 0; i < edges.Count; i++)
            {
                int uRep = findSet(edges[i].Source.Index);
                int vRep = findSet(edges[i].Destination.Index);
                if (uRep != vRep)
                {
                    mst.Add(edges[i]);
                    unionSet(uRep, vRep);
                }
            }
            Print(mst);
            return mst;
        }
    }
}