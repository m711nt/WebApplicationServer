
namespace WebApplicationServer.Service
{
    public class JobStorageManager
    {
        private readonly JobStorage _storage = JobStorage.Instance;
        private readonly Dictionary<string, List<JobStub>> _clientJobs = new();

        public JobStub? AssignJobToClient(string clientId)
        {
            var freeJob = _storage.GetAllJobs().FirstOrDefault(j => !_clientJobs.Values.SelectMany(x => x).Any(cj => cj.Id == j.Id));
            if (freeJob == null) return null;
            if (!_clientJobs.ContainsKey(clientId))
                _clientJobs[clientId] = new List<JobStub>();
            _clientJobs[clientId].Add(freeJob);
            return freeJob;
        }

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

        public bool ReturnJob(string clientId, Guid jobId)
        {
            return RevokeJob(clientId, jobId);
        }

        public bool AcknowledgeJob(string clientId, Guid jobId)
        {
            return _clientJobs.TryGetValue(clientId, out var jobs) && jobs.Any(j => j.Id == jobId);
        }

        public List<JobStub> GetClientJobs(string clientId)
        {
            return _clientJobs.TryGetValue(clientId, out var jobs) ? jobs : new List<JobStub>();
        }

        public Dictionary<string, List<JobStub>> GetAllAssignments()
        {
            return _clientJobs;
        }
    }
} 