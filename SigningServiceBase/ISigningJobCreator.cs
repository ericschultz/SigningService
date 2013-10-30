namespace SigningServiceBase
{
    public interface ISigningJobCreator : ITransientDependency
    {
        Job CreateJob(string container, string path, bool strongName);
        string GetStatusPath(string path);
    }
}