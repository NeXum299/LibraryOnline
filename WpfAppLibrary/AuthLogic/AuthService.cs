using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfAppLibrary.AuthLogic;
using WpfAppLibrary.Models;


namespace WpfAppLibrary.ViewModel
{
    public class AuthService : IAuthService
    {
        private const string ApiUrl = "https://localhost:7058/login";

        public async Task<bool> Authenticate(LoginModel loginModel)
        {
            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync(ApiUrl, loginModel);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<AuthResponse>();
                var token = responseContent.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    Application.Current.Properties["JwtToken"] = token;
                    return true;
                }

                return true;
            }

            return false;
        }
    }
}
