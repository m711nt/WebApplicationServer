using System.Text.Json;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using SharedContract;

namespace WebApplicationClient.Services;

internal class CommandHandlerService(
    IControllerService controllerService,
    ILogger<CommandHandlerService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Client: launching a background service");


        try
        {
            await foreach (var command in controllerService.StreamMessagesAsync(
                GetOutgoingMessages(stoppingToken)
            ).WithCancellation(stoppingToken).ConfigureAwait(false))
            {
                logger.LogInformation($"Get message from server: {JsonSerializer.Serialize((RegCommand)command.Command)}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing messages to server");
        }
    }

    private async IAsyncEnumerable<SimpleMessage> GetOutgoingMessages(CancellationToken token)
    {
        yield break;
    }
}