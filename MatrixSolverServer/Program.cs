using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MatrixSolverServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<ISolverService, SolverService>();
            builder.Services.AddSingleton<ITaskQueue>(sp => new TaskQueue(5)); // Регистрация с использованием фабричного метода

            var app = builder.Build();

            // Добавляем использование файлов по умолчанию и статических файлов
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Добавляем маршрут для обработки POST-запросов
            app.MapPost("/solve", SolveHandler);

            string serverUrl = "http://localhost:5145";
            Console.WriteLine($"Сервер запущен по адресу: {serverUrl}");
            app.Run(serverUrl);
        }

        private static async Task SolveHandler(HttpContext context)
        {
            if (!context.Request.HasFormContentType || !context.Request.Form.Files.Any(f => f.Name == "csvFile"))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Некорректный запрос.");
                return;
            }

            var file = context.Request.Form.Files["csvFile"];
            if (file.Length == 0)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Пустой файл.");
                return;
            }

            string fileContent;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            try
            {
                var (matrix, vector) = ParseCsv(fileContent);

                var solverService = context.RequestServices.GetRequiredService<ISolverService>();
                var taskQueue = context.RequestServices.GetRequiredService<ITaskQueue>();

                await taskQueue.Enqueue(async () =>
                {
                    var solution = solverService.SolveSLAUWithLDL(matrix, vector);

                    var response = new
                    {
                        Solution = solution
                    };
                    var responseJson = JsonSerializer.Serialize(response);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(responseJson);
                });
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ошибка при обработке запроса.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Внутренняя ошибка сервера.");
            }
        }

        private static (double[][], double[]) ParseCsv(string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var matrix = new double[lines.Length][];
            var vector = new double[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                var values = lines[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != lines.Length + 1)
                {
                    throw new ArgumentException("Matrix must be square with an additional vector column.");
                }

                matrix[i] = new double[values.Length - 1];

                for (int j = 0; j < values.Length - 1; j++)
                {
                    matrix[i][j] = double.Parse(values[j], CultureInfo.InvariantCulture);
                }

                vector[i] = double.Parse(values[values.Length - 1], CultureInfo.InvariantCulture);
            }

            return (matrix, vector);
        }

        public interface ISolverService
        {
            double[] SolveSLAUWithLDL(double[][] matrix, double[] vector);
        }

        public class SolverService : ISolverService
        {
            public double[] SolveSLAUWithLDL(double[][] matrix, double[] vector)
            {
                ValidateMatrix(matrix, vector);

                int n = matrix.Length;
                double[] solution = new double[n];
                double[] d = new double[n];
                double[][] l = new double[n][];

                for (int i = 0; i < n; i++)
                {
                    l[i] = new double[i];
                }

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        double sum = matrix[i][j];
                        for (int k = 0; k < j; k++)
                        {
                            sum -= l[i][k] * l[j][k] * d[k];
                        }
                        l[i][j] = sum / d[j];
                    }

                    double sumDiag = matrix[i][i];
                    for (int k = 0; k < i; k++)
                    {
                        sumDiag -= l[i][k] * l[i][k] * d[k];
                    }
                    d[i] = sumDiag;
                }

                // Решение Ly = b
                double[] y = new double[n];
                for (int i = 0; i < n; i++)
                {
                    double sum = vector[i];
                    for (int j = 0; j < i; j++)
                    {
                        sum -= l[i][j] * y[j];
                    }
                    y[i] = sum;
                }

                // Решение DL^T x = y
                for (int i = 0; i < n; i++)
                {
                    y[i] /= d[i];
                }

                for (int i = n - 1; i >= 0; i--)
                {
                    double sum = y[i];
                    for (int j = i + 1; j < n; j++)
                    {
                        sum -= l[j][i] * solution[j];
                    }
                    solution[i] = sum;
                }

                return solution;
            }

            private void ValidateMatrix(double[][] matrix, double[] vector)
            {
                if (matrix == null || vector == null)
                {
                    throw new ArgumentNullException("Matrix and vector cannot be null.");
                }

                int n = matrix.Length;
                if (n == 0 || vector.Length != n)
                {
                    throw new ArgumentException("Matrix and vector dimensions do not match or matrix is empty.");
                }

                foreach (var row in matrix)
                {
                    if (row.Length != n)
                    {
                        throw new ArgumentException("Matrix must be square.");
                    }
                }
            }
        }

        public interface ITaskQueue
        {
            Task Enqueue(Func<Task> taskFunc);
        }

        public class TaskQueue : ITaskQueue
        {
            private readonly SemaphoreSlim _semaphore;
            private readonly ConcurrentQueue<Func<Task>> _queue;

            public TaskQueue(int maxConcurrency)
            {
                _semaphore = new SemaphoreSlim(maxConcurrency);
                _queue = new ConcurrentQueue<Func<Task>>();
            }

            public async Task Enqueue(Func<Task> taskFunc)
            {
                _queue.Enqueue(taskFunc);

                await _semaphore.WaitAsync();

                if (_queue.TryDequeue(out var taskToExecute))
                {
                    try
                    {
                        await taskToExecute();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }
        }
    }
}
