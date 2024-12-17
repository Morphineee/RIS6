//using System;

//namespace LoadTesting
//{
//    public static class MatrixGenerator
//    {
//        public static double[][] GenerateMatrix(int size)
//        {
//            var random = new Random();
//            var matrix = new double[size][];
//            for (int i = 0; i < size; i++)
//            {
//                matrix[i] = new double[size];
//                for (int j = 0; j < size; j++)
//                {
//                    matrix[i][j] = random.NextDouble() * 10; // Значения от 0 до 10
//                }
//            }
//            return matrix;
//        }

//        public static double[] GenerateVector(int size)
//        {
//            var random = new Random();
//            var vector = new double[size];
//            for (int i = 0; i < size; i++)
//            {
//                vector[i] = random.NextDouble() * 10;
//            }
//            return vector;
//        }
//    }
//}
