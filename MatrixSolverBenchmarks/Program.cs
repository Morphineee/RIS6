using System;
using BenchmarkDotNet.Running;

namespace MatrixSolver.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Запуск бенчмарков
            BenchmarkRunner.Run<SolverServiceBenchmarks>();

            // Удержание консоли открытой после выполнения тестов
            Console.WriteLine("Тесты завершены. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
