using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;

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
            // CONTINUAR AQUI
            //return StatusCode(StatusCodes.Status200OK, new { success=true, message="Esto es temporal", data="" });
            try
            {
                // --------- Inicio de Sesión.
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"⚠️ No fue posible continuar con la solicitud, los datos recibidos no coinciden con el modelo de Inicio de Sesión. ${ModelState}");
                    return StatusCode(StatusCodes.Status400BadRequest, new{ success=false, message="El modelo recibido no es válido." });
                }

                bool loginStatus = _authData.LoginAuth(user);
                if (!loginStatus)
                {
                    _logger.LogWarning($"⚠️ Inicio de sesión fallido para el usuario: {user.Username}");
                    return StatusCode(StatusCodes.Status401Unauthorized, new{ success=false, message="Credenciales incorrectas, inténtalo de nuevo." });
                }

                LoggedInUser userData = _authData.GetUserData(user.Username); // Inicio de sesión exitoso, obtener info del usuario.
                if (userData.User_id == null)
                {
                    _logger.LogWarning($"⚠️ Algo salió mal, la solicitud para obtener los datos del usuario se completó, pero esta no devolvió información. Modelo vacío.");
                    return StatusCode(StatusCodes.Status204NoContent, new{ success=false, message="La solicitud se completó pero el servidor no devolvió información del usuario solicitado, inténtalo de nuevo o contacta con el administrador de la aplicación." });
                }

                // --------- Configuración de la Cookie.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userData.User_id.ToString()),
                    new Claim(ClaimTypes.Name, $"{userData.Name} {userData.Pat_surname}"),
                    new Claim(ClaimTypes.Role, userData.AccessLevel)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); // Emitir Cookie al cliente.

                // --------- Respuesta al cliente.
                _logger.LogInformation($"✅ Inicio de sesión exitoso para el usuario: {userData.Username}");
                return StatusCode(StatusCodes.Status200OK, new{ success=true, message="Ok", redirectUrl="/Home/Main" });
            }

            catch (Exception ex)
            {
                _logger.LogError($"❌ Ocurrió un error inesperado al iniciar sesión. AuthController.cs -> Login(). Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new{ success=false, message="Ocurrió un error inesperado al iniciar sesión, inténtalo de nuevo o contacta con el administrador de la aplicación." });
            }
        }
    }
}