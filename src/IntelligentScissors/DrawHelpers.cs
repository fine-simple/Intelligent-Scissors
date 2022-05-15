using System;
using System.Drawing;

namespace IntelligentScissors
{
    public static class DrawHelpers
    {
        private static float scaleFactor = 1, filler = 0;
        private static bool horizontalScaled = false;
        public static Rectangle getAnchorRect(Point mousePos)
        {
            int size = 5;
            return new Rectangle((int)(mousePos.X - 0.5 * size), (int)(mousePos.Y - 0.5 * size), size, size);
        }
        public static void applyScaling(int wI, int hI, int wB, int hB)
        {
            float imageRatio = wI / (float)hI;
            float boxRatio = wB / (float)hB;
            if (imageRatio >= boxRatio)
            {
                horizontalScaled = true;
                scaleFactor = wB / (float)wI;
                float scaledSize = hI * DrawHelpers.scaleFactor;
                filler = Math.Abs(hB - scaledSize) / 2;
            }
            else
            {
                scaleFactor = hB / (float)hI;
                float scaledSize = wI * DrawHelpers.scaleFactor;
                filler = Math.Abs(wB - scaledSize) / 2;
            }
        }
        public static void removeScaling()
        {
            scaleFactor = 1;
            filler = 0;
        }
        public static Point unscaledPos(Point p)
        {
            Point unscaledP = new Point();
            if (horizontalScaled)
            {
                unscaledP.X = (int)(p.X / scaleFactor);
                unscaledP.Y = (int)((p.Y - filler) / scaleFactor);
            }
            else
            {
                unscaledP.X = (int)((p.X - filler) / scaleFactor);
                unscaledP.Y = (int)(p.Y / scaleFactor);
            }
            return unscaledP;
        }
        public static Point scaledPos(Point p)
        {
            Point scaledP = new Point();
            if(horizontalScaled)
            {
                scaledP.X = (int)(p.X * scaleFactor);
                scaledP.Y = (int)(p.Y * scaleFactor + filler);
            }
            else
            {
                scaledP.X = (int)(p.X * scaleFactor + filler);
                scaledP.Y = (int)(p.Y * scaleFactor);
            }    
            return scaledP;
        }
        public static double eucledianDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        public static int manhattanDistance(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }
    }
}
