using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AspAppLibrary.AuthLogic
{
    public static class AuthOptions
    {
        public static readonly string ISSUER;
        public static readonly string AUDIENCE;
        private static readonly string KEY;

        static AuthOptions()
        {
            try
            {
                // Проверяем, существует ли .env файл
                var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
                if (File.Exists(envPath))
                {
                    Env.Load(envPath);
                }
                else
                {
                    Console.WriteLine(".env file not found! Using default values.");
                }

                ISSUER = Environment.GetEnvironmentVariable("ISSUER") ?? "LibraryServer";
                AUDIENCE = Environment.GetEnvironmentVariable("AUDIENCE") ?? "AuthClientWPF";
                KEY = Environment.GetEnvironmentVariable("KEY")
                    ?? "KEY not found in environment variables";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize AuthOptions", ex);
            }
        }

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            if (string.IsNullOrEmpty(KEY))
                throw new InvalidOperationException("JWT Key is not set!");

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
