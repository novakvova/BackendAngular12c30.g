using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebCrudApi.DAL.Entities;
using WebCrudApi.Helpers;
using WebCrudApi.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace WebCrudApi.Controllers
{
    [Produces("application/json")]
    [Route("api/user-portal")]
    //[ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly UserManager<DbUser> _userManager;
        private readonly SignInManager<DbUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public UsersController(EFContext context,
            UserManager<DbUser> userManager,
            SignInManager<DbUser> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender)//ctor
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }
        [HttpGet("users")]
        [Authorize]
        public List<UserViewModel> Get()
        {
            var model=_context.Users
                .Select(u => new UserViewModel
                {
                    Id=u.Id,
                    Email=u.Email,
                    FirstName=u.UserProfile.FirstName,
                    LastName=u.UserProfile.LastName,
                    Age=u.UserProfile.Age,
                    Salary=u.UserProfile.Salary,
                    EmailConfirmed = u.EmailConfirmed
                }).ToList();
            return model;
        }

        // POST api/user-portal/users
        [HttpPost("users")]
        public async Task<IActionResult> Post([FromBody]UserAddViewModel model)
        {
            string id = null;
            if (!ModelState.IsValid)
            {
                var errors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errors);
            }

            var userProfile = new UserProfile
            {
                FirstName=model.FirstName,
                LastName=model.LastName,
                Salary=model.Salary,
                Age=model.Age
            };
            var user = new DbUser()
            {
                UserName = model.Email,
                Email = model.Email,
                UserProfile=userProfile
            };
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errors);
            }
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);


            var frontEndURL = _configuration.GetValue<string>("FrontEndURL");
            var callbackUrl =
                $"{frontEndURL}/confirmemail?userId={user.Id}&" +
                $"code={WebUtility.UrlEncode(code)}";

            await _emailSender.SendEmailAsync(model.Email, "Confirm Email",
               $"Please confirm your email by clicking here: " +
               $"<a href='{callbackUrl}'>link</a>");
            return Ok("SEMEN");
        }


        [HttpGet("users/{userid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userid)
        {
            var user = _context.Users
                .Include(u => u.UserProfile).SingleOrDefault(u => u.Id == userid); 
            if (user != null)
            {
                return Ok(new UserEditViewModel
                {
                    Id=user.Id,
                    FirstName=user.UserProfile.FirstName,
                    LastName=user.UserProfile.LastName,
                    Age=user.UserProfile.Age,
                    Salary=user.UserProfile.Salary
                });
            }
            return BadRequest(new { invalid = "Problem edit user by DB" });
        }

        [HttpPut("users/{userid}")]
        [AllowAnonymous]
        public IActionResult UpdateUser(string userid, [FromBody]UserEditViewModel model)
        {
            var user = _context.Users
                .Include(u => u.UserProfile).SingleOrDefault(u => u.Id == userid);
            if (user != null)
            {
                user.UserProfile.FirstName = model.FirstName;
                user.UserProfile.LastName = model.LastName;
                user.UserProfile.Age = model.Age;
                user.UserProfile.Salary = model.Salary;
                _context.SaveChanges();
                return Ok();
            }
            return BadRequest(new { invalid = "Problem edit user by DB" });
        }


        [HttpDelete("users/{userid}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUser(string userid)
        {
            var user = await _userManager.FindByIdAsync(userid);
            if (user != null)
            {
                var result = _userManager.DeleteAsync(user);
                if (result.Result.Succeeded)
                {
                    return Ok();
                }
                return BadRequest(new { invalid = "Problem delete user by DB" });
            }
            return Ok();
        }

        // POST api/user-portal/users/login
        [HttpPost("users/login")]
        public async Task<IActionResult> Login([FromBody]UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }

            var result = await _signInManager
                .PasswordSignInAsync(model.Email, model.Password,
                false, false);
            if (!result.Succeeded)
            {
                return BadRequest(new { invalid = "Не правильно введені дані!" });
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(
                new
                {
                    status = 200,
                    result = new { token=CreateToken(user) },
                    message = 1
                });
        }

        string CreateToken(DbUser user)
        {
            var roles = _userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>()
            {
                //new Claim(JwtRegisteredClaimNames.Sub, user.Id)
                new Claim("id", user.Id),
                new Claim("name", user.UserName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is the secret phrase"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(signingCredentials: signingCredentials, claims: claims);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [HttpGet("users/profile")]
        [Authorize]
        public UserViewModel GetUserProfile()
        {
            var isAuth = User.Identity.IsAuthenticated;
            var UserId = User.Claims.ToList();
            var model = new UserViewModel();
            //var model = _context.Users
            //    .Select(u => new UserViewModel
            //    {
            //        Id = u.Id,
            //        Email = u.Email,
            //        FirstName = u.UserProfile.FirstName,
            //        LastName = u.UserProfile.LastName,
            //        Age = u.UserProfile.Age,
            //        Salary = u.UserProfile.Salary,
            //        EmailConfirmed = u.EmailConfirmed
            //    }).ToList();
            return model;
        }

        [HttpPut("users/confirmemail/{userid}")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userid, [FromBody]ConfirmEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return BadRequest(new { invalid = "User is not found" });
            }
            var result = await _userManager.ConfirmEmailAsync(user, model.Code);
            if (!result.Succeeded)
            {
                var errrors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errrors);
            }
            return Ok();
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return BadRequest(new { invalid = "User is not found" });

            IdentityResult result =
                    await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                var errrors = CustomValidator.GetErrorsByIdentityResult(result);
                return BadRequest(errrors);
            }

            return Ok();
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errrors = CustomValidator.GetErrorsByModel(ModelState);
                return BadRequest(errrors);
            }

            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
            {
                return BadRequest(new { invalid = "User with this email was not found" });
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                "",
                "resetpassword",
                //pageHandler: null,
                values: new { userId = user.Id, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
               $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

            return Ok(new { answer = "Check your email" });
        }


    }
}