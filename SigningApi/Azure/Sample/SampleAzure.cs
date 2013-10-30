using System.IO.Abstractions;
using Outercurve.DTO.Services.Azure;
using SigningServiceBase;

namespace Outercurve.SigningApi.Azure.Sample
{
    public class SampleAzureClient : AzureClientBase
    {
        public SampleAzureClient(IFs fs) : base(fs)
        {
            Root = AzureService.UseStorageEmulator();
        }
    }
}