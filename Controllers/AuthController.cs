using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using La_Castellana.Models;

namespace La_Castellana.Controllers;

public class AuthController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly AuthData _authData;

    public AuthController(ILogger<AuthController> logger, AuthData authData)
    {
        _logger = logger;
        _authData = authData;
    }

    [AllowAnonymous]
    [HttpPost("/Login")]
    public async Task<IActionResult> Login([FromForm] UserLoginDTO user)
    {
        try
        {
            // ============ Validar cuerpo de la solicitud (Form vs Modelo esperado).
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Inicio de sesión fallido por credenciales incorrectas.");
                return View("Login", user); // === Devolver la misma vista con el modelo vacío para que Models.User en la vista interprete el error.
            }

            // ============ Inicio de Sesión.
            bool loginStatus = _authData.LoginAuth(user); // === Solicitar el inicio de sesión a la capa de datos.
            if (!loginStatus)
            {
                _logger.LogWarning($"Inicio de sesión faliido para usuario: {user.Username}");
                ModelState.AddModelError(string.Empty, "Las credenciales son incorrectas, revisa tus datos e intentalo de nuevo.");
                return View("Login", user);
            }

            // ============ Obtener información del usuario.
            LoggedInUser userData = _authData.GetUserData(user.Username!);
            if (userData.Username == null)
            {
                _logger.LogError("_authData.GetUserData devolvió un modelo vacío de LoggedInUser.");
                TempData["NotificationType"] = "Error";
                TempData["NotificationMessage"] = "Ocurrió un error inesperado, inténtalo de nuevo o contacta al administrador de la aplicación.";

                return Redirect("/");
            }

            // ============ Configuración de la Cookie.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userData.User_id.ToString()),
                new Claim(ClaimTypes.Name, $"{userData.Name} {userData.Pat_surname}"),
                new Claim(ClaimTypes.Role, userData.Rol!)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); // === Emitir la Cookie al cliente.
            _logger.LogInformation($"Inicio de sesión exitoso para usuario: {userData.Username}");

            return Redirect("Main");
        }

        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado en AuthController.cs => Login. Error: {ex.Message}");
            TempData["NotificationType"] = "Error";
            TempData["NotificationMessage"] = "Ocurrió un error inesperado al intentar iniciar sesión. Inténtalo de nuevo en unos minutos o contacta con el administrador.";

            return Redirect("/Login");
        }
    }
}