using Microsoft.AspNetCore.Builder;
using ProtoBuf.Grpc.ClientFactory;
using WebApplicationMatrix.Contract;
using WebApplicationMatrix.Service;

namespace WebApplicationMatrix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddCodeFirstGrpcClient<IControllerService>(options =>
                {
                    options.Address = new Uri("https://localhost:7159");
                })
                .ConfigureChannel((_, options) => {
                    options.HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });

            builder.Services.AddHostedService<ControllerClient>();

            builder.Services.AddLogging();

            var app = builder.Build();

            app.Run();
        }
    }
}
