#nullable enable
namespace Infrastructure.Services
{
    public class AuthMessageSenderOptions
    {
        public string? SendGridKey { get; set; }
        public string? Email { get; set; }
    }
}