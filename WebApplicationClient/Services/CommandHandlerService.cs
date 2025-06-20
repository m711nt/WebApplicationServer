using SharedContract;

namespace WebApplicationClient.Services;

/// <summary>
/// Сервис для запуска двунаправленного обмена между контроллером и клиентом
/// </summary>
/// <param name="controllerService"></param>
internal class CommandHandlerService(IControllerService controllerService, CommandToControllerService commandToControllerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in controllerService.StreamMessagesAsync(commandToControllerService.ReadAllMessages(stoppingToken), stoppingToken).WithCancellation(stoppingToken))
        {
            
        }
    }
}