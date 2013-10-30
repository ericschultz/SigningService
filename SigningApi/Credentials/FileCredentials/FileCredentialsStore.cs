using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Outercurve.SigningApi.Credentials.FileCredentials.Store;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Text;
using SigningServiceBase;

namespace Outercurve.SigningApi.Credentials.FileCredentials
{
    
    public class FileCredentialsStore : BasicAuthProvider, ISimpleCredentialStore
    {
        private readonly ILoggingService _log;
        private readonly IFs _fs;
        public const int DEFAULT_PASS_LENGTH = 16;

        private readonly object _lock = new object();
        private readonly string _filePath;
        private readonly JsonCredentialStore _store;


        public FileCredentialsStore(HttpServerUtilityBase serverBase, IFs fs, ILoggingService log, string path)
        {
            _fs = fs;
            _log = log;
            _filePath = path;
            if (path.StartsWith("~"))
            {
                _filePath = serverBase.MapPath(path);
            }
            
            if (!fs.FileSystem.File.Exists(_filePath))
            {
                fs.FileSystem.File.WriteAllText(_filePath, "{}");
            }

            using (var file = fs.FileSystem.File.OpenRead(_filePath))
            {
                _store = JsonSerializer.DeserializeFromStream<JsonCredentialStore>(file);
            }
            
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            lock (_lock)
            {
                var user = GetUser(session.UserAuthName); 

                if (user == null)
                {
                    return;
                }

                session.IsAuthenticated = true;
                //Fill the IAuthSession with data which you want to retrieve in the app eg:
                

                session.Permissions = new List<string>(user.Permissions);

             
               

                //Important: You need to save the session!
                authService.SaveSession(session, SessionExpiry);
            }
        }

        public override bool TryAuthenticate(IServiceBase service, string userName, string password)
        {
           
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                _log.StartAuthenticate(userName, password, false);
                return false;
            }
            lock (_lock)
            {
                var user = GetUser(userName);

                if (user == null)
                {
                    return false;
                }

                var storedPassword = user.HashedPass;

                

                var pwd = HashPassword(password);
                if (pwd == storedPassword)
                {
                    _log.StartAuthenticate(userName, password, true);
                    return true;
                }
                

                /*   if (storedPassword == password)
                   {
                       // matched against password unsalted.
                       // user should change password asap.
                       return true;
                   }*/
                _log.StartAuthenticate(userName, password, false);
                return false;
            }
        }

        private User GetUser(string name)
        {
            return _store.Users.FirstOrDefault(u => u.Username == name);
        }

        //add salt!!!
           private string HashPassword(string password)
            {
                using (var hasher = MD5.Create())
                {
                    return
                        hasher.ComputeHash(Encoding.Unicode.GetBytes( password))
                              .Aggregate(String.Empty, (current, b) => current + b.ToString("x2").ToUpper());
                }
            }

            public string CreateUser(string userName)
            {
                


                var password = RandomPasswordGenerator.GeneratePassword(DEFAULT_PASS_LENGTH);
                if (CreateUserWithPassword(userName, password))
                {
                    return password;
                }
                return null;
            }





            private void Save()
            {
                using (var file = _fs.FileSystem.File.Open(_filePath, FileMode.Create))
                {
                    JsonSerializer.SerializeToStream(_store, file);
                }
            }

            


            /// <summary>
            /// 
            /// </summary>
            /// <param name="userName"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public bool CreateUserWithPassword(string userName, string password)
            {
                lock (_lock)
                {




                    var user =
                        GetUser(userName);

                        
                        if (user != null)
                        {

                            //user already exists!
                            return false;
                        }

                    user = new User {Username = userName, HashedPass = HashPassword(password)};

                    var list = new List<User>(_store.Users);
                    list.Add(user);
                    _store.Users = list.ToArray();

                    Save();

                        
                   return true;
                        
                    
                }
        

            }

#if false

            public string ResetPasswordAsAdmin(string userName)
            {
                lock (_lock)
                {
                    var path = UserPropertySheetPath;

                    if (path != null)
                    {
                        var propertySheet = UserPropertySheet;
                       
                        var user =
                           propertySheet.Rules.FirstOrDefault(rule => rule.Name == "user" && rule.Parameter == userName);


                        if (user == null)
                        {

                            //user doesn't exists!
                            return null;
                        }

                        var password = RandomPasswordGenerator.GeneratePassword(DEFAULT_PASS_LENGTH);
                        var propRule = user.GetPropertyRule("password");
                        var pv = propRule.GetPropertyValue(string.Empty);
                     
                     
                        pv.Add(HashPassword(password));
                        propertySheet.Save(path);


                        return password;
                        

                    }
                }

                return null;
            }

            public bool SetPassword(string userName, string newPassword)
            {
                lock (_lock)
                {
                    var path = UserPropertySheetPath;

                    if (path != null)
                    {
                        var propertySheet = UserPropertySheet;

                        var user =
                            propertySheet.Rules.FirstOrDefault(rule => rule.Name == "user" && rule.Parameter == userName);


                        if (user == null)
                        {

                            //user doesn't exists!
                            return false;
                        }

                        
                        var propRule = user.GetPropertyRule("password");
                        var pv = propRule.GetPropertyValue(string.Empty);

                       
                       pv.Clear();

                        pv.Add(HashPassword(newPassword));
                        propertySheet.Save(path);

                        return true;

                    }
                }

                return false;
            }



  

          


            private void SetRolesToUser(Rule user, IEnumerable<string> roles)
            {

                
                var rolesVal = GetRolesPV(user);
             

                SetPropertyValues(rolesVal, roles.Select(s => s.ToLowerInvariant()).ToArray());
                
            }

          

        
          



           

           

            internal bool RemoveUser(string userName)
            {
                lock (_lock)
                {
                    var path = UserPropertySheetPath;

                    if (path != null)
                    {
                        var propertySheet = UserPropertySheet;

                        var user =
                            propertySheet.Rules.FirstOrDefault(rule => rule.Name == "user" && rule.Parameter == userName);


                        if (user == null)
                        {

                            //user doesn't exists!
                            return false;
                        }

                        propertySheet.RemoveRule(user);
                        
                        propertySheet.Save(path);
                        return true;

                    }
                }

                return false;
            }
#endif
        }

}
