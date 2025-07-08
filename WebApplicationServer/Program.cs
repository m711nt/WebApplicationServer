using ProtoBuf.Grpc.Server;
using SharedContract;
using WebApplicationServer.Service;
using WebApplicationServer.Handlers;
using WebApplicationServer.Abstractions;
using WebApplicationServer.Services;


namespace WebApplicationServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCodeFirstGrpc(s =>
            {
                s.EnableDetailedErrors = true;
            });
            builder.Services.AddSingleton<ServerCommandManager>();
            builder.Services.AddScoped<IControllerService, ControllerService>();
            builder.Services.AddTransient<ICommandHandler<HelloCommand>, HelloCommandHandler>();
            builder.Services.AddSingleton<JobStorage>();
            builder.Services.AddTransient<ICommandHandler<JobAssignCommand>, JobAssignCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<JobRevokeCommand>, JobRevokeCommandHandler>();
            builder.Services.AddTransient<ICommandHandler<JobReturnCommand>, JobReturnCommandHandler>();

            builder.Services.AddLogging();

            var app = builder.Build();

            app.MapGrpcService<ControllerService>();

            app.Run();
        }
    }
}
