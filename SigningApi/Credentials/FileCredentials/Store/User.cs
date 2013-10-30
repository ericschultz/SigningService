namespace Outercurve.SigningApi.Credentials.FileCredentials.Store
{

    internal class User
    {

      
        public string Username { get; set; }

        
        public string HashedPass { get; set; }

       
        public string[] Permissions { get; set; }
    }

}
