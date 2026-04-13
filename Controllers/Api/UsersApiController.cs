using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace La_Castellana.Controllers.API
{
    [ApiController]
    [Route("UsersApi/[controller]")]

    public class UsersApiController : ControllerBase
    {
        private readonly ILogger<UsersApiController> _logger;
        private readonly UsersData _usersData;

        public UsersApiController(ILogger<UsersApiController> logger, UsersData usersData)
        {
            _logger = logger;
            _usersData = usersData;
        }
    }
}