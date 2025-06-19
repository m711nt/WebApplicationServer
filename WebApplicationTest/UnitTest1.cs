using Moq;
using WebApplicationMatrix.Service;
using WebApplicationMatrix.Contract;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;

namespace WebApplicationTest
{
    public class ControllerClientGrpcTests
    {
        private readonly Mock<IControllerService> _mockService = new();
        private readonly Mock<ILogger<ControllerClient>> _mockLogger = new();
        private readonly ControllerClient _client;

        public ControllerClientGrpcTests()
        {
            _client = new ControllerClient(_mockService.Object, _mockLogger.Object);
        }

        private System.Threading.Channels.Channel<MathTask> GetChannel() =>
            (System.Threading.Channels.Channel<MathTask>)typeof(ControllerClient)
                .GetField("_taskChannel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_client);

        private Task InvokePrivate(string method, params object[] args) =>
            (Task)typeof(ControllerClient)
                .GetMethod(method, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_client, args);

        [Fact]
        public async Task StartBidirectionalCommunication_ReceivesTasksAndWritesToChannel()
        {
            var tasks = new List<MathTask>
            {
                new MathTask { Number1 = 1, Number2 = 2, Operation = "+" },
                new MathTask { Number1 = 3, Number2 = 4, Operation = "-" }
            };

            async IAsyncEnumerable<MathTask> GetTasks()
            {
                foreach (var t in tasks)
                {
                    yield return t;
                    await Task.Delay(10);
                }
            }

            _mockService.Setup(s => s.StreamMessagesAsync(It.IsAny<IAsyncEnumerable<MathResult>>(), It.IsAny<CallContext>()))
                .Returns(GetTasks());

            var channel = GetChannel();
            var cts = new CancellationTokenSource();
            var startTask = InvokePrivate("StartBidirectionalCommunication", cts.Token);

            await Task.Delay(100);
            cts.Cancel();

            var receivedTasks = new List<MathTask>();
            while (channel.Reader.TryRead(out var mathTask))
                receivedTasks.Add(mathTask);

            Assert.Equal(2, receivedTasks.Count);
            Assert.Equal(1, receivedTasks[0].Number1);
            Assert.Equal(3, receivedTasks[1].Number1);
        }

        [Fact]
        public async Task StartBidirectionalCommunication_HandlesErrors()
        {
            _mockService
                .Setup(s => s.StreamMessagesAsync(It.IsAny<IAsyncEnumerable<MathResult>>(), It.IsAny<CallContext>()))
                .Throws(new InvalidOperationException("Тестовая ошибка"));

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await (Task)typeof(ControllerClient)
                    .GetMethod("StartBidirectionalCommunication", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_client, new object[] { cts.Token });
            });
        }

        [Fact]
        public async Task StartBidirectionalCommunication_HandlesCancellation()
        {
            async IAsyncEnumerable<MathTask> GetInfiniteTasks()
            {
                while (true)
                {
                    yield return new MathTask { Number1 = 1, Number2 = 2, Operation = "+" };
                    await Task.Delay(10);
                }
            }

            _mockService.Setup(s => s.StreamMessagesAsync(It.IsAny<IAsyncEnumerable<MathResult>>(), It.IsAny<CallContext>()))
                .Returns(GetInfiniteTasks());

            var cts = new CancellationTokenSource();
            var communicationTask = (Task)typeof(ControllerClient)
                .GetMethod("StartBidirectionalCommunication", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_client, new object[] { cts.Token });

            await Task.Delay(100);
            cts.Cancel();

            try
            {
                await communicationTask;
                Assert.True(false, "Ожидалось исключение отмены");
            }
            catch (OperationCanceledException) { }
        }

        [Fact]
        public async Task GenerateResults_SendsCorrectResults()
        {
            var channel = GetChannel();
            await channel.Writer.WriteAsync(new MathTask { Number1 = 5, Number2 = 3, Operation = "+" });
            channel.Writer.Complete();

            var cts = new CancellationTokenSource();
            var results = typeof(ControllerClient)
                .GetMethod("GenerateResults", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_client, new object[] { cts.Token }) as IAsyncEnumerable<MathResult>;

            var enumerator = results.GetAsyncEnumerator();
            Assert.True(await enumerator.MoveNextAsync());
            Assert.Equal(8, enumerator.Current.Result);
        }

        [Fact]
        public async Task GenerateResults_HandlesInvalidOperation()
        {
            var channel = GetChannel();
            await channel.Writer.WriteAsync(new MathTask { Number1 = 5, Number2 = 3, Operation = "invalid" });
            channel.Writer.Complete();

            var cts = new CancellationTokenSource();
            var results = typeof(ControllerClient)
                .GetMethod("GenerateResults", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_client, new object[] { cts.Token }) as IAsyncEnumerable<MathResult>;

            var enumerator = results.GetAsyncEnumerator();
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await enumerator.MoveNextAsync());
        }
    }
}