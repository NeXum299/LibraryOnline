using AspAppLibrary.AuthLogic;
using DotNetEnv;

namespace AspAppLibrary.Repository
{
    public class AuthRepository
    {
        static AuthRepository()
        {
            Env.Load();
        }

        private static List<LoginModel> _loginModels = new List<LoginModel>
        { 
            new LoginModel { Username = "library1", Password = "P@ssw0rd123!"}
        };

        public static LoginModel GetLoginModel(string username, string password)
        {
            return _loginModels.FirstOrDefault(u => u.Username == username && u.Password == password);
        }
    }
}
