using Models;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace La_Castellana.Controllers;

public class UsersController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly AuthService _authService;
    private readonly AuthData _authData;

    public UsersController(ILogger<UsersController> logger, AuthData authData, AuthService authService)
    {
        _logger = logger;
        _authData = authData;
        _authService = authService;
    }

    [HttpGet("/Users/Users")]
    [Authorize]
    public IActionResult Users()
    {
        if (TempData["message"] != null)
        {
            ViewBag.message = TempData["message"];
        }
        return View("Users");
    }

    [HttpGet("/Users/SignIn")]
    [Authorize]
    public IActionResult SignIn()
    {
        if (TempData["message"] != null) // Revisar si existe un mensaje de alguna redirección previa.
        {
            ViewBag.message = TempData["message"];
        }
        return View("SignIn");
    }

    [HttpPost("/Users/SignIn")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult SignIn(UserCreateDTO user)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"⚠️ Los datos recibidos no coinciden con el modelo esperado.");
                return RedirectToAction("ErrorHandler", "Home", new{ statusCode=400, customError="Ocurrió un error en el servidor al procesar el modelo esperado." });
            }

            // --- Hashear el password para guardarlo en la DB.
            string hashedPassword = _authService.PasswordHasher(user.Password!.Trim());

            // --- Intentar separar el hash generado, si no son exactamente dos partes separadas por un punto, significa que se algo salió mal y no es válido.
            if (hashedPassword.Split('.').Length != 2) {
                _logger.LogError($"❌ Algo salió mal, el hash generado no tiene la esctructura esperada: <salt_base64>, <hash_base64>.");
                TempData["message"] = new Dictionary<string, string>
                {
                    { "message", "Ocurrió un error al guardar la contraseña, inténtalo de nuevo o contacta con el administrador." },
                    { "type", "error" }
                };
                return RedirectToAction("SignIn");
            }

            _authData.AddUser(user, hashedPassword); // Agregar al usuario, si algo sale mal, retorna excepción.

            ViewBag.message = $"El usuario {user.Name} {user.Pat_surname} fue creado con éxito.";
            return View("SignIn");
        }

        catch (InvalidOperationException ioe)
        {
            TempData["message"] = new Dictionary<string, string>
            {
                { "message", $"{ioe.Message}" },
                { "type", "error" }
            };
            return RedirectToAction("SignIn");
        }

        catch (Exception ex)
        {
            _logger.LogError($"❌ Ocurrió un error inesperado en UserController.cs -> SignIn() [POST]. Error: {ex.Message}");
            TempData["message"] = new Dictionary<string, string>
            {
                { "message", "Ocurrió un error inesperado al crear el usuario, intpentalo de nuevo o contacta con el administrador." },
                { "type", "error" }
            };
            return RedirectToAction("SignIn");
        }
    }
}