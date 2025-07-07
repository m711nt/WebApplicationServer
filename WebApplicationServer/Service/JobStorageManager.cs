using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplicationServer.Service
{
    public class JobStorageManager
    {
        private readonly JobStorage _storage = JobStorage.Instance;
        // clientId -> список джобов клиента
        private readonly Dictionary<string, List<JobStub>> _clientJobs = new();

        // Выдать джобу клиенту (назначить первую свободную)
        public JobStub? AssignJobToClient(string clientId)
        {
            var freeJob = _storage.GetAllJobs().FirstOrDefault(j => !_clientJobs.Values.SelectMany(x => x).Any(cj => cj.Id == j.Id));
            if (freeJob == null) return null;
            if (!_clientJobs.ContainsKey(clientId))
                _clientJobs[clientId] = new List<JobStub>();
            _clientJobs[clientId].Add(freeJob);
            return freeJob;
        }

        // Отозвать джобу у клиента
        public bool RevokeJob(string clientId, Guid jobId)
        {
            if (_clientJobs.TryGetValue(clientId, out var jobs))
            {
                var job = jobs.FirstOrDefault(j => j.Id == jobId);
                if (job != null)
                {
                    jobs.Remove(job);
                    return true;
                }
            }
            return false;
        }

        // Вернуть джобу (клиент отказывается, возвращаем в пул)
        public bool ReturnJob(string clientId, Guid jobId)
        {
            return RevokeJob(clientId, jobId);
        }

        // Подтвердить взятие джобы (можно расширить логику, например, менять статус)
        public bool AcknowledgeJob(string clientId, Guid jobId)
        {
            // Здесь можно реализовать смену статуса, сейчас просто проверяем наличие
            return _clientJobs.TryGetValue(clientId, out var jobs) && jobs.Any(j => j.Id == jobId);
        }

        // Получить все джобы клиента
        public List<JobStub> GetClientJobs(string clientId)
        {
            return _clientJobs.TryGetValue(clientId, out var jobs) ? jobs : new List<JobStub>();
        }

        // Получить все назначения (clientId -> джобы)
        public Dictionary<string, List<JobStub>> GetAllAssignments()
        {
            return _clientJobs;
        }
    }
} 