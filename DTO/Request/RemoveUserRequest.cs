using Outercurve.DTO.Response;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace Outercurve.DTO.Request
{
    [Route("/remove-user")]
    [Authenticate]
    [RequiredPermission(ApplyTo.All, "RemoveUser")]
    public class RemoveUserRequest : BaseRequest<BaseResponse>
    {
        public string UserName { get; set; }
    }
}
