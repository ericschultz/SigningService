using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigningServiceBase.Flags;

namespace SigningServiceBase
{
    public interface IFs : IDependency, IDisposable
    {
       
        string TempPath { get; }
        IFileSystem FileSystem { get; }
        void ResetTempFolder();
        void MoveFileEx(string oldFilename, string newFilename, MoveFileFlags moveFlags);
        void TryHardToMakeFileWriteable(string filename);
        void TryHardToDelete(string location);
        string GenerateTemporaryFilename(string filename);
        string MarkFileTemporary(string filename);
        void RemoveTemporaryFiles();
        string NormalizePath( string path);
        string CreateTempPath(string fileName);
    }
}
