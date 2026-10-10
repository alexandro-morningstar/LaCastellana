/*********************************************************************************************************************
 *     █████╗ ██╗     ███████╗██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██████╗  ██████╗ ███████╗    ███████╗███╗   ███╗     *
 *    ██╔══██╗██║     ██╔════╝╚██╗██╔╝██╔══██╗████╗  ██║██╔══██╗██╔══██╗██╔═══██╗██╔════╝    ██╔════╝████╗ ████║     *
 *    ███████║██║     █████╗   ╚███╔╝ ███████║██╔██╗ ██║██║  ██║██████╔╝██║   ██║███████╗    █████╗  ██╔████╔██║     *
 *    ██╔══██║██║     ██╔══╝   ██╔██╗ ██╔══██║██║╚██╗██║██║  ██║██╔══██╗██║   ██║╚════██║    ██╔══╝  ██║╚██╔╝██║     *
 *    ██║  ██║███████╗███████╗██╔╝ ██╗██║  ██║██║ ╚████║██████╔╝██║  ██║╚██████╔╝███████║    ███████╗██║ ╚═╝ ██║     *
 *    ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ╚══════╝╚═╝     ╚═╝     *
 *                                                                                                                   *
 *                                                                                                                   *
 *                                 Copyright (c) 2026 Sinuhé Alejandro Gómez Hernández                               *
 *                                                                                                                   *
 *                              Permission is granted for free use, but NOT for sale/rent.                           *
 *                             Commercial use is prohibited without explicit authorization.                          *
 *                                                                                                                   *
 *********************************************************************************************************************/
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using La_Castellana;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;

namespace La_Castellana.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index(string? reason=null, string? returnUrl = null)
    {
        ViewBag.SessionMessage = reason switch
        {
            "sessionExpired" => "Tu sesión terminó por inactividad. Inicia Sesión nuevamente.",
            "loginRequired" => "Debes iniciar sesión para acceder a la aplicación.",
            _ => null
        };

        return View("Login");
    }

    [HttpGet("/Home/ErrorGenerator")]
    public IActionResult Errorgen()
    {
        return Abort(404, "Esta es una prueba de error 404 con mensaje personalizado.");
    }

    [AllowAnonymous]
    [HttpGet("/Home/ErrorHandler")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ErrorHandler(int? statusCode = null)
    {
        int code = statusCode ?? HttpContext.Response.StatusCode;
        if (code == 200) { code = 500; }

        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // === Obtener la descripción personalizada del Abort().
        string? customError = TempData["CustomErrorDescription"] as string;
        
        // === Extraer la descripción HTTP nativa por defecto de .NET.
        string defaultReason = ReasonPhrases.GetReasonPhrase(code);
        if (string.IsNullOrEmpty(defaultReason)) { defaultReason = "Error no identificado."; }

        // === Cascada de prioridades: Abort() > Exception > DefaultDescription.
        string description = customError ?? exception?.Message ?? defaultReason;

        // === Armar objeto de respuesta.
        var errorInfo = new Dictionary<string, string>
        {
            { "error_code", code.ToString() },
            { "error_description", description }
        };

        string path = exceptionHandlerPathFeature?.Path ?? HttpContext.Request.Path;
        _logger.LogError($"Error: {code}: {description} | Path: {path}");

        ViewBag.info = errorInfo;
        return View("Error");
    }

    [AllowAnonymous]
    [HttpGet("/Home/Main")]
    public IActionResult Main()
    {
        return View("Main");
    }
}