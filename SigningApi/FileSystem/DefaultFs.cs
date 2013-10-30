using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

using Microsoft.Win32;
using Outercurve.SigningApi.WinApi;
using SigningServiceBase;
using SigningServiceBase.Flags;

namespace Outercurve.SigningApi.FileSystem
{
    public class DefaultFs : IFs
    {


        private readonly List<string> _disposableFilenames = new List<string>();

        private static int _counter = System.Diagnostics.Process.GetCurrentProcess().Id << 16;

        public static int Counter
        {
            get
            {
                return ++_counter;
            }
        }

        public static string CounterHex
        {
            get
            {
                return Counter.ToString("x8");
            }
        }


        


        /// <summary>
        ///     the Kernel filename prefix string for paths that should not be interpreted. Just nod, and keep goin'
        /// </summary>
        private const string NonInterpretedPathPrefix = @"\??\";

        /// <summary>
        ///     regular expression to identify a UNC path returned by the Kernel. (needed for path normalization for reparse points)
        /// </summary>
        private static readonly Regex UncPrefixRx = new Regex(@"\\\?\?\\UNC\\");

        /// <summary>
        ///     regular expression to match a drive letter in a low level path (needed for path normalization for reparse points)
        /// </summary>
        private static readonly Regex DrivePrefixRx = new Regex(@"\\\?\?\\[a-z,A-Z]\:\\");

        private bool _triedCleanup = false;
        private static string _originalTempFolder = null;
        public string TempPath { get; private set; }

        public DefaultFs(IFileSystem fileSystem)
        {
           
            FileSystem = fileSystem;
            _originalTempFolder = _originalTempFolder ?? FileSystem.Path.GetTempPath();
            ResetTempFolder();
        }

        public  void ResetTempFolder()
        {
            // set the temporary folder to be a child of the User temporary folder
            // based on the application name
            var appName = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Name;
            if (_originalTempFolder.IndexOf(appName, StringComparison.CurrentCultureIgnoreCase) == -1)
            {
                var appTempPath = FileSystem.Path.Combine(_originalTempFolder, appName);
                if (!FileSystem.Directory.Exists(appTempPath))
                {
                    FileSystem.Directory.CreateDirectory(appTempPath);
                }

                TempPath = FileSystem.Path.Combine(appTempPath, (FileSystem.Directory.GetDirectories(appTempPath).Count() + 1).ToString());
                if (!FileSystem.Directory.Exists(TempPath))
                {
                    FileSystem.Directory.CreateDirectory(TempPath);
                }

                Environment.SetEnvironmentVariable("TMP", TempPath);
                Environment.SetEnvironmentVariable("TEMP", TempPath);
            }

            TempPath = TempPath ?? _originalTempFolder;

            
        }

        public IFileSystem FileSystem
        {
            get;
            private set;
        }

        public void MoveFileEx(string oldFilename, string newFilename, MoveFileFlags moveFlags)
        {
            Kernel32.MoveFileEx(oldFilename, newFilename, moveFlags);
        }

        public void TryHardToMakeFileWriteable(string filename)
        {
            filename = FileSystem.Path.GetFullPath(filename);

            if (FileSystem.File.Exists(filename))
            {
                var tmpFilename = GenerateTemporaryFilename(filename);
                FileSystem.File.Move(filename, tmpFilename);
                FileSystem.File.Copy(tmpFilename, filename);
                TryHardToDelete(tmpFilename);
            }
        }


        public void TryHardToDelete(string location)
        {
            if (FileSystem.Directory.Exists(location))
            {
                try
                {
                    FileSystem.Directory.Delete(location, true);
                }
                catch
                {
                    // didn't take, eh?
                }
            }

            if (FileSystem.File.Exists(location))
            {
                try
                {
                    FileSystem.File.Delete(location);
                }
                catch
                {
                    // didn't take, eh?
                }
            }

            // if it is still there, move and mark it.
            if (FileSystem.File.Exists(location) || FileSystem.Directory.Exists(location))
            {
                try
                {
                    // move the file to the tmp file
                    // and tell the OS to remove it next reboot.
                    var tmpFilename = GenerateTemporaryFilename(location); // generates a unique filename but not a file!
                    MoveFileEx(location, tmpFilename, MoveFileFlags.MOVEFILE_REPLACE_EXISTING);

                    if (FileSystem.File.Exists(location) || FileSystem.Directory.Exists(location))
                    {
                        // of course, if the tmpFile isn't on the same volume as the location, this doesn't work.
                        // then, last ditch effort, let's rename it in the current directory
                        // and then we can hide it and mark it for cleanup .
                        tmpFilename = FileSystem.Path.Combine(FileSystem.Path.GetDirectoryName(location), "tmp." + CounterHex + "." + FileSystem.Path.GetFileName(location));
                        MoveFileEx(location, tmpFilename, MoveFileFlags.MOVEFILE_REPLACE_EXISTING);
                        if (FileSystem.File.Exists(tmpFilename) || FileSystem.Directory.Exists(location))
                        {
                            // hide the file for convenience.
                            FileSystem.File.SetAttributes(tmpFilename, FileSystem.File.GetAttributes(tmpFilename) | FileAttributes.Hidden);
                        }
                    }

                    // Now we mark the locked file to be deleted upon next reboot (or until another coapp app gets there)
                    MoveFileEx(FileSystem.File.Exists(tmpFilename) ? tmpFilename : location, null, MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT);
                }
                catch
                {
                    // really. Hmmm. 
                    // Logger.Error(e);
                }

                if (FileSystem.File.Exists(location))
                {
                    // Logger.Error("Unable to forcably remove file '{0}'. This can't be good.", location);
                }
            }
            return;
        }

        public string GenerateTemporaryFilename(string filename)
        {
            string ext = null;
            string name = null;
            string folder = null;

            if (!string.IsNullOrEmpty(filename))
            {
                ext = Path.GetExtension(filename);
                name = Path.GetFileNameWithoutExtension(filename);
                folder = Path.GetDirectoryName(filename);
            }

            if (string.IsNullOrEmpty(ext))
            {
                ext = ".tmp";
            }
            if (string.IsNullOrEmpty(folder))
            {
                folder = TempPath;
            }

            name = FileSystem.Path.Combine(folder, "tmpFile." + CounterHex + (string.IsNullOrEmpty(name) ? ext : "." + name + ext));

            if (FileSystem.File.Exists(name))
            {
                TryHardToDelete(name);
            }

            return MarkFileTemporary(name);
        }


        public string MarkFileTemporary(string filename)
        {
#if !DEBUG
            lock(typeof(FilesystemExtensions)) {
                _disposableFilenames.Add(filename);
            }
#endif
            return filename;
        }

        public void RemoveTemporaryFiles() {
            foreach (var f in _disposableFilenames.ToArray().Where(FileSystem.File.Exists)) {
                TryHardToDelete(f);
            }

            // this is a wee bit more aggressive than I'm comfortable with.
            // RemoveInvalidSymlinks();

            // and try to clean up as we leave the process too.
           // TryToHandlePendingRenames(true);
        }
#if false
        public void TryToHandlePendingRenames(bool force = false)
        {
            //*
            try
            {
                if (force || !_triedCleanup)
                {
                    _triedCleanup = true;

                    var localMachine = _registry.LocalMachine.
                        ;RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    var sessionManager = localMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager");
                    if (sessionManager == null)
                    {
                        return;
                    }

                    if (sessionManager.GetValueNames().ContainsIgnoreCase("PendingFileRenameOperations"))
                    {
                        var pfrops = (String[])sessionManager.GetValue("PendingFileRenameOperations");
                        var skipped = new List<string>();
                        if (!pfrops.IsNullOrEmpty())
                        {
                            for (var i = 0; i < pfrops.Length; i += 2)
                            {
                                var src = pfrops[i];
                                var dest = pfrops[i + 1];

                                try
                                {
                                    var srcNormal = NormalizePath(src);
                                    if (Directory.Exists(srcNormal))
                                    {
                                        if (string.IsNullOrEmpty(dest))
                                        {
                                            Directory.Delete(srcNormal, true);
                                            if (Directory.Exists(srcNormal))
                                            {
                                                // didn't delete? put it back in the list.
                                                skipped.Add(src + "\0");
                                            }
                                            continue;
                                        }
                                        // A directory rename operation. we'll leave these alone
                                        skipped.Add(src);
                                        skipped.Add(dest);
                                        continue;
                                    }

                                    if (!File.Exists(srcNormal))
                                    {
                                        continue; // we're dropping files that don't exist anymore at all.
                                    }

                                    if (string.IsNullOrEmpty(dest))
                                    {
                                        // delete op
                                        Kernel32.DeleteFile(srcNormal);
                                        if (File.Exists(srcNormal))
                                        {
                                            // didn't work. No problem. Add it back to the list.
                                            skipped.Add(src + "\0");
                                        }
                                        continue;
                                    }

                                    // it's a move op
                                    Kernel32.MoveFileEx(srcNormal, NormalizePath(dest), MoveFileFlags.MOVEFILE_REPLACE_EXISTING);
                                    if (File.Exists(srcNormal))
                                    {
                                        skipped.Add(src);
                                        skipped.Add(dest);
                                    }
                                    continue;
                                }
                                catch (Exception)
                                {
                                    skipped.Add(src);
                                    skipped.Add(dest);
                                }
                            }
                            sessionManager.SetValue("PendingFileRenameOperations", skipped.ToArray(), RegistryValueKind.MultiString);
                        }
                    }
                    sessionManager.Close();
                }
            }
            catch
            {
                // no worry if this doesn't go. it's a best-effort-kind of thing
            }
        }
#endif


        public string NormalizePath( string path)
        {
            if (path.StartsWith(NonInterpretedPathPrefix))
            {
                if (UncPrefixRx.Match(path).Success)
                {
                    path = UncPrefixRx.Replace(path, @"\\");
                }

                if (DrivePrefixRx.Match(path).Success)
                {
                    path = path.Replace(NonInterpretedPathPrefix, "");
                }
            }
            if (path.EndsWith("\\"))
            {
                var couldBeFilePath = path.Substring(0, path.Length - 1);
                if (FileSystem.File.Exists(couldBeFilePath))
                {
                    path = couldBeFilePath;
                }
            }

            return path;
        }

        public string CreateTempPath(string fileName)
        {
            var tempFolder = FileSystem.Path.GetTempPath();//_settings.GetString("TempFolder") != null ? HttpContext.Current.Server.MapPath(_settings.GetString("TempFolder")) : Path.GetTempPath();
           // LogManager.GetLogger(GetType()).DebugFormat("temp folder is: {0}", tempFolder);
            return FileSystem.Path.Combine(tempFolder, Guid.NewGuid().ToString() + fileName);
        }


        public void Dispose()
        {
            RemoveTemporaryFiles();
        }
    }
}