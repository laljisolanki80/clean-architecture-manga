using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IdentityAPI.Controllers
{
    public partial class AccountController
    {
        public class RegisterDto
        {

            [Required(ErrorMessage = "UserName is required.")]
            public string UserName { get; set; }


            [Required(ErrorMessage = "Email is required."), RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Email is Not Valid.")]
            public string Email { get; set; }


            [Required(ErrorMessage = "PhoneNumber Not Valid."), RegularExpression(@"^[0-9]{10}$",ErrorMessage ="Please Enter Corrrect Mobile Number Without + and Country Code")]
            public string PhoneNumber { get; set; }


            [Required]
            [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
            public string Password { get; set; }
        }
    }


}
