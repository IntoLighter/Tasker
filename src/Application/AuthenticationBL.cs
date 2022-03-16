using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Application
{
    public class AuthenticationBL
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthenticationBL> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthenticationBL(UserManager<IdentityUser> userManager,
            IEmailSender emailSender, SignInManager<IdentityUser> signInManager, ILogger<AuthenticationBL> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task RegisterAsync(string email, string password)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                _logger.LogInformation(LogEvents.Register,
                    "User {UserId} account created", user.Id);

                #region Подтверждение по почте через SendGrid (Необходим API key)

                // var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                // var callbackUrl = new UriBuilder
                // {
                //     Scheme = request.Scheme,
                //     Host = request.Host.Host,
                //     Path = "/Authentication/ConfirmEmail",
                //     Query = $"code={code}&userId={user.Id}"
                // };
                // if (request.Host.Port.HasValue)
                //     callbackUrl.Port = request.Host.Port.Value;
                //
                // await _emailSender.SendEmailAsync(email, "Confirm your email",
                //     $"Please confirm your account by " +
                //     $"<a href='{HtmlEncoder.Default.Encode(callbackUrl.ToString())}'>clicking here</a>.");

                #endregion

                await _signInManager.SignInAsync(user, false);
                return;
            }

            ThrowAuthenticationExceptionFromIdentityResultErrors("Invalid registration attempt", result, user.Id);
        }

        public async Task LoginAsync(string email, string password, ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                _logger.LogError(LogEvents.AuthenticationException,
                    "Invalid login attempt, user {UserId} has already been authenticated",
                    user.FindFirst(ClaimTypes.NameIdentifier).Value);
                throw new AuthenticationException("Invalid login attempt, user has already been authenticated");
            }

            var result = await _signInManager.PasswordSignInAsync(
                email, password, false, false);
            if (result.Succeeded)
            {
                _logger.LogInformation(LogEvents.LogIn,
                    "User with email {Email} logged in", email);
                return;
            }

            _logger.LogError(LogEvents.AuthenticationException,
                "Invalid login attempt for email {Email}", email);
            throw new AuthenticationException("Invalid login attempt");
        }

        public async Task LogoutAsync(ClaimsPrincipal user)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation(LogEvents.LogOut, "User {UserId} logged out",
                user.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        public async Task ConfirmEmailAsync(string userId, string code)
        {
            if (userId == null && code == null)
            {
                _logger.LogError(LogEvents.AuthenticationException,
                    "Tried to confirm email when userId and code were null");
                throw new AuthenticationException("userId and code are null");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError(LogEvents.AuthenticationException,
                    "Tried to confirm email, user {UserId} did not find", userId);
                throw new AuthenticationException($"There is no user with id={userId}");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
                ThrowAuthenticationExceptionFromIdentityResultErrors("Invalid confirm email attempt", result, userId);
        }

        private void ThrowAuthenticationExceptionFromIdentityResultErrors(string msg, IdentityResult result,
            string userId)
        {
            var errors = result.Errors.ToArray();
            var e = new AuthenticationException(msg);
            for (var i = 0; i < errors.Length; i++) e.Data.Add(i + 1, errors[i].Description);

            _logger.LogError(LogEvents.AuthenticationException, msg + " for user {UserId}", userId);
            throw e;
        }
    }
}