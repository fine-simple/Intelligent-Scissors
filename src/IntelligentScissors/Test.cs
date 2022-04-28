using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO; 


namespace IntelligentScissors
{
    class Test
    {
        static double EPS = 1;
        public enum SampleType
        {
            Sample1, Sample2, Sample3
        }
        public enum CompleteType
        {
            Complete1, Complete2
        }

        public class Graph
        {

 
            public static void sample(List<KeyValuePair<int, double>>[] graph, SampleType type)
            {
                string path = Cases.GetSamplePath(type);
                bool testPassed = true;

                using (StreamReader reader = new StreamReader(path))
                {
                    reader.ReadLine(); // skip header text
                    reader.ReadLine(); // skip empty line

                    
                    while(!reader.EndOfStream)
                    {
                        reader.ReadLine(); // skip node index line
                        reader.ReadLine(); // skip Edges line

                        string edgeLine = reader.ReadLine();
                        while(!edgeLine.Equals(String.Empty))
                        {
                            if (!verifyEdge(graph, edgeLine))
                                testPassed = false;

                            edgeLine = reader.ReadLine();
                        }

                        skipEmptyLines(reader); // skips 2 empty lines
                    }

                }
                if (testPassed)
                    Console.WriteLine("Congratulations! Sample Test " + type + " Passed Successfuly");
                else
                    Console.WriteLine("Test Failed!");
            }

            public static void complete(List<KeyValuePair<int, double>>[] graph, CompleteType type)
            {
                //TODO: implement complete testing
                throw new NotImplementedException();
            }

            private static bool verifyEdge(List<KeyValuePair<int, double>>[] graph, string edgeLine)
            {
                string[] edgeWords = edgeLine.Split(' ');

                int fromIndex = 4, toIndex = 8, weightIndex = edgeWords.Length - 1;

                int from = Convert.ToInt32(edgeWords[fromIndex]);
                int to = Convert.ToInt32(edgeWords[toIndex]);
                double weight;
                
                weight = Convert.ToDouble(edgeWords[weightIndex]);

                bool edgeVerified = false;
                for (int i = 0; i < 4; i++) // search the 4 edges of the vertex
                {
                    if (graph[from][i].Key == to)
                    {
                        edgeVerified = Math.Abs(graph[from][i].Value - weight) <= EPS;
                        break;
                    }
                }

                if (!edgeVerified)
                {
                    Console.WriteLine("Edge From " + from + " To " + to + " Doesn't exist or has wrong weight");
                    return false;
                }

                return true;
            }

            private static void skipEmptyLines(StreamReader reader)
            {
                reader.ReadLine();
                reader.ReadLine();
            }
        }




        #region Cases paths
        class Cases
        {
            private static string TESTS_PATH = "../../../../Testcases";
            private static string Sample1 = TESTS_PATH + "/Sample/Case1/output.txt";
            private static string Sample2 = TESTS_PATH + "/Sample/Case2/output2.txt";
            private static string Sample3 = TESTS_PATH + "/Sample/Case3/output.txt";

            private static string Complete1 = TESTS_PATH + "/Complete/Case1/output/output.txt";
            private static string Complete2 = TESTS_PATH + "/Complete/Case2/output2/output2.txt";

            public static string GetSamplePath(SampleType type)
            {
                switch (type)
                {
                    case SampleType.Sample1:
                        return Sample1;
                    case SampleType.Sample2:
                        return Sample2;
                    case SampleType.Sample3:
                        return Sample3;
                }
                return "";
            }

            public static string GetCompleteTestPath(CompleteType type)
            {
                switch (type)
                {
                    case CompleteType.Complete1:
                        return Complete1;
                    case CompleteType.Complete2:
                        return Complete2;
                }
                return "";
            }
        }
        #endregion

    }
}
