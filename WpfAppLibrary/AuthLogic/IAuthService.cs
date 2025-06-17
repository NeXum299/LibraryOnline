using System.Threading.Tasks;
using WpfAppLibrary.Models;

namespace WpfAppLibrary.ViewModel
{
    public interface IAuthService
    {
        Task<bool> Authenticate(LoginModel model);
    }
}
