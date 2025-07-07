namespace WebApplicationServer.Abstractions
{
    public interface ICommandHandler<TCommand> : ICommandHandler
    {
        ValueTask HandleAsync(TCommand command);
    }

    public interface ICommandHandler
    {
        ValueTask HandleAsync(object command);
    }
}
