using System.ComponentModel.DataAnnotations;

namespace Web.Models.VMs
{
    public class AuthenticationVM
    {
        [Required(ErrorMessage = "Email required")]
        [EmailAddress(ErrorMessage = "Email invalid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        [DataType(DataType.Password, ErrorMessage = "Password invalid")]
        public string Password { get; set; }
    }
}