using System;
using System.Collections.Generic;

namespace WebApplicationServer.Service
{
    public class JobAssignedEvent
    {
        public Guid JobId { get; set; }
        public string ClientId { get; set; }
    }

    public class JobRevokedEvent
    {
        public Guid JobId { get; set; }
        public string ClientId { get; set; }
    }

    public class JobReturnedEvent
    {
        public Guid JobId { get; set; }
        public string ClientId { get; set; }
    }

    public class JobBalancer
    {
        public List<object> BalanceJobs(
            List<JobStub> allJobs,
            Dictionary<string, List<JobStub>> clientJobs)
        {
            var events = new List<object>();
            var assignedJobIds = new HashSet<Guid>();
            foreach (var jobs in clientJobs.Values)
                foreach (var job in jobs)
                    assignedJobIds.Add(job.Id);

            foreach (var job in allJobs)
            {
                if (!assignedJobIds.Contains(job.Id) && clientJobs.Count > 0)
                {
                    var clientId = GetFirstClientId(clientJobs);
                    events.Add(new JobAssignedEvent { JobId = job.Id, ClientId = clientId });
                }
            }
            return events;
        }

        private string GetFirstClientId(Dictionary<string, List<JobStub>> clientJobs)
        {
            foreach (var kvp in clientJobs)
                return kvp.Key;
            return string.Empty;
        }
    }
}