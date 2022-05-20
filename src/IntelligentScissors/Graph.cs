using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IntelligentScissors
{
    
    class Graph
    {
        static int CONNECTIVITY = 4;
        static double WEIGHT_UPPER_LIMIT = 1e16;
        public static List<KeyValuePair<int, double>>[] adj;
        static int vertexCount;
        static int imageWidth, imageHeight;
        public enum Neighbour
        {
            Top, Right, Left, Bot
        }

        public static void Init(RGBPixel[,] ImageMatrix, int connectivity)
        {
            CONNECTIVITY = connectivity;

            imageHeight = ImageMatrix.GetLength(0);
            imageWidth = ImageMatrix.GetLength(1);

            vertexCount = imageWidth * imageHeight;
            
            adj = new List<KeyValuePair<int, double>>[vertexCount] // O(N^2)

            constructGraph(ImageMatrix);
        }

        private static void constructGraph(RGBPixel[,] ImageMatrix) // O(N^2)
        {
            for (int row = 0; row < imageHeight; row++)
            {
                for (int col = 0; col < imageWidth; col++)
                {
                    Vector2D pixelEnergies = ImageOperations.CalculatePixelEnergies(col, row, ImageMatrix);

                    addEnergyFor(row, col, pixelEnergies);
                }
            }
        }


        #region Helper Functions
        
        

        /* the function adds the energy of current pixel to the neighbour pixel
         * and vice versa to establish the 4 way connectivity
         */
        private static void addEnergyFor(int row, int col, Vector2D pixelEnergies) // O(1)
        {
            double rightPixelEnergy = pixelEnergies.X == 0? WEIGHT_UPPER_LIMIT: 1 / pixelEnergies.X;
            double botPixelEnergy = pixelEnergies.Y == 0 ? WEIGHT_UPPER_LIMIT : 1 / pixelEnergies.Y;

            int mainPixelIndex = convert2DIndexTo1D(row, col);

            if (adj[mainPixelIndex] == null)
                adj[mainPixelIndex] = new List<KeyValuePair<int, double>>();

            //add to right pixel
            if (validIndex(row, col + 1))
            {
                int rightPixelIndex = convert2DIndexTo1D(row, col + 1);
                addEnergyToNeighbour(mainPixelIndex, rightPixelIndex, rightPixelEnergy);
            }

            //add to bottom pixel
            if (validIndex(row + 1, col))
            {
                int botPixelIndex = convert2DIndexTo1D(row + 1, col);
                addEnergyToNeighbour(mainPixelIndex, botPixelIndex, botPixelEnergy);
            }
            
        }
        private static void addEnergyToNeighbour(int mainPixelIndex, int neighbourIndex, double energy) // O(1)
        {
            if (adj[neighbourIndex] == null)
                adj[neighbourIndex] = new List<KeyValuePair<int, double>>();

            adj[neighbourIndex].Add(new KeyValuePair<int, double>(mainPixelIndex, energy));

            adj[mainPixelIndex].Add(new KeyValuePair<int, double>(neighbourIndex, energy));
        }

        public static int convert2DIndexTo1D(int row, int col)
        {
            return row * imageWidth + col;
        }

        public static Point convert1DIndexTo2D(int index)
        {
            int col = index % imageWidth;
            int row = (index-col) / imageWidth;

            return new Point(col, row);
        }

        public static bool validIndex(int row, int col)
        {
            bool lowerBound = (row >= 0 && col >= 0);
            bool upperBound = (row < imageHeight && col < imageWidth);
            return lowerBound && upperBound;
        }

        public static double getNeighbourWeight(int mainRow, int mainCol, Neighbour neighbourType)
        {

            Point neighbour = getNeighbourLocation(mainRow, mainCol, neighbourType);

            int indx = convert2DIndexTo1D(mainRow, mainCol);
            int neighbourIndx = convert2DIndexTo1D(neighbour.Y, neighbour.X);
            for (int i = 0; i < adj[indx].Count; i++)
            {
                if (adj[indx][i].Key == neighbourIndx)
                    return adj[indx][i].Value;
            }
            return -1;
        }

        private static Point getNeighbourLocation(Point currentLocation, Neighbour neighbourType)
        {
            int targetRow = currentLocation.Y, targetCol = currentLocation.X;
            switch (neighbourType)
            {
                case Neighbour.Right:
                    targetCol++;
                    break;
                case Neighbour.Left:
                    targetCol--;
                    break;
                case Neighbour.Top:
                    targetRow--;
                    break;
                case Neighbour.Bot:
                    targetRow++;
                    break;

            }
            if (validIndex(targetRow, targetCol))
                return new Point(targetCol, targetRow);
            else
                throw new ArgumentException("Current location doesn't have requested neighbour type!");
        }
        private static Point getNeighbourLocation(int row, int col, Neighbour neighbourType)
        {
            int targetRow = row, targetCol = col;
            switch (neighbourType)
            {
                case Neighbour.Right:
                    targetCol++;
                    break;
                case Neighbour.Left:
                    targetCol--;
                    break;
                case Neighbour.Top:
                    targetRow--;
                    break;
                case Neighbour.Bot:
                    targetRow++;
                    break;

            }
            if (validIndex(targetRow, targetCol))
                return new Point(targetCol, targetRow);
            else
                throw new ArgumentException("Current location doesn't have requested neighbour type!");
        }


        #endregion

    }
}
