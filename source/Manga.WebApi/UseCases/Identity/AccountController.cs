using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Manga.WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public partial class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<object> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool IsEmail = Regex.IsMatch(model.EmailOrUserNameOrPhone,@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",RegexOptions.IgnoreCase);
            bool IsPhone = Regex.IsMatch(model.EmailOrUserNameOrPhone, @"^[0-9]{10}$", RegexOptions.IgnoreCase);

            if(IsEmail)
            {
                var isEmailExist = await _userManager.FindByEmailAsync(model.EmailOrUserNameOrPhone);

                if (isEmailExist != null)
                {
                    return await AttemptLogin(isEmailExist, model.Password);
                }
                else
                {
                    return "INVALID_EMAIL_ADDRESS";
                }

            }

            if (IsPhone)
            {
                var isPhoneExist = _userManager.Users.SingleOrDefault(p=> p.PhoneNumber == model.EmailOrUserNameOrPhone);

                if (isPhoneExist != null)
                {
                    return await AttemptLogin(isPhoneExist, model.Password);
                }
                else
                {
                    return "INVALID_PHONE_NUMBER";
                }

            }

            if (!IsPhone && !IsEmail)
            {
                var isUserNameExist = await _userManager.FindByNameAsync(model.EmailOrUserNameOrPhone);

                if (isUserNameExist != null)
                {
                    return await AttemptLogin(isUserNameExist,model.Password);
                }
                else
                {
                    return "INVALID_USER_NAME";
                }

            }

            return "UNEXPEXTED_ERROR";
            //throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        private async Task<Object> AttemptLogin(IdentityUser appUser,string Password)
        {
            var result = await _signInManager.PasswordSignInAsync(appUser,Password, false, false);
            if (result.Succeeded)
            {
                //var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.EmailOrUserNameOrPhone);
                return await GenerateJwtToken(appUser.Email, appUser);
            }
            else if(result.IsLockedOut)
            {
                return "YOUR_ACCOUNT_IS_LOCKED";
            }
            else if (result.IsNotAllowed)
            {
                return "YOU_ARE_NOT_ALLOWED_TO_ATTEMPT_LOGIN";
            }
            else if (result.RequiresTwoFactor)
            {
                return "YOUR_ACCOUNT_IS_REQUIRED_TWO_FACTOR_AUTHENTICATION";
            }
            else
            {
                return "INVALID_PASSWORD";
            }
        }

        [HttpPost]
        public async Task<object> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var isEmailExist = await _userManager.FindByEmailAsync(user.Email);
            var isUserNameExist = await _userManager.FindByNameAsync(user.UserName);
            var isPhoneExist = _userManager.Users.SingleOrDefault(p => p.PhoneNumber==user.PhoneNumber);

            if (isEmailExist != null)
            {
                if (isUserNameExist != null)
                {
                    if(isPhoneExist!=null)
                    {
                        return "PHONENUMBER_ALREADY_EXIST";
                    }
                    return "USER_ALREADY_EXIST";
                }
                return "EMAIL_ALREADY_EXIST";
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                return await GenerateJwtToken(model.Email, user);
            }
            else
            {
                return result.Errors;
            }


            //return "SOMETHING_WENT_WRONG";
        }

        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
