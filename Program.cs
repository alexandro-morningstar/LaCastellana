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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AuthData>();
builder.Services.AddScoped<AuthService>();
// builder.Services.AddScoped<AdminData>();

// =============== Configuración de Seguridad.
builder.Services.AddAntiforgery(
    options =>
    {
        options.HeaderName = "RequestVerificationToken";
    }
); // === Protección CSRF.
builder.Services.AddAuthentication().AddCookie(
    options =>
    {
        //options.Cookie.Name = "Auth";                                             // Referencia: Se puede renombrar la cookie, sin embargo se usará el nombre por defecto.
        options.Cookie.HttpOnly = true;                                             // OWASP A03: Cross-Site Scripting - XSS | Ocultar la cookie de código JS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;                    // Cambiar de None a Always en producción. | Dicta si la cookie puede viajar por canales No Seguros (HTTP).
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;    // Si en un futuro se integran servicios externos, cambiar a Lax.
        options.ExpireTimeSpan = TimeSpan.FromHours(2);                             // Tiempo de vida de la Cookie.
        options.SlidingExpiration = true;                                           // Renovar el tiempo de expiración de la cookie cada que "toque" el servidor. | Nota: Pasada la mitad del tiempo de vida, la Cookie se renueva totalmente con el renewSession.
        options.Cookie.MaxAge = options.ExpireTimeSpan;                             // Aunque se cierre el navegador, si no se cierra explicitamente la sesión, la cookie va a permanecer activa el mismo tiempo que fue definido en ExpireTimeSpan.
        options.LoginPath = "/Home/Login";                                          // Redigir a esta ruta cuando el usuario no tenga una sesión activa. (No está autenticado)
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/ErrorHandler";                            // Redirigir al manejador de errores personalizado.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                // Heurística: Si existe una Cookie asociada al usuario, pero este no está autenticado, entonces probablemente expiró o no es válida.
                bool hasAuthCookie = ctx.Request.Cookies.ContainsKey(options.Cookie.Name!); // Verificar si existe la Cookie (aunque sea inválida).
                string reason = hasAuthCookie ? "sessionExpired" : "loginRequired";

                // Determinar si se trata de una petición por Fetch.
                bool isFetchJS = string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                bool acceptsJson = ctx.Request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

                if (isFetchJS || acceptsJson)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.Headers["X-Auth-Reason"] = reason; // <--- Este Header contiene el motivo del error para el cliente.

                    return Task.CompletedTask;
                }

                // Si no fue por Fetch, significa que fue una petición directa a Controlador.
                    var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);               // Codificar la URL actual, para redirigir nuevamente aquí después del Login.
                    var loginUrl = $"{options.LoginPath}?reason={reason}&{options.ReturnUrlParameter}={returnUrl}"; // Construir la URL del login con parámetros de Motivo y Redirección.
                ctx.Response.Redirect(loginUrl);

                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = ctx =>
            {
                // Determinar si se trata de una petición por Fetch.
                bool isFetchJS = string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                bool acceptsJson = ctx.Request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

                if (isFetchJS || acceptsJson)
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    ctx.Response.Headers["X-Auth-Reason"] = "forbidden";

                    return Task.CompletedTask;
                }

                // Fue una petición directa al Controlador.
                var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);
                var errorHandlerUrl = $"{options.AccessDeniedPath}?statusCode=403";
                ctx.Response.Redirect(errorHandlerUrl);

                return Task.CompletedTask;
            }
        };
    }
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePages( // === Middleware.
    async ctx =>
    {
        var request = ctx.HttpContext.Request;
        var response = ctx.HttpContext.Response;
        var statusCode = response.StatusCode;

        // Determinar si se trata de una petición mediante FetchJS.
        bool isFetchJS = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        bool acceptsJson = request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

        if (statusCode >= 400 && !isFetchJS && !acceptsJson) // Si NO se trata de una petición por Fetch JavaScript
        {
            response.Redirect($"/Home/ErrorHandler?statusCode={statusCode}");
        }
    }
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
