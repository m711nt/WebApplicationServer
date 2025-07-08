using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SharedContract;

namespace WebApplicationServer.Service
{
    public class JobStub
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Payload { get; set; }
    }

    public class JobStorage
    {
        // регистрация в di через фабрику, делает ровно тоже самое
        // и т.к. внедрение JobStorage будет через di, то именно этот вариант и следует использовать
        // services.AddSingleton(() => new JobStorage())
        private static readonly Lazy<JobStorage> _instance = new(() => new JobStorage());
        public static JobStorage Instance => _instance.Value;

        private readonly ConcurrentDictionary<Guid, JobStub> _jobs = new();
        private readonly ConcurrentDictionary<ClientConnectionId, List<Guid>> _clientJobs = new();

        public JobStorage() { }

        public bool AddJob(JobStub job)
        {
            return _jobs.TryAdd(job.Id, job);
        }

        public bool RemoveJob(Guid jobId)
        {
            return _jobs.TryRemove(jobId, out _);
        }

        public JobStub GetJob(Guid jobId)
        {
            _jobs.TryGetValue(jobId, out var job);
            return job;
        }

        public IEnumerable<JobStub> GetAllJobs()
        {
            return _jobs.Values;
        }

        public bool AssignJobToClient(ClientConnectionId clientId, Guid jobId)
        {
            var list = _clientJobs.GetOrAdd(clientId, _ => new List<Guid>());
            lock (list)
            {
                if (!list.Contains(jobId))
                {
                    list.Add(jobId);
                    return true;
                }
            }
            return false;
        }

        public List<Guid> GetClientJobs(ClientConnectionId clientId)
        {
            return _clientJobs.TryGetValue(clientId, out var list) ? new List<Guid>(list) : new List<Guid>();
        }

        public bool UnassignJobFromClient(ClientConnectionId clientId, Guid jobId)
        {
            if (_clientJobs.TryGetValue(clientId, out var list))
            {
                lock (list)
                {
                    return list.Remove(jobId);
                }
            }
            return false;
        }
    }
}