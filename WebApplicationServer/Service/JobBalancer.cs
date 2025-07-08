using System;
using System.Collections.Generic;
using System.Linq;
using SharedContract;

namespace WebApplicationServer.Service
{
    public class JobBalancer
    {
        // На вход: все джобы, список клиентов с их задачами (ClientConnectionId -> List<JobStub>)
        // На выход: IEnumerable<BaseCommand> (JobAssignCommand, JobRevokeCommand, JobReturnCommand)
        public IEnumerable<BaseCommand> BalanceJobs(
            IReadOnlyCollection<JobStub> allJobs,
            IReadOnlyDictionary<ClientConnectionId, List<JobStub>> clientJobs)
        {
            var result = new List<BaseCommand>();
            if (clientJobs.Count == 0 || allJobs.Count == 0)
                return result;

            // 1. Собираем все назначенные джобы
            var assignedJobIds = new HashSet<Guid>(clientJobs.Values.SelectMany(jobs => jobs.Select(j => j.Id)));
            // 2. Свободные джобы
            var freeJobs = allJobs.Where(j => !assignedJobIds.Contains(j.Id)).ToList();

            // 3. Считаем среднее количество задач на клиента
            int totalJobs = allJobs.Count;
            int clientCount = clientJobs.Count;
            int avgPerClient = totalJobs / clientCount;
            int remainder = totalJobs % clientCount;

            // 4. Формируем список: сколько задач должно быть у каждого клиента
            var clientList = clientJobs.Keys.ToList();
            var targetCounts = new Dictionary<ClientConnectionId, int>();
            for (int i = 0; i < clientList.Count; i++)
                targetCounts[clientList[i]] = avgPerClient + (i < remainder ? 1 : 0);

            // 5. Для каждого клиента считаем разницу и формируем команды
            int freeJobIdx = 0;
            foreach (var client in clientList)
            {
                var current = clientJobs[client].Count;
                var target = targetCounts[client];
                if (current < target)
                {
                    // Назначить недостающие задачи
                    int need = target - current;
                    for (int n = 0; n < need && freeJobIdx < freeJobs.Count; n++, freeJobIdx++)
                    {
                        result.Add(new JobAssignCommand { JobId = freeJobs[freeJobIdx].Id, ClientId = client });
                    }
                }
                else if (current > target)
                {
                    // Отозвать лишние задачи
                    var toRevoke = clientJobs[client].Take(current - target).ToList();
                    foreach (var job in toRevoke)
                        result.Add(new JobRevokeCommand { JobId = job.Id, ClientId = client });
                }
            }
            return result;
        }
    }
}