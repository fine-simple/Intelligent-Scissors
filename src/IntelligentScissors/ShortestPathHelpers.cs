﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using QuickGraph.Collections;

namespace IntelligentScissors
{
    public static class ShortestPathHelpers
    {
        private static int heightBoundary;
        private static int widthBoundary;

        private static int[] Dijkstra(int src, int dest, List<KeyValuePair<int, double>>[] adj)
        {

            double[] dist = new double[adj.Count()];
            int [] parent = new int[adj.Count()];

            Point source = Graph.convert1DIndexTo2D(src);

            for (int x = -widthBoundary; x < widthBoundary; x++)
            {
                for (int y = -heightBoundary; y < heightBoundary; y++)
                {
                    Point point = new Point(source.X + x, source.Y + y);
                    if (!Graph.validIndex(point.Y, point.X))
                        continue;

                    int ind = Graph.convert2DIndexTo1D(point.Y, point.X);
                    
                    dist[ind] = double.MaxValue;
                    parent[ind] = -1;
                }
            }
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

                    if(withinBounds(Child, src) && newCost<dist[Child])
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
                Point point = Graph.convert1DIndexTo2D(node);
                Point nodePoint = point;

                ShortestPathPoints.Add(nodePoint);
            }

            return ShortestPathPoints;
        }

        public static Boolean withinBounds(int node, int src)
        {
            Point point = Graph.convert1DIndexTo2D(node);
            Point source = Graph.convert1DIndexTo2D(src);
            Boolean withinWidth = (point.X <= source.X + widthBoundary) && (point.X >= source.X - widthBoundary);
            Boolean withinHeight = (point.Y <= source.Y + heightBoundary) && (point.Y >= source.Y - heightBoundary);
            return withinHeight && withinWidth;
        }

        public static void setBounds(int width, int height)
        {
            heightBoundary = height / 2;
            widthBoundary = width / 2;
        }
    }
}
