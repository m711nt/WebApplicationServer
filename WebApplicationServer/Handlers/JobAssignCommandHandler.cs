using System;
using System.Threading.Tasks;
using WebApplicationServer.Abstractions;
using WebApplicationServer.Service;
using SharedContract;

namespace WebApplicationServer.Handlers
{
    public class JobAssignCommandHandler(JobStorage jobStorage) : ICommandHandler<JobAssignCommand>
    {
        public ValueTask HandleAsync(JobAssignCommand command)
        {
            jobStorage.AssignJobToClient(command.ClientId, command.JobId);
            return ValueTask.CompletedTask;
        }

        async ValueTask ICommandHandler.HandleAsync(object command)
        {
            await HandleAsync((JobAssignCommand)command);
        }
    }
} 