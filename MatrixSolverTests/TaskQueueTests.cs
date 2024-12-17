using System;
using System.Threading.Tasks;
using Xunit;
using MatrixSolverServer;

namespace MatrixSolver.Tests
{
    public class TaskQueueTests
    {
        [Fact]
        public async Task Enqueue_ShouldProcessTasksConcurrently()
        {
            // Arrange
            var taskQueue = new TaskQueue(2);
            int counter = 0;

            Task IncrementCounter() => Task.Run(() =>
            {
                Task.Delay(100).Wait();
                counter++;
            });

            // Act
            await Task.WhenAll(
                taskQueue.Enqueue(IncrementCounter),
                taskQueue.Enqueue(IncrementCounter),
                taskQueue.Enqueue(IncrementCounter)
            );

            // Assert
            Assert.Equal(3, counter);
        }

        [Fact]
        public async Task Enqueue_ShouldThrottleTasks_BasedOnConcurrencyLimit()
        {
            // Arrange
            var taskQueue = new TaskQueue(1);
            int concurrentTasks = 0;
            int maxConcurrentTasks = 0;

            Task TestTask() => Task.Run(async () =>
            {
                concurrentTasks++;
                maxConcurrentTasks = Math.Max(maxConcurrentTasks, concurrentTasks);
                await Task.Delay(100);
                concurrentTasks--;
            });

            // Act
            await Task.WhenAll(
                taskQueue.Enqueue(TestTask),
                taskQueue.Enqueue(TestTask),
                taskQueue.Enqueue(TestTask)
            );

            // Assert
            Assert.Equal(1, maxConcurrentTasks); // Ограничение в 1
        }
    }
}
