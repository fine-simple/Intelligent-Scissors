using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph.Collections;
using System.Drawing;

namespace IntelligentScissors
{
    public static class ShortestPathHelpers
    {
        
        private static int[] Dijkstra(int src, int dest, List<KeyValuePair<int, double>>[] adj)
        {

            double[] dist = Enumerable.Repeat(double.MaxValue, adj.Count()).ToArray();
            int [] parent = Enumerable.Repeat(-1, adj.Count()).ToArray();

            /* Using Fibonacci Heaps for a priority queue to improve asymptotic running time
             * Priority --> least cost
             * Value --> Node
             */

            FibonacciHeap<double, int> PriorityQueue = new FibonacciHeap<double, int>();

            PriorityQueue.Enqueue(0, src);
            dist[src] = 0;

            while(!PriorityQueue.IsEmpty)
            {
                int currentNode = PriorityQueue.Top.Value;
                double currentCost = PriorityQueue.Top.Priority;

                if (currentNode == dest)
                    break;

                PriorityQueue.Dequeue();

                if (currentCost > dist[currentNode])
                    continue;

                foreach(KeyValuePair<int, double> adjNode in adj[currentNode])
                {
                    int Child = adjNode.Key;
                    double newCost = adjNode.Value + currentCost;

                    if(newCost<dist[Child])
                    {
                        dist[Child] = newCost;
                        parent[Child] = currentNode;
                        PriorityQueue.Enqueue(newCost, Child);
                    }
                }
            }
            
            return parent;
        }

        // Backtracking to get shortest path from source to destination
        private static List<int> GetPath(int[] parent, int Dest)
        {
            List<int> Path = new List<int>();
            int currentNode = Dest;

            while(parent[currentNode]!=-1)
            {
                Path.Add(parent[currentNode]);
                currentNode = parent[currentNode];
            }

            Path.Reverse();

            return Path;
        }

        public static List<Point> GetShortestPath(Point srcPoint, Point destPoint, List<KeyValuePair<int, double>>[] adj)
        {
            int src = Graph.convert2DIndexTo1D(srcPoint.Y, srcPoint.X);
            int dest = Graph.convert2DIndexTo1D(destPoint.Y, destPoint.X);

            int[] parent = Dijkstra(src, dest, adj);
            List<int> ShortestPath = GetPath(parent, dest);

            List<Point> ShortestPathPoints = new List<Point>();

            foreach(int node in ShortestPath)
            {
                KeyValuePair<int, int> point = Graph.convert1DIndexTo2D(node);
                Point nodePoint = DrawHelpers.scaledPos(new Point(point.Value, point.Key));

                ShortestPathPoints.Add(nodePoint);
            }

            return ShortestPathPoints;
        }
    }
}
