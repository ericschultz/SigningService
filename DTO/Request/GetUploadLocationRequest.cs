using Outercurve.DTO.Response;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace Outercurve.DTO.Request
{
    [Route("/get-uploadlocation")]
    [Authenticate]
    [RequiredPermission(ApplyTo.All, "AuthenticodeSign")]
    public class GetUploadLocationRequest : BaseRequest<GetUploadLocationResponse>
    {
    }
}