using System.Runtime.CompilerServices;
using WebApplicationMatrix.Contract;
using ProtoBuf.Grpc;

namespace WebApplicationMatrix.Service;

public class ControllerGrpcService : IControllerService
{
    private readonly ILogger<ControllerGrpcService> _logger;
    private readonly Random _random = new();
    private readonly string[] _operations = { "+", "-", "*", "/" };

    public ControllerGrpcService(ILogger<ControllerGrpcService> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<MathTask> StreamMessagesAsync(IAsyncEnumerable<MathResult> clientResults, [EnumeratorCancellation] CallContext context = default)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var task = GenerateMathTask();
            _logger.LogInformation($"Отправка задачи клиенту: {task.Number1} {task.Operation} {task.Number2}");

            yield return task;

            await foreach (var result in clientResults.WithCancellation(context.CancellationToken))
            {
                var expectedResult = CalculateExpectedResult(task);
                _logger.LogInformation($"Получен ответ от клиента: {result.Result} (ожидалось: {expectedResult})");
                if (Math.Abs(result.Result - expectedResult) < 0.0001)
                {
                    _logger.LogInformation("Ответ верный!");
                }
                else
                {
                    _logger.LogWarning("Ответ неверный!");
                }
                break;
            }

            await Task.Delay(1000, context.CancellationToken);
        }
    }

    private MathTask GenerateMathTask()
    {
        var operation = _operations[_random.Next(_operations.Length)];
        double number1, number2;

        if (operation == "/")
        {
            number1 = _random.Next(1, 100);
            number2 = _random.Next(1, 100);
        }
        else
        {
            number1 = Math.Round(_random.NextDouble() * 100, 2);
            number2 = Math.Round(_random.NextDouble() * 100, 2);
        }

        return new MathTask
        {
            Number1 = number1,
            Number2 = number2,
            Operation = operation,
            Timestamp = DateTime.UtcNow
        };
    }

    private double CalculateExpectedResult(MathTask task)
    {
        return task.Operation switch
        {
            "+" => task.Number1 + task.Number2,
            "-" => task.Number1 - task.Number2,
            "*" => task.Number1 * task.Number2,
            "/" => task.Number1 / task.Number2,
            _ => throw new ArgumentException($"Неизвестная операция: {task.Operation}")
        };
    }
}