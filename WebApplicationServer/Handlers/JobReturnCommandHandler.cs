using System;
using System.Threading.Tasks;
using WebApplicationServer.Abstractions;
using WebApplicationServer.Service;
using SharedContract;

namespace WebApplicationServer.Handlers
{
    public class JobReturnCommandHandler(JobStorage jobStorage) : ICommandHandler<JobReturnCommand>
    {
        public ValueTask HandleAsync(JobReturnCommand command)
        {
            jobStorage.UnassignJobFromClient(command.ClientId, command.JobId);
            return ValueTask.CompletedTask;
        }

        async ValueTask ICommandHandler.HandleAsync(object command)
        {
            await HandleAsync((JobReturnCommand)command);
        }
    }
} 