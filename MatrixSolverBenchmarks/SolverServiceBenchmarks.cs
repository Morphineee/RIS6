using System;
using BenchmarkDotNet.Attributes;
using MatrixSolverServer;
using static MatrixSolverServer.Program;

namespace MatrixSolver.Benchmarks
{
    [MemoryDiagnoser] // Добавляет диагностику памяти
    public class SolverServiceBenchmarks
    {
        private readonly ISolverService _solverService;

        public SolverServiceBenchmarks()
        {
            _solverService = new SolverService();
        }

        [Params(100, 500, 1000)] // Размеры матриц для тестирования
        public int Size;

        [Benchmark]
        public void SolveLargeSLAU()
        {
            // Создаём большую матрицу и вектор
            double[][] matrix = new double[Size][];
            double[] vector = new double[Size];

            for (int i = 0; i < Size; i++)
            {
                matrix[i] = new double[Size];
                for (int j = 0; j < Size; j++)
                {
                    matrix[i][j] = (i == j) ? 2 : 1; // Диагональная матрица
                }
                vector[i] = 1;
            }

            // Выполняем решение
            _solverService.SolveSLAUWithLDL(matrix, vector);
        }
    }
}
