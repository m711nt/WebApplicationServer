using ProtoBuf.Grpc.Server;
using WebApplicationMatrix.Contract;
using WebApplicationMatrix.Service;

namespace WebApplicationMatrix.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCodeFirstGrpc();
            builder.Services.AddScoped<IControllerService, ControllerGrpcService>();

            builder.Services.AddLogging();

            var app = builder.Build();

            app.MapGrpcService<ControllerGrpcService>();

            app.Run();
        }
    }
}
