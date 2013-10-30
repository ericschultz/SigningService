using Autofac;
using SigningServiceBase;

namespace Outercurve.SigningApi.Credentials.HttpCredentials
{
    public class HttpCredentialsModule : Module
    {
        public string BaseUrl { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpCredentialsStore>()
                .WithParameter(new NamedParameter("baseUrl", BaseUrl))
                .As<ISimpleCredentialStore>();
        }
    }
}
