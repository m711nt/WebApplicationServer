using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.ClientFactory;
using SharedContract;
using WebApplicationClient.Services;

namespace WebApplicationTest;

public class BidirectionalGrpcIntegrationTests : IAsyncLifetime
{
    private WebApplicationFactory<WebApplicationServer.Program>? _serverApp;
    private WebApplicationFactory<WebApplicationClient.Program>? _clientApp;

    public Task InitializeAsync()
    {
        // Инициализация сервера
        _serverApp = new WebApplicationFactory<WebApplicationServer.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                });
            });

        _ = _serverApp.Server;

        var handler = _serverApp.Server.CreateHandler();

        // Инициализация клиента
        _clientApp = new WebApplicationFactory<WebApplicationClient.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services
                        .AddCodeFirstGrpcClient<IControllerService>(o => o.Address = _serverApp.Server.BaseAddress)
                        .ConfigurePrimaryHttpMessageHandler(() => handler /*_serverApp.Server.CreateHandler() */);
                });
            });

        return Task.CompletedTask;
    }

    class TestSynchronizationContext : SynchronizationContext
    {
        public bool PostCalled { get; private set; }
        public override void Post(SendOrPostCallback d, object? state)
        {
            PostCalled = true;
            base.Post(d, state);
        }
    }

    [Fact]
    public async Task ServerToClient_And_ClientToServer_Echo()
    {
        var syncContext = new TestSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(syncContext);

        // Arrange
        var handlerService = _clientApp.Services.GetRequiredService<CommandHandlerService>();
        var testCommand = new SimpleMessage
        {
            Command = new HelloCommand { Id = 666 },
            Timestamp = DateTime.UtcNow
        };

        // Act - клиент отправляет сообщение серверу через свою очередь
        handlerService.EnqueueOutgoingMessage(testCommand);

        // Ждем ответ от клиента (RegCommand)
        var sw = System.Diagnostics.Stopwatch.StartNew();
        bool received = false;
        while (sw.Elapsed < TimeSpan.FromSeconds(5))
        {
            if (handlerService.TryDequeueIncomingMessage(out var msg) && msg.Command is RegCommand reg && reg.Registered)
            {
                received = true;
                break;
            }
            await Task.Delay(10);
        }
        Assert.True(received, "Клиент не получил ответ от сервера (RegCommand) за 5 секунд");
    }

    public Task DisposeAsync()
    {
        _serverApp?.Dispose();
        _clientApp?.Dispose();

        return Task.CompletedTask;
    }
}