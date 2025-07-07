using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WebApplicationServer.Service
{
    public class JobStub
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Payload { get; set; }
    }

    public class JobStorage
    {
        private static readonly Lazy<JobStorage> _instance = new(() => new JobStorage());
        public static JobStorage Instance => _instance.Value;

        private readonly ConcurrentDictionary<Guid, JobStub> _jobs = new();

        private JobStorage() { }

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
    }
} 