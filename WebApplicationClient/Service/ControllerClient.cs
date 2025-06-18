using WebApplicationMatrix.Contract;
using Grpc.Core;

namespace WebApplicationMatrix.Service;

public class ControllerClient(IControllerService client, ILogger<ControllerClient> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Начинаем получение IP-адресов контроллеров...");

                await foreach (var controller in client.StreamControllersAsync(new CallOptions(cancellationToken: stoppingToken)))
                {
                    logger.LogInformation($"Получен контроллер: {controller.ControllerName} - {controller.IpAddress}");
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
            {
                logger.LogError($"Ошибка подключения к серверу: {ex.Status.Detail}");
                logger.LogInformation("Повторная попытка через 5 секунд...");
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при получении контроллеров: {ex.Message}");
                if (ex.InnerException != null)
                {
                    logger.LogError($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
                logger.LogInformation("Повторная попытка через 5 секунд...");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}