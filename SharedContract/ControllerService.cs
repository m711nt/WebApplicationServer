using System.Runtime.Serialization;
using System.ServiceModel;
using ProtoBuf.Grpc;

namespace WebApplicationMatrix.Contract;

[DataContract]
public class MathTask
{
    [DataMember(Order = 1)]
    public double Number1 { get; set; }

    [DataMember(Order = 2)]
    public double Number2 { get; set; }

    [DataMember(Order = 3)]
    public string Operation { get; set; } 

    [DataMember(Order = 4)]
    public DateTime Timestamp { get; set; }
}

[DataContract]
public class MathResult
{
    [DataMember(Order = 1)]
    public double Result { get; set; }

    [DataMember(Order = 2)]
    public DateTime Timestamp { get; set; }
}

[ServiceContract]
public interface IControllerService
{
    [OperationContract]
    IAsyncEnumerable<MathTask> StreamMessagesAsync(IAsyncEnumerable<MathResult> clientResults, CallContext context = default);
}