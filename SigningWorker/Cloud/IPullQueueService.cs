using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Outercurve.DTO.Services.Azure
{
    public interface IPullQueueService
    {
        string Account { get; }
        ICloudQueue GetQueue(string name);
    }
    
}
