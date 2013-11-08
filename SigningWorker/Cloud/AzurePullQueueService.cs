using Outercurve.DTO.Services.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigningWorker.Cloud
{
    public class AzurePullQueueService : IPullQueueService
    {
        private readonly CloudQueueClient _client;
        public AzurePullQueueService(CloudQueueClient client)
        {
            _client = client;
        }

        public string Account
        {
            get { throw new NotImplementedException(); }
        }

        public string GetQueue(string name)
        {
            throw new NotImplementedException();
        }
    }
}
