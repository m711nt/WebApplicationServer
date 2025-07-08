using System.Collections.Concurrent;
using System.Text.Json;
using SharedContract;

namespace WebApplicationClient.Services;

public class CommandHandlerService(
    IControllerService controllerService,
    ILogger<CommandHandlerService> logger)
    : BackgroundService
{
    private readonly ConcurrentQueue<SimpleMessage> _outgoingQueue = new();
    private readonly ConcurrentQueue<SimpleMessage> _incomingQueue = new();
    public void EnqueueOutgoingMessage(SimpleMessage message) => _outgoingQueue.Enqueue(message);
    public bool TryDequeueIncomingMessage(out SimpleMessage? message) => _incomingQueue.TryDequeue(out message);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Client: launching a background service");
        try
        {
            await foreach (var command in controllerService.StreamMessagesAsync(
                GetOutgoingMessages(stoppingToken)
            ).WithCancellation(stoppingToken).ConfigureAwait(false))
            {
                _incomingQueue.Enqueue(command);
                logger.LogInformation($"Get message from server: {JsonSerializer.Serialize(command.Command)}");
                if (command.Command is HelloCommand hello)
                {
                    var reply = new SimpleMessage { Command = new RegCommand { Registered = true }, Timestamp = DateTime.UtcNow };
                    _outgoingQueue.Enqueue(reply);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing messages to server");
        }
    }

    private async IAsyncEnumerable<SimpleMessage> GetOutgoingMessages(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_outgoingQueue.TryDequeue(out var msg))
            {
                yield return msg;
            }
            else
            {
                await Task.Delay(10, token);
            }
        }
    }
}