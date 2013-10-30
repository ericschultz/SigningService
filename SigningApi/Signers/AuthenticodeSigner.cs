using System;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using Autofac;
using Funq;
using Outercurve.SigningApi.WinApi;
using ServiceStack.Logging;
using SigningServiceBase;

namespace Outercurve.SigningApi.Signers
{
    

    public class AuthenticodeSigner : INativeWindowsSigner
    {
        private readonly IFs _fs;
        private readonly ILoggingService _loggingService;
        private readonly IAuthenticodeCertificateWrapper _certificateWrapper;

       


        public AuthenticodeSigner(IFs fs, X509Certificate2 certificate, ILoggingService log, IAuthenticodeCertificateWrapper certificateWrapper) {
            Certificate = certificate;
            _fs = fs;
            _loggingService = log;
            _certificateWrapper = certificateWrapper;
           
           
           
        }
        public X509Certificate2 Certificate {get; private set;}

        public void Sign(string path, bool strongName = false)
        {
          
          /*  try
            {
                

                try
                {
                    _loggingService.Debug("LoggingInAuthenticodeSign");
                    //LogManager.GetLogger(GetType()).DebugFormat("path is {0}", path);
                    var certRef = new CertificateReference(Certificate);
                    _loggingService.Debug("Going to load binary at {0}".format(path));
                    /*
                    if (new FileInfo(path).Length < 4)
                    {
                        throw new InvalidFileToSignException();
                    }*/
                    if (strongName)
                    {
                        using (var wrap = new StrongNameCertificateWrapper(Certificate))
                        {
                            wrap.Sign(path);
                        }

                    }

                    _loggingService.Debug("Signing {0} with {1}".format(path, Certificate.SerialNumber));
            
                    SignUsingDefaultTimeStampUrls(path, Certificate);

                    
                    _loggingService.Debug("Finished signing {0} with {1}".format(path, Certificate.SerialNumber));
/*
                    r = BinaryLoad(path);
                    _loggingService.Debug("Binary at {0} loaded".format(path));
                    //LogManager.GetLogger(GetType()).DebugFormat("filename of Binary is {0}", r.Filename);
                    r.SigningCertificate = certRef;
                    if (strongName)
                        r.StrongNameKeyCertificate = certRef;

                    _loggingService.Debug("Going to do the signing");
                    
                    _loggingService.Debug("signgin finished successfully");
                }

                catch (AggregateException ae)
                {
                    _loggingService.Debug("Something is wrong!");
                    foreach (var i in ae.Flatten().InnerExceptions)
                    {
                        _loggingService.Error("Bad", i);
                    }

                    if (ae.Flatten()
                          .InnerExceptions.OfType<DigitalSignFailure>()
                          .Any(dsf => dsf.Win32Code == 2148204547))
                    {
                        throw new InvalidFileToSignException();
                    }
                    throw;
                }
            }*/
        }


        public void SignUsingDefaultTimeStampUrls(string filename, X509Certificate2 cert)
        {
            _fs.TryHardToMakeFileWriteable(filename);

            var urls = new[] {
                "http://timestamp.verisign.com/scripts/timstamp.dll", "http://timestamp.comodoca.com/authenticode", "http://www.startssl.com/timestamp", "http://timestamp.globalsign.com/scripts/timstamp.dll", "http://time.certum.pl/"
            };

            var signedOk = false;
            // try up to three times each url if we get a timestamp error
            for (var i = 0; i < urls.Length * 3; i++)
            {
                var url = urls[i % urls.Length];
                try
                {
                    

                    if (_loggingService != null)
                        _loggingService.Debug("Going to sign and timestamp with {0} for {1}".format(url, filename));
                    _certificateWrapper.Sign(filename, urls[i % urls.Length]);
                    if (_loggingService != null)
                        _loggingService.Debug("Sign and timestamp worked with {0} for {1}".format(url, filename));
                    // whee it worked!
                    signedOk = true;
                    break;
                    


                }
                catch (FailedTimestampException)
                {
                    if (_loggingService != null)
                        _loggingService.Debug("Failed sign and timestamp with {0} for {1}".format(url, filename));
                    continue;
                }
            }

            if (!signedOk)
            {
                // we went thru each one 3 times, and it never signed?
                throw new FailedTimestampException(filename, "All of them!");
            }
        }
    }

   
}
