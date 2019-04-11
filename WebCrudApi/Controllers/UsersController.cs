using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebCrudApi.DAL.Entities;
using WebCrudApi.Helpers;
using WebCrudApi.ViewModels;

namespace WebCrudApi.Controllers
{
    [Produces("application/json")]
    [Route("api/user-portal")]
    //[ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly UserManager<DbUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public UsersController(EFContext context,
            UserManager<DbUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender)//ctor
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
        }
        [HttpGet("users")]
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
                    Salary=u.UserProfile.Salary
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