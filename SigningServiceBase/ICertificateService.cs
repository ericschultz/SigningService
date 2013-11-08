using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SigningServiceBase
{
    public interface ICertificateService : ITransientDependency
    {
        X509Certificate2 Get();
    }
}
