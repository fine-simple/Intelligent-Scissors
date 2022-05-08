using System.Drawing;

namespace IntelligentScissors
{
    public static class DrawHelpers
    {
        public static float scaleFactor, filler;
        public static bool horizontalScaled = false;
        public static Rectangle getAnchorRect(Point mousePos)
        {
            int size = 5;
            return new Rectangle((int)(mousePos.X - 0.5 * size), (int)(mousePos.Y - 0.5 * size), size, size);
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
    }
}
