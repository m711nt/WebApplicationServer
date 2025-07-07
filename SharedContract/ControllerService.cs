using System.ServiceModel;
using ProtoBuf;
using ProtoBuf.Grpc;

namespace SharedContract;


[ProtoContract]
[ProtoInclude(100, typeof(HelloCommand))]
[ProtoInclude(101, typeof(RegCommand))]
public abstract class BaseCommand { }

[ProtoContract]
public class SimpleMessage
{
    [ProtoMember(1)]
    public required BaseCommand Command { get; set; }

    [ProtoMember(2)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

[ProtoContract]
public class HelloCommand : BaseCommand
{
    [ProtoMember(1)]
    public int Id { get; set; }
}

[ProtoContract]
public class RegCommand : BaseCommand
{
    [ProtoMember(1)]
    public bool Registered { get; set; }
}


[ServiceContract]
public interface IControllerService
{
    [OperationContract]
    IAsyncEnumerable<SimpleMessage> StreamMessagesAsync(IAsyncEnumerable<SimpleMessage> request, CallContext context = default);
}