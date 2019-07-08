using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityAPI.Controllers
{
    public partial class AccountController
    {
        public class LoginDto
        {
            [Required(ErrorMessage = "Email/UserName/PhoneNumber is required.")]
            public string EmailOrUserNameOrPhone { get; set; }

            [Required]
            public string Password { get; set; }

        }
    }


}
