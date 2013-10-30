using System;
using System.IO;

namespace SigningServiceBase
{
    public interface IAuthenticodeCertificateWrapper : ITransientDependency, IDisposable
    {
        void Sign(Stream s, string timeStampUrl);
        void Sign(string fileName, string timeStampUrl);
    }
}