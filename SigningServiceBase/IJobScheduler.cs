using System;

namespace SigningServiceBase
{
    public interface IJobScheduler : IDisposable, IDependency
    {
        void Add(Job job);
    }
}