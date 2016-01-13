using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Helpers
{
    public static class ResultCalculationMetricsHelpers
    {
        public static void CalculatePRF(int truePositives, int falsePositive, int falseNegatives, out double precision, out double recall, out double fScore)
        {
            precision = (double)truePositives / ((double)truePositives + (double)falsePositive);
            recall = (double)truePositives / ((double)truePositives + (double)falseNegatives);
            fScore = 2 * (double)precision * (double)recall / ((double)precision + (double)recall);
        }

        public static void PrintMatrix(int[][] matrix, bool printClassLabels)
        {
            Console.Write(string.Format("{0,7}", string.Empty));
            for (int j = 0; j < matrix[0].Length; j++)
            {
                Console.Write(string.Format("{0,7}", j));
            }

            Console.WriteLine();
            for (int i = 0; i < matrix.Length; i++)
            {
                Console.Write(string.Format("{0,7}", i));
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write(string.Format("{0,7}", matrix[i][j]));
                }
                Console.WriteLine();
            }
        }

        public static int[][] BuildConfusionMatrix(double[] problemYEval, List<double> predictedY, int numberOfLabels)
        {
            List<int[]> list = new List<int[]>();
            for (int i = 0; i < numberOfLabels; i++)
            {
                list.Add(new int[numberOfLabels]);
            }

            int[][] matrix = list.ToArray();

            for (int i = 0; i < problemYEval.Length; i++)
            {
                var expectedClassLabel = (int)problemYEval[i];
                var predictedClassLabel = (int)predictedY[i];

                matrix[expectedClassLabel][predictedClassLabel] = matrix[expectedClassLabel][predictedClassLabel] + 1;
            }
            return matrix;
        }
    }
}
