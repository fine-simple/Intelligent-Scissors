using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelligentScissors
{
    
    class Graph
    {
        static int CONNECTIVITY = 4;
        static double WEIGHT_UPPER_LIMIT = 1e16;
        public static List<KeyValuePair<int, double>>[] adj;
        static int vertexCount;
        static int imageWidth, imageHeight;
        public static void Init(RGBPixel[,] ImageMatrix, int connectivity)
        {
            CONNECTIVITY = connectivity;

            imageHeight = ImageMatrix.GetLength(0);
            imageWidth = ImageMatrix.GetLength(1);

            vertexCount = imageWidth * imageHeight;
            
            adj = new List<KeyValuePair<int, double>>[vertexCount];

            constructGraph(ImageMatrix);
        }

        private static void constructGraph(RGBPixel[,] ImageMatrix)
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
        private static void addEnergyFor(int row, int col, Vector2D pixelEnergies)
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
        private static void addEnergyToNeighbour(int mainPixelIndex, int neighbourIndex, double energy)
        {
            if (adj[neighbourIndex] == null)
                adj[neighbourIndex] = new List<KeyValuePair<int, double>>();

            adj[neighbourIndex].Add(new KeyValuePair<int, double>(mainPixelIndex, energy));

            adj[mainPixelIndex].Add(new KeyValuePair<int, double>(neighbourIndex, energy));
        }

        private static int convert2DIndexTo1D(int row, int col)
        {
            return row * imageWidth + col;
        }

        private static bool validIndex(int row, int col)
        {
            bool lowerBound = (row >= 0 && col >= 0);
            bool upperBound = (row < imageHeight && col < imageWidth);
            return lowerBound && upperBound;
        }

        #endregion

    }
}
