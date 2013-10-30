using System;

namespace SigningServiceBase
{
    public interface IStrongNameCertificateWrapper : IDisposable, ITransientDependency
    {
        void Sign(string fileName);
    }
}