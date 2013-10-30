using System.Security.Cryptography.X509Certificates;

namespace SigningServiceBase
{
    public interface INativeWindowsSigner : ITransientDependency
    {
        X509Certificate2 Certificate { get; }
        void Sign(string path, bool strongName = false);
    }
}