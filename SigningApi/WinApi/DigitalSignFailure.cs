using System;

namespace Outercurve.SigningApi.WinApi {


    public class DigitalSignFailure : Exception {
        public uint Win32Code;

        public DigitalSignFailure(string filename, uint win32Code)
            : base(String.Format("Failed to digitally sign '{0}' Win32 RC: '{1:x}'",filename, win32Code)) {
            Win32Code = win32Code;
        }
    }
}