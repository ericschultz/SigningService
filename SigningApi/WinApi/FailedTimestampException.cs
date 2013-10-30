//-----------------------------------------------------------------------
// <copyright company="CoApp Project">
//     Copyright (c) 2010-2013 Garrett Serack and CoApp Contributors. 
//     Contributors can be discovered using the 'git log' command.
//     All rights reserved.
// </copyright>
// <license>
//     The software is licensed under the Apache 2.0 License (the "License")
//     You may not use the software except in compliance with the License. 
// </license>
//-----------------------------------------------------------------------

using System;

namespace Outercurve.SigningApi.WinApi {
    public class FailedTimestampException : Exception {
        public FailedTimestampException(string filename, string timestampurl)
            : base(String.Format("Failed to get timestamp for '{0}' from '{1}'",filename, timestampurl)) {
        }
    }
}