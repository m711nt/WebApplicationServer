using ProtoBuf.Grpc.ClientFactory;
using SharedContract;
using WebApplicationClient.Services;

namespace WebApplicationClient
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
                });

            builder.Services.AddHostedService<CommandHandlerService>();

            builder.Services.AddLogging();

            var app = builder.Build();

            app.MapGet("/", () => "OK");

            app.Run();
        }
    }
}
