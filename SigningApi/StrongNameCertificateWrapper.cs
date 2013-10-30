using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Outercurve.SigningApi.WinApi;
using SigningServiceBase;

namespace Outercurve.SigningApi
{
    public class StrongNameCertificateWrapper : IStrongNameCertificateWrapper
    {
      
        private string _keyContainer = null;
        public StrongNameCertificateWrapper(X509Certificate2 certificate)
        {
            

            _keyContainer = Guid.NewGuid().ToString();
            var privateKey = (certificate.PrivateKey as RSACryptoServiceProvider).ExportCspBlob(true);

            if (!Mscoree.StrongNameKeyInstall(_keyContainer, privateKey, privateKey.Length))
            {
                throw new Exception("Unable to create KeyContainer");
            }
        }

        public void Sign(string fileName)
        {
            if (!Mscoree.StrongNameSignatureGeneration(fileName, _keyContainer, IntPtr.Zero, 0, 0, 0))
            {
                throw new Exception(String.Format("Unable Strong name assembly '{0}'.", fileName));
            }
        }

        public void Dispose()
        {
            if (_keyContainer != null)
            {
                Mscoree.StrongNameKeyDelete(_keyContainer);
                _keyContainer = null;
            }
        }
    }

 
}
