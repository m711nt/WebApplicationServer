using WebApplicationMatrix.Contract;
using ProtoBuf.Grpc;

namespace WebApplicationMatrix.Service;

public class ControllerGrpcService : IControllerService
{
    private readonly ILogger<ControllerGrpcService> _logger;
    private readonly List<ControllerInfo> _controllers;

    public ControllerGrpcService(ILogger<ControllerGrpcService> logger)
    {
        _logger = logger;
        _controllers = new List<ControllerInfo>
        {
            new ControllerInfo { IpAddress = "192.168.1.100", ControllerName = "Controller 1" },
            new ControllerInfo { IpAddress = "192.168.1.101", ControllerName = "Controller 2" },
            new ControllerInfo { IpAddress = "192.168.1.102", ControllerName = "Controller 3" },
            new ControllerInfo { IpAddress = "192.168.1.103", ControllerName = "Controller 4" },
            new ControllerInfo { IpAddress = "192.168.1.104", ControllerName = "Controller 5" }
        };
    }

    public async IAsyncEnumerable<ControllerInfo> StreamControllersAsync(CallContext context = default)
    {
        foreach (var controller in _controllers)
        {
            yield return controller;
            await Task.Delay(1000, context.CancellationToken);
        }
    }
}