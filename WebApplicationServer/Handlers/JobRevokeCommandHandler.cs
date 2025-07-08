using System;
using System.Threading.Tasks;
using WebApplicationServer.Abstractions;
using WebApplicationServer.Service;
using SharedContract;

namespace WebApplicationServer.Handlers
{
    public class JobRevokeCommandHandler(JobStorage jobStorage) : ICommandHandler<JobRevokeCommand>
    {
        public ValueTask HandleAsync(JobRevokeCommand command)
        {
            jobStorage.UnassignJobFromClient(command.ClientId, command.JobId);
            return ValueTask.CompletedTask;
        }

        async ValueTask ICommandHandler.HandleAsync(object command)
        {
            await HandleAsync((JobRevokeCommand)command);
        }
    }
} 