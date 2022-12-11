using System.Collections.Generic;

namespace Graph
{
    public class Kruskal
    {
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
    }
}