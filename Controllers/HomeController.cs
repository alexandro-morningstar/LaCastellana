using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Timers;

namespace La_Castellana.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string? reason=null, string? returnUrl=null)
    {
        ViewBag.SessionMessage = reason switch
        {
            "sessionExpired" => "Tu sesión terminó por inactividad. Inicia sesión nuevamente.",
            "loginRequired" => "Debes iniciar sesión para poder continuar.",
            _ => null
        };

        // return View("Login");
        return RedirectToAction("ErrorHandler", new{ statusCode=404 });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration=0, Location=ResponseCacheLocation.None, NoStore=true)]
    public IActionResult ErrorHandler(int statusCode, string? customError = null)
    {
        var errorInfo = new Dictionary<string, string>();

        // --------- Código de error para mostrar en vista.
        string errorCode = statusCode.ToString();
        errorInfo["errorCode"] = errorCode;

        // --------- Texto que se mostrará en el titulo de la pestaña.
        string tabInfo = statusCode switch
        {
            400 => "400 - Bad Request",
            403 => "493 - Forbbiden",
            404 => "404 - Not Found",
            405 => "405 - Method Not Allowed",
            408 => "408 - Timeout",
            500 => "500 - Internal Server Error",
            _ => "Unknow Error"
        };
        errorInfo["tabInfo"] = tabInfo;

        // --------- Texto que se mostrará en el cuerpo de la página.
        if (customError is null)
        {
            string bodyInfo = statusCode switch
            {
                400 => "Algo \"malió sal\" al intentar procesar la solicitud.",
                403 => "Parece que no cuentas con el nivel de privilegio para acceder a esta página.",
                404 => "Contenido no encontrado",
                405 => "Parece que intentas hacer una petición no válida.",
                408 => "El tiempo de solicitud excedió el límite de espera",
                500 => "Error interno del servidor",
                _ => "Parece que rompite la aplicación: Error no identificado."
            };
            errorInfo["bodyInfo"] = bodyInfo;
        }
        else { errorInfo["bodyInfo"] = customError; }

        ViewBag.info = errorInfo;
        return View("Error");
    }
}
