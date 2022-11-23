using System;

namespace Graph
{
    public class Prim // nefunctional
    {
        private static int MinKey(double[] key, bool[] mstSet, int verticesCount)
        {
            double min = double.MaxValue;
            int min_index = -1;

            for (int v = 0; v < verticesCount; v++)
            {
                if (mstSet[v] == false && key[v] < min)
                {
                    min = key[v];
                    min_index = v;
                }
            }
            return min_index;
        }

        public static int[] MST(double[,] graph, int verticesCount)
        {
            int[] parent = new int[verticesCount];
            double[] key = new double[verticesCount];
            bool[] mstSet = new bool[verticesCount];

            for (int i = 0; i < verticesCount; i++)
            {
                key[i] = double.MaxValue;
                mstSet[i] = false;
            }

            key[0] = 0;
            parent[0] = -1;

            for (int count = 0; count < verticesCount - 1; count++)
            {
                int u = MinKey(key, mstSet, verticesCount);
                mstSet[u] = true;

                for (int v = 0; v < verticesCount; v++)
                {
                    if (graph[u, v] != 0 && mstSet[v] == false && graph[u, v] < key[v])
                    {
                        parent[v] = u;
                        key[v] = graph[u, v];
                    }
                }
            }

            return parent;
        }
    }
}