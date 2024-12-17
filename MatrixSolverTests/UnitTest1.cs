using System;
using Xunit;
using MatrixSolverServer;
using static MatrixSolverServer.Program;

namespace MatrixSolver.Tests
{
    public class SolverServiceTests
    {
        private readonly ISolverService _solverService;

        public SolverServiceTests()
        {
            _solverService = new SolverService();
        }

        [Fact]
        public void SolveSLAUWithLDL_ShouldReturnCorrectSolution_ForValidInput()
        {
            // Arrange
            double[][] matrix =
            {
        new double[] { 4, 1, 2 },
        new double[] { 1, 3, 1 },
        new double[] { 2, 1, 5 }
    };
            double[] vector = { 7, 8, 10 };

            double[] expectedSolution = { 1, 2, 1 }; // Пример решения

            // Act
            double[] solution = _solverService.SolveSLAUWithLDL(matrix, vector);

            // Assert
            Assert.Equal(expectedSolution.Length, solution.Length); // Проверяем, что размеры совпадают

            for (int i = 0; i < expectedSolution.Length; i++)
            {
                Assert.True(Math.Abs(expectedSolution[i] - solution[i]) < 1e-5,
                    $"Элемент {i} не совпадает. Ожидалось {expectedSolution[i]}, но было {solution[i]}");
            }
        }


        [Fact]
        public void SolveSLAUWithLDL_ShouldThrowException_ForNonSquareMatrix()
        {
            // Arrange
            double[][] matrix =
            {
                new double[] { 1, 2 },
                new double[] { 3, 4, 5 }
            };
            double[] vector = { 1, 1 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _solverService.SolveSLAUWithLDL(matrix, vector));
        }

        [Fact]
        public void SolveSLAUWithLDL_ShouldThrowException_WhenMatrixIsNull()
        {
            // Arrange
            double[][] matrix = null;
            double[] vector = { 1, 2 };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _solverService.SolveSLAUWithLDL(matrix, vector));
        }
    }
}
