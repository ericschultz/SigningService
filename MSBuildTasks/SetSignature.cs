using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Outercurve.ClientLib;
using Outercurve.ClientLib.Messages;
using Outercurve.ClientLib.Services;

namespace Outercurve.MSBuildTasks
{
    /// <summary>
    /// Task for Autheniticode signing or strong naming a set of files
    /// </summary>
    public class SetSignature : Task, ICancelableTask
    {
        private readonly IFileSystem _fileSystem;
       

        private CancellationTokenSource _source = new CancellationTokenSource();

        public SetSignature() : this(new FileSystem()) 
        {
            
        }
       

        internal SetSignature(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
           
        }
       
        /// <summary>
        /// The files to be signed
        /// </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        /// <summary>
        /// The output directory of the signed files (usually you just want this to be your current directory)
        /// </summary>
        [Required]
        public string OutputDir { get; set; }

        
        /// <summary>
        /// True if you want to strong name your input files, false if you do not. File must already be delay signed. Has no effect on any file other than .NET assemblies
        /// </summary>
        public bool StrongName { get; set; }

        /// <summary>
        /// The Root Url of the signing service. If this is not set and you have set your default service 
        /// using the Set-OcfDefaultRemoteService cmdlet, the task will use the value saved via the cmdlet. If empty and no saved value exists, 
        /// the task will end with an error.
        /// </summary>      
        public string ServiceUrl { get; set; }

        /// <summary>
        /// The username for the signing service. If this is not set and you have set your default service 
        /// using the Set-OcfDefaultRemoteService cmdlet, the task will use the value saved via the cmdlet. If empty and no saved value exists, 
        /// the task will end with an error.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password for the signing service. If this is not set and you have set your default service 
        /// using the Set-OcfDefaultRemoteService cmdlet, the task will use the value saved via the cmdlet. If empty and no saved value exists, 
        /// the task will end with an error.
        /// </summary>
        public string Password { get; set; }

        [Output]
        public ITaskItem[] SignedFiles { get; set; }

        public override bool Execute()
        {
            try
            {
                //can we run Execute twice? I don't know so we'll say reset the CTS here
                _source = new CancellationTokenSource();
   
                SetCredentials();
                var sourcesToDestination = MapSourcesToDestination().ToArray();
                Log.LogMessage(sourcesToDestination.First().Destination.FullName);

                var signer = new SigningService(UserName, Password,
                                        sourcesToDestination, ServiceUrl,
                                        MessageHandler, ProgressHandler);
                signer.Sign(StrongName, _source);


                SignedFiles = sourcesToDestination.Select(i => i.Destination).Select(i => new TaskItem(i.FullName)).Cast<ITaskItem>().ToArray();
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e, true, true, null);
            }

            
            return !Log.HasLoggedErrors;

        }

        public void Cancel()
        {
            _source.Cancel();
        }

        private void ProgressHandler(ProgressMessage progressMessage)
        {
            
           // WriteProgress(new ProgressRecord(progressMessage.ActivityId, progressMessage.Activity, progressMessage.Description) { PercentComplete = progressMessage.PercentComplete, RecordType = progressMessage.MessageType.CastToString().CastToEnum<ProgressRecordType>() });
        }

        private void MessageHandler(Message message)
        {
            switch (message.MessageType)
            {

                case MessageType.Info: 
                    Log.LogMessage(MessageImportance.Normal, message.Contents);
                    break;
                case MessageType.Warning: 
                    Log.LogWarning(message.Contents);

                    break;
            }
        }

        private IEnumerable<SourceToDestinationMap<FileInfoBase>> MapSourcesToDestination()
        {
            
            var fullOutputDir = _fileSystem.Path.GetFullPath(OutputDir);
            return InputFiles.Select(i =>
                new SourceToDestinationMap<FileInfoBase>
                {
                    Source = _fileSystem.FileInfo.FromFileName(i.ItemSpec.Trim('/', '\\')),
                    Destination = _fileSystem.FileInfo.FromFileName(_fileSystem.Path.Combine(fullOutputDir.TrimEnd('/', '\\'), i.ItemSpec.Trim('/', '\\')))
                });
           

        }

        private void SetCredentials()
        {

            var exceptions = new List<Exception>();
            if (String.IsNullOrEmpty(UserName))
            {
                exceptions.Add(new Exception(@"UserName wasn't set. Either add it to your project/msbuild file or run Set-OcfDefaultRemoteService"));
            }
            if (String.IsNullOrEmpty(Password))
            {
                exceptions.Add(new ArgumentException(@"Password wasn't set. Either add it to your project/msbuild file or run Set-OcfDefaultRemoteService"));
            }

            if (String.IsNullOrEmpty(ServiceUrl))
            {
                exceptions.Add(new ArgumentException(@"ServiceUrl wasn't set. Either add it to your project/msbuild file or run Set-OcfDefaultRemoteService"));
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
