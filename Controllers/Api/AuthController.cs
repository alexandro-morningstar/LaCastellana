using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace La_Castellana.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly AuthData _authData;
        
        public AuthController(ILogger<AuthController> logger, AuthData authData)
        {
            _logger=logger;
            _authData=authData;
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] UserLogin user) 
        {
            // CONTINUAR AQUIß
            //return StatusCode(StatusCodes.Status200OK, new { success=true, message="Esto es temporal", data="" });
        }
    }
}