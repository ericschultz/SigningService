using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using ClrPlus.Core.Extensions;
using ClrPlus.Platform;
using ClrPlus.Windows.Api;
using ClrPlus.Windows.PeBinary.Utility;

namespace Outercurve.SigningApi
{
    public class AuthenticodeCertificateWrapper : IDisposable
    {
        private IntPtr _digitalSignInfo = IntPtr.Zero;
        private IntPtr _signContext = IntPtr.Zero;
        private X509Certificate2 _cert = null;

        public AuthenticodeCertificateWrapper(X509Certificate2 certificate)
        {
            _cert = certificate;
        }


        public void Sign(string fileName, string timeStampUrl)
        {
            
            _signContext = IntPtr.Zero;

            // Prepare signing info: exe and cert
            //
            var digitalSignInfo = new DigitalSignInfo();
            digitalSignInfo.dwSize = Marshal.SizeOf(digitalSignInfo);
            digitalSignInfo.dwSubjectChoice = DigitalSignSubjectChoice.File;
            digitalSignInfo.pwszFileName = fileName;
            digitalSignInfo.dwSigningCertChoice = DigitalSigningCertificateChoice.Certificate;
            digitalSignInfo.pSigningCertContext = _cert.Handle;
            digitalSignInfo.pwszTimestampURL = timeStampUrl;

            digitalSignInfo.dwAdditionalCertChoice = DigitalSignAdditionalCertificateChoice.AddChainNoRoot;
            digitalSignInfo.pSignExtInfo = IntPtr.Zero;

         //   var digitalSignExtendedInfo = new DigitalSignExtendedInfo("description", "http://moerinfo");
           // var ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf((object) digitalSignExtendedInfo));
            //Marshal.StructureToPtr(digitalSignExtendedInfo, ptr, false);
            // digitalSignInfo.pSignExtInfo = ptr;

            // Sign exe
            //
            
            if ((!CryptUi.CryptUIWizDigitalSign(DigitalSignFlags.NoUI, IntPtr.Zero, null, ref digitalSignInfo, ref _signContext)))
            {
                var rc = (uint)Marshal.GetLastWin32Error();
                if (rc == 0x8007000d)
                {
                    // this is caused when the timestamp server fails; which seems intermittent for any timestamp service.
                    throw new FailedTimestampException(fileName, timeStampUrl);
                }
                throw new DigitalSignFailure(fileName, rc);
            }

        }

        

        public static void SignUsingDefaultTimeStampUrls(string filename, X509Certificate2 cert, LoggingService loggingService = null)
        {
            filename.TryHardToMakeFileWriteable();

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
                    using (var wrap = new AuthenticodeCertificateWrapper(cert))
                    {
                        
                        if (loggingService != null)
                            loggingService.Debug("Going to sign and timestamp with {0} for {1}".format(url, filename));
                        wrap.Sign(filename, urls[i % urls.Length]);
                        if (loggingService != null)
                            loggingService.Debug("Sign and timestamp worked with {0} for {1}".format(url, filename));
                        // whee it worked!
                        signedOk = true;
                        break;
                    }
                    
                    
                }
                catch (FailedTimestampException)
                {
                    if (loggingService != null)
                        loggingService.Debug("Failed sign and timestamp with {0} for {1}".format(url, filename));
                    continue;
                }
            }

            if (!signedOk)
            {
                // we went thru each one 3 times, and it never signed?
                throw new FailedTimestampException(filename, "All of them!");
            }
        }



        public void Dispose()
        {
            if (_signContext != IntPtr.Zero)
            {
                if ((!CryptUi.CryptUIWizFreeDigitalSignContext(_signContext)))
                {
                    //throw new Win32Exception(Marshal.GetLastWin32Error(), "CryptUIWizFreeDigitalSignContext");
                }
            }

            Marshal.FreeCoTaskMem(_digitalSignInfo);
        }
    }
}
