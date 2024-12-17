//using System;
//using System.Diagnostics;
//using System.Net.WebSockets;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;

//namespace LoadTesting
//{
//    class Program
//    {
//        static async Task Main(string[] args)
//        {
//            Console.WriteLine("Нагрузочное тестирование сервера начато.");

//            // Настройки теста
//            int[] matrixSizes = { 10, 50, 100 }; // Размеры матриц
//            int parallelRequests = 300; // Одновременные запросы

//            foreach (var size in matrixSizes)
//            {
//                Console.WriteLine($"\nТестирование с матрицей {size}x{size}");

//                // Генерация данных
//                var matrix = MatrixGenerator.GenerateMatrix(size);
//                var vector = MatrixGenerator.GenerateVector(size);

//                // Замер времени
//                var stopwatch = Stopwatch.StartNew();
//                var tasks = new Task[parallelRequests];

//                for (int i = 0; i < parallelRequests; i++)
//                {
//                    tasks[i] = SendRequestAsync(matrix, vector);
//                }

//                await Task.WhenAll(tasks);

//                stopwatch.Stop();
//                Console.WriteLine($"Время обработки {parallelRequests} запросов: {stopwatch.ElapsedMilliseconds} мс");
//            }

//            Console.ReadKey();
//        }

//        private static async Task SendRequestAsync(double[][] matrix, double[] vector)
//        {
//            string serverAddress = "ws://localhost:5145/ws";

//            try
//            {
//                using var webSocket = new ClientWebSocket();
//                await webSocket.ConnectAsync(new Uri(serverAddress), CancellationToken.None);

//                var request = new { Matrix = matrix, Vector = vector };
//                string jsonRequest = JsonSerializer.Serialize(request);

//                var requestBytes = Encoding.UTF8.GetBytes(jsonRequest);
//                await webSocket.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);

//                var buffer = new byte[4096];
//                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

//                string response = Encoding.UTF8.GetString(buffer, 0, result.Count);
//                Console.WriteLine($"Ответ сервера: {response}");

//                // Корректное закрытие соединения
//                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка запроса: {ex.Message}");
//            }
//        }

//    }
//}
