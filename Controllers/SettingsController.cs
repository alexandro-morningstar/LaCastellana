using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;

namespace La_Castellana.Controllers;

public class SettingsController : BaseController
{
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(ILogger<SettingsController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("/Settings")]
    public IActionResult Settings()
    {
        ViewBag.PageName = "Configuración";
        return View("Settings");
    }
}