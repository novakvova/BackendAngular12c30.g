using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCrudApi.DAL.Entities;
using WebCrudApi.ViewModels;

namespace WebCrudApi.Controllers
{
    [Produces("application/json")]
    [Route("api/user-portal")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EFContext _context;
        public UsersController(EFContext context)//ctor
        {
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
        public string Post([FromBody]UserAddViewModel model)
        {
            string id = null;
            if(ModelState.IsValid)
            {
                return id;
            }
            return id;
        }
    }
}