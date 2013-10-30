namespace SigningServiceBase
{
    public interface IOPCSigner : ITransientDependency
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="overrideCurrentSignature"></param>
        /// <from>http://msdn.microsoft.com/en-us/library/system.io.packaging.packagedigitalsignaturemanager.sign(v=vs.100).aspx</from>
        void Sign(string path, bool overrideCurrentSignature);
    } 
    
}