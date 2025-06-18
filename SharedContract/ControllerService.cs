using System.Runtime.Serialization;
using System.ServiceModel;
using ProtoBuf.Grpc;

namespace WebApplicationMatrix.Contract;

[DataContract]
public class ControllerInfo
{
    [DataMember(Order = 1)]
    public string IpAddress { get; set; }

    [DataMember(Order = 2)]
    public string ControllerName { get; set; }
}

[ServiceContract]
public interface IControllerService
{
    [OperationContract]
    IAsyncEnumerable<ControllerInfo> StreamControllersAsync(CallContext context = default);
}