using System.IO.Abstractions;
using System.Linq;
using Outercurve.DTO.Services.Azure;
using ServiceStack.Configuration;
using SigningServiceBase;

namespace Outercurve.SigningApi
{
    public class AzureClient : AzureClientBase
    {
        public AzureClient(IFs fs, string account, string key) : base(fs)
        {
            Root = new AzureService(account, key);
        }
    }

}