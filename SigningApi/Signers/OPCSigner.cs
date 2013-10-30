using System;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SigningServiceBase;

namespace Outercurve.SigningApi.Signers
{
    public class OPCSigner : IOPCSigner
    {
        public OPCSigner(X509Certificate2 certificate, LoggingService log) {
            Certificate = certificate;
            _log = log;
        }

        private readonly LoggingService _log;

        public X509Certificate2 Certificate {get; private set;}

        public void Sign(string path, bool overrideCurrentSignature)
        {
            {
                _log.Debug(String.Format("We're going to try signing {0}, override current signature {1}", path,
                                                                                                   overrideCurrentSignature));
                var package = Package.Open(path);

                _log.Debug(String.Format("Opened {0}",path));
                var signatureManager = new PackageDigitalSignatureManager(package)
                    {
                        CertificateOption = CertificateEmbeddingOption.InSignaturePart
                    };

                if (signatureManager.IsSigned)
                {
                    if (overrideCurrentSignature)
                    {
                        _log.Debug("{0} is signed we'll try to remove signatures".format(path));
                        //TODO: make smarter so we only remove signatures for the relevant parts
                        signatureManager.RemoveAllSignatures();
                        package.Flush();
                    }
                    else
                    {
                        _log.Debug("{0} is signed, we're going to throw".format(path));
                        throw new AlreadySignedException();
                    }
                }

                var toSign = package.GetParts().Select(packagePart => packagePart.Uri).ToList();

                toSign.Add(PackUriHelper.GetRelationshipPartUri(signatureManager.SignatureOrigin));
                toSign.Add(signatureManager.SignatureOrigin);
                toSign.Add(PackUriHelper.GetRelationshipPartUri(new Uri("/", UriKind.RelativeOrAbsolute)));

                _log.Debug(String.Format("About to start signing {0}",path));
                signatureManager.Sign(toSign, Certificate);
                _log.Debug(String.Format("signed {0}, going to close",path));
                package.Close();

                _log.Debug(String.Format("closed {0}", path));
            }
            GC.Collect();
        }
    }

   
}
