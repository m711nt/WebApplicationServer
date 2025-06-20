using System.Runtime.CompilerServices;
using System.Threading.Channels;
using WebApplicationMatrix.Contract;

namespace WebApplicationMatrix.Service;

public class ControllerClient : BackgroundService
{
    private readonly IControllerService _controllerService;
    private readonly ILogger<ControllerClient> _logger;
    private readonly string _serverAddress;
    private readonly Channel<MathTask> _taskChannel = Channel.CreateUnbounded<MathTask>();

    public ControllerClient(IControllerService controllerService, ILogger<ControllerClient> logger)
    {
        _controllerService = controllerService;
        _logger = logger;
        _serverAddress = "https://localhost:7159";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation($"Подключение к серверу {_serverAddress}...");
                await StartBidirectionalCommunication(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подключения к серверу: {Message}", ex.Message);
                _logger.LogInformation("Повторная попытка через 5 секунд...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task StartBidirectionalCommunication(CancellationToken stoppingToken)
    {
        var clientResults = GenerateResults(stoppingToken);
        await foreach (var task in _controllerService.StreamMessagesAsync(clientResults, default))
        {
            _logger.LogInformation($"Получена задача от сервера: {task.Number1} {task.Operation} {task.Number2}");
            await _taskChannel.Writer.WriteAsync(task, stoppingToken);
        }
    }

    private async IAsyncEnumerable<MathResult> GenerateResults([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            MathTask task;
            try
            {
                task = await _taskChannel.Reader.ReadAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении задачи");
                continue;
            }

            var result = CalculateResult(task);
            _logger.LogInformation($"Отправка ответа: {result}");
            yield return new MathResult
            {
                Result = result,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    private double CalculateResult(MathTask task)
    {
        return task.Operation switch
        {
            "+" => task.Number1 + task.Number2,
            "-" => task.Number1 - task.Number2,
            "*" => task.Number1 * task.Number2,
            "/" => task.Number1 / task.Number2,
            _ => throw new InvalidOperationException($"Неизвестная операция: {task.Operation}")
        };
    }
}