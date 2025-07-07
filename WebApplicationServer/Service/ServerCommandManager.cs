using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using SharedContract;

namespace WebApplicationServer.Services;

public class ServerCommandManager
{
    public readonly record struct ClientConnectionId(Guid Value)
    {
        public ClientConnectionId() : this(Guid.NewGuid()) { }
    }

    private readonly ConcurrentDictionary<ClientConnectionId, Connection> _clients = new();
    public class Connection
    {
        // это внутренняя реализация очереди команд
        // она должна быть скрыта от внешнего кода, т.е. может быть иметь любую реализацию
        private readonly Channel<SimpleMessage> _channel = Channel.CreateUnbounded<SimpleMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        // чтобы записать в очередь
        public ValueTask WriteAsync(SimpleMessage message, CancellationToken cancellationToken = default)
        {
            return _channel.Writer.WriteAsync(message, cancellationToken);
        }

        // чтобы подождать наличия в очереди хотя бы одного сообщения
        public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.WaitToReadAsync(cancellationToken);
        }

        // чтобы вычитать из очереди сообщение, если есть
        public bool TryRead([MaybeNullWhen(false)] out SimpleMessage message)
        {
            return _channel.Reader.TryRead(out message);
        }

        // чтобы закрыть канал соединения (лишь сообщает, что в очередь больше ничего писаться не будет)
        internal bool TryClose(Exception? ex = null)
        {
            return _channel.Writer.TryComplete(ex);
        }
    }

    // подписка возвращает готовое соединение
    // никаканих методов чтения из соединения в этом классе быть не должно, т.к. иначе это раскрывает реализацию класса Connection
    public Connection Subscribe(ClientConnectionId id)
    {
        // используем специальные методы конкурентного словаря
        return _clients.GetOrAdd(id, _ => new Connection());
    }

    public bool TryUnsubscribe(ClientConnectionId id)
    {
        // при удалении закрываем канал
        // вместо удалить соединение из словаря и закрыть канал, лишь одно простое - отписаться - которое делает все необходимые для этого действия
        if (_clients.TryRemove(id, out var connection))
            connection.TryClose();

        return false;
    }

    public ValueTask SendCommandAsync(SimpleMessage message, CancellationToken cancellationToken = default)
    {
        var tasks = _clients.Values.Select(conn => conn.WriteAsync(message, cancellationToken)).ToArray();
        return tasks.Length == 0 ? ValueTask.CompletedTask : new ValueTask(Task.WhenAll(tasks.Select(t => t.AsTask())));
    }

    public ValueTask SendCommandAsync(ClientConnectionId id, SimpleMessage message, CancellationToken cancellationToken = default)
    {
        if (_clients.TryGetValue(id, out var conn))
        {
            return conn.WriteAsync(message, cancellationToken);
        }
        return ValueTask.CompletedTask;
    }
}