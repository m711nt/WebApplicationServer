using System.Text.Json;
using ProtoBuf.Grpc;
using SharedContract;
using WebApplicationServer.Services;
namespace WebApplicationServer.Service;

public class ControllerService(ServerCommandManager serverCommandManager, ILogger<ControllerService> logger, IServiceProvider serviceProvider) : IControllerService
{
    public async IAsyncEnumerable<SimpleMessage> StreamMessagesAsync(IAsyncEnumerable<SimpleMessage> request, CallContext context = default)
    {
        var clientId = new ServerCommandManager.ClientConnectionId();
        var connection = serverCommandManager.Subscribe(clientId);
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);

        var commandTask = CommandHandler(request, cancellationTokenSource.Token, clientId, serverCommandManager)
            .ContinueWith(task =>
            {
                logger.LogError(task.Exception, "Error on stream fetching");
                cancellationTokenSource.Cancel(true);
            }, TaskContinuationOptions.OnlyOnFaulted);

        try
        {
            while (await connection.WaitToReadAsync(cancellationTokenSource.Token).ConfigureAwait(false))
            {
                while (connection.TryRead(out var message))
                    yield return message;
            }
        }
        finally
        {
            serverCommandManager.TryUnsubscribe(clientId);
        }

        await commandTask;
    }

    private async Task CommandHandler(
    IAsyncEnumerable<SimpleMessage> request,
    CancellationToken cancellationToken,
    ServerCommandManager.ClientConnectionId clientId,
    ServerCommandManager serverCommandManager)
    {
        await foreach (var command in request.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation($"Server fetch & exec command: {JsonSerializer.Serialize((HelloCommand)command.Command)}");

            try
            {
                var ret = new SimpleMessage { Command = new RegCommand { Registered = true } };
                logger.LogInformation($"Server send reply: {JsonSerializer.Serialize((RegCommand)ret.Command)}");

                await serverCommandManager.SendCommandAsync(clientId, ret, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Server reply fail");
            }
        }
    }
}



