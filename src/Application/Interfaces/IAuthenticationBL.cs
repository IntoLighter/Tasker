using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IAuthenticationBL
    {
        public Task RegisterAsync(string email, string password, HttpRequest request);
        public Task LogInAsync(string email, string password, ClaimsPrincipal user);
        public Task LogoutAsync(ClaimsPrincipal user);
        public Task<string> ConfirmEmailAsync(string userId, string code);
    }
}
