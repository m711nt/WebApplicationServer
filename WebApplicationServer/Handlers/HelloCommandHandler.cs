using WebApplicationServer.Abstractions;
using SharedContract;

namespace WebApplicationServer.Handlers;

public class HelloCommandHandler(ILogger<HelloCommandHandler> logger) : ICommandHandler<HelloCommand>
{
    public ValueTask HandleAsync(HelloCommand command)
    {
        logger.LogInformation($"[HelloCommandHandler] Обработка HelloCommand: Id={command.Id}");
        return ValueTask.CompletedTask;
    }

    async ValueTask ICommandHandler.HandleAsync(object command)
    {
        await HandleAsync((HelloCommand)command);
    }
}