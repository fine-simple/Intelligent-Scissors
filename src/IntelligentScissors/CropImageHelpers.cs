using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace IntelligentScissors
{
    static class CropImageHelpers
    {
        private static HashSet<KeyValuePair<int, int> > getFullPath(List<Point> lasso, Dictionary<Point, List<Point>> AnchorPaths)
        {
            HashSet<KeyValuePair<int, int> > onPath = new HashSet<KeyValuePair<int, int> >();
            AnchorPaths[lasso[lasso.Count - 1]] = ShortestPathHelpers.GetShortestPath(lasso[lasso.Count - 1], lasso[0], Graph.adj);
            foreach (KeyValuePair<Point, List<Point>> anchor in AnchorPaths)
            {
                foreach (Point pathPoint in anchor.Value)// paths are from src to dest (anchor to anchor) so no need to add key to dictionary
                {
                    onPath.Add(new KeyValuePair<int, int>(pathPoint.X, pathPoint.Y));
                }
            }

            return onPath;
        }
        public static KeyValuePair<int, int>[] getBoundaries(Dictionary<Point, List<Point>> AnchorPaths)
        {
            int yMin = int.MaxValue;
            int yMax = -1;
            int xMin = int.MaxValue;
            int xMax = -1;

            foreach (KeyValuePair<Point, List<Point>> anchor in AnchorPaths)
            {
                foreach (Point pathPoint in anchor.Value)
                {
                    yMin = Math.Min(yMin, pathPoint.Y);
                    yMax = Math.Max(yMax, pathPoint.Y);
                    xMin = Math.Min(xMin, pathPoint.X);
                    xMax = Math.Max(xMax, pathPoint.X);
                }
            }

            KeyValuePair<int, int>[] boundaries = {
                new KeyValuePair<int, int>(xMin, yMin),
                new KeyValuePair<int, int>(xMax, yMax),
            };

            return boundaries;
        }
        private static HashSet<KeyValuePair<int, int>> BFS(KeyValuePair<int, int> start, HashSet<KeyValuePair<int, int>> onPath, KeyValuePair<int, int>[] boundaries)
        {
            // pixel x is adjacent to pixel y if x is NOT on the lasso path
            // this way, all visited nodes are outside the cropped shape
            /*
             * FIXME: currently only draws path
             */
            Queue<KeyValuePair<int, int>> queue = new Queue<KeyValuePair<int, int>>();
            HashSet<KeyValuePair<int, int>> visited = new HashSet<KeyValuePair<int, int>>();

            queue.Enqueue(start);

            while(queue.Count!=0)
            {
                KeyValuePair<int, int> currNode = queue.Dequeue();

                if (visited.Contains(currNode))
                    continue;
                visited.Add(currNode);
                int currNodeIndex = Graph.convert2DIndexTo1D(currNode.Key, currNode.Value);
                foreach(KeyValuePair<int, double> adjNode in Graph.adj[currNodeIndex])
                {
                    KeyValuePair<int, int> Node = Graph.convert1DIndexTo2D(adjNode.Key);
                    if (!onPath.Contains(Node))
                        queue.Enqueue(Node);
                }
            }

            return visited;
        }
        private static Boolean withinBoundaries(KeyValuePair<int, int>[] boundaries, KeyValuePair<int, int> point)
        {
            int xMin = boundaries[0].Key;
            int yMin = boundaries[0].Value;
            int xMax = boundaries[1].Key;
            int yMax = boundaries[1].Value;

            Boolean withinMaxXandMinX = point.Key >= xMin && point.Key <= xMax;
            Boolean withinMaxYandMinY = point.Value >= yMin && point.Value <= yMax;

            return withinMaxXandMinX && withinMaxYandMinY;
        }
        private static Bitmap getTransparentBackground(Bitmap originalBmp, HashSet<KeyValuePair<int, int> >background)
        {
            Bitmap b = new Bitmap(originalBmp.Width, originalBmp.Height);

            for (int i = 0; i < originalBmp.Width; i++)
            {
                for (int j = 0; j < originalBmp.Height; j++)
                {
                    if (background.Contains(new KeyValuePair<int, int>(i, j)))
                        b.SetPixel(i, j, Color.Transparent);
                    else
                        b.SetPixel(i,j, originalBmp.GetPixel(i, j));
                }
            }

            return b;
        }
        public static Bitmap CropImage(Bitmap bmp, List<Point> lasso, Dictionary<Point, List<Point>> AnchorPaths)
        {
            HashSet<KeyValuePair<int, int>> lassoPath = getFullPath(lasso, AnchorPaths);
            KeyValuePair<int, int>[] boundaries = getBoundaries(AnchorPaths);
            HashSet<KeyValuePair<int, int>> background = BFS(new KeyValuePair<int, int>(0, 0), lassoPath, boundaries);
            Bitmap finalImage = getTransparentBackground(bmp, background);

            return finalImage;
        }
    }
}
