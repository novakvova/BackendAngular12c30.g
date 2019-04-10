using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public UsersController(EFContext context,
            UserManager<DbUser> userManager)//ctor
        {
            _userManager = userManager;
            _context = context;
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
            return Ok("SEMEN");
        }
    }
}