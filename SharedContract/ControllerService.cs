using System.ServiceModel;
using ProtoBuf;
using ProtoBuf.Grpc;

namespace SharedContract;


[ProtoContract]
[ProtoInclude(100, typeof(HelloCommand))]
[ProtoInclude(101, typeof(RegCommand))]
[ProtoInclude(102, typeof(RequestJobCommand))]
[ProtoInclude(103, typeof(RequestJobAck))]
[ProtoInclude(104, typeof(RevokeJobCommand))]
[ProtoInclude(105, typeof(RevokeJobAck))]
[ProtoInclude(106, typeof(ReturnJobCommand))]
[ProtoInclude(107, typeof(ReturnJobAck))]
[ProtoInclude(108, typeof(AcknowledgeJobCommand))]
[ProtoInclude(109, typeof(AcknowledgeJobAck))]
[ProtoInclude(110, typeof(JobAssignCommand))]
[ProtoInclude(111, typeof(JobRevokeCommand))]
[ProtoInclude(112, typeof(JobReturnCommand))]
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

[ProtoContract]
public class RequestJobCommand : BaseCommand
{
}

[ProtoContract]
public class RequestJobAck : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class RevokeJobCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class RevokeJobAck : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class ReturnJobCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class ReturnJobAck : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class AcknowledgeJobCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public class AcknowledgeJobAck : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
}

[ProtoContract]
public readonly struct ClientConnectionId
{
    [ProtoMember(1)]
    public Guid Value { get; init; }
    public ClientConnectionId(Guid value) => Value = value;
    public override string ToString() => Value.ToString();
}

[ProtoContract]
public class JobAssignCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
    [ProtoMember(2)]
    public ClientConnectionId ClientId { get; set; }
}

[ProtoContract]
public class JobRevokeCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
    [ProtoMember(2)]
    public ClientConnectionId ClientId { get; set; }
}

[ProtoContract]
public class JobReturnCommand : BaseCommand
{
    [ProtoMember(1)]
    public Guid JobId { get; set; }
    [ProtoMember(2)]
    public ClientConnectionId ClientId { get; set; }
}

[ServiceContract]
public interface IControllerService
{
    [OperationContract]
    IAsyncEnumerable<SimpleMessage> StreamMessagesAsync(IAsyncEnumerable<SimpleMessage> request, CallContext context = default);
}