using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Outercurve.DTO.Response;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace Outercurve.DTO.Request
{
    [Route("/get-status")]
    [Authenticate]
    [RequiredPermission(ApplyTo.All, "AuthenticodeSign")]
    
    public class GetStatus : BaseRequest<GetStatusResponse>
    {
        public string Container { get; set; }
        public string Path { get; set; }
    }
}
