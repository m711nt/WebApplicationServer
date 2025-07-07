using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.ClientFactory;
using SharedContract;
using WebApplicationClient.Services;
using WebApplicationServer.Services;

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
                    // При необходимости можно подменить сервисы сервера
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
        var commandService = _clientApp!.Services.GetRequiredService<ServerCommandManager>();
        var testCommand = new SimpleMessage
        {
            Command = new HelloCommand { Id = 666 },
            Timestamp = DateTime.UtcNow
        };

        // Act - клиент отправляет сообщение серверу
        var clientId = new ServerCommandManager.ClientConnectionId();
        commandService.Subscribe(clientId);

        await commandService.SendCommandAsync(clientId, testCommand).ConfigureAwait(false);

        // Даем время на обработку (лучше заменить на явное ожидание)
        while (true)
        {
            await Task.Yield();
        }


        // Assert - проверяем, что сервер получил сообщение
        // Здесь нужно добавить проверку состояния сервера через его публичный API
        // Например:
        // var serverStats = await GetServerStatsAsync();
        // Assert.Contains(666, serverStats.ReceivedCommands);

        // Act - сервер отправляет ответ (если это предусмотрено логикой)
        // ...

        // Assert - проверяем, что клиент получил ответ
        // var clientStats = await GetClientStatsAsync();
        // Assert.True(clientStats.HasResponse);

        //Assert.False(syncContext.PostCalled, "Была попытка вернуться в контекст");
    }

    public Task DisposeAsync()
    {
        _serverApp?.Dispose();
        _clientApp?.Dispose();

        return Task.CompletedTask;
    }
}