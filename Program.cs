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
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(
    options => { 
        options.HeaderName = "RequestVerificationToken"; 
    }
); // Protección CSRF.
builder.Services.AddAuthentication().AddCookie(
    options =>
    {
        //options.Cookie.Name = "Auth"                                              // Solo como referencia, podemos nombrar la cookie, pero se usará el nombre por defecto.
        options.Cookie.HttpOnly = true;                                             // OWASP A03: Cross-Site Scripting - XSS | Ocultar la cookie de código JS.
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;                    // Cambiar de None a Always en producción | Dicta si la cookie puede viajar por canales No seguros (HTTP).
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;    // Si en un futuro se integran servicios externos, cambiar a Lax.
        options.ExpireTimeSpan = TimeSpan.FromHours(2);                             // Tiempo de expiración de la cookie.
        options.SlidingExpiration = true;                                           // Renovar tiempo de expiración de la cookie cada que el usuario "toque" el servidor. | NOTA: Pasada la mitad del tiempo de vida, la cookie se renueva totalmente en automático con el renewSession.
        options.Cookie.MaxAge = options.ExpireTimeSpan;                             // Aunque se cierre el navegador, si no se cierra explicitamente la sesión, la cookie sigue viva.
        options.LoginPath = "/Home/Login";                                          // Redirigir aquí cuando no se está autenticado.
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/ErrorHandler";                            // Redirigir al manejador de errores personalizado.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                // Heurística: Si la cookie de auth existe, pero no estamos autenticados, entonces probablemente expiró o es inválida.
                bool hasAuthCookie = ctx.Request.Cookies.ContainsKey(options.Cookie.Name!); // Verificar si existe la cookie (aunque sea inválida).
                string reason = hasAuthCookie ? "sessionExpired" : "loginRequired";         // Determinar el motivo: "sessionExpired" = Cookie existe pero expiró | "loginRequired" = no hay Cookie.

                // Determinar si se trata de una petición por AJAX.
                bool isAjax = string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                bool acceptsJson = ctx.Request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

                // Fue una petición AJAX.
                if (isAjax || acceptsJson)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.Response.Headers["X-Auth-Reason"] = reason; // Header que contiene el motivo para el cliente.

                    return Task.CompletedTask;
                }

                // Fue una petición directa a controlador.
                var returnUrl = Uri.EscapeDataString(ctx.Request.Path + ctx.Request.QueryString);               // Codificar la URL actual, para redirección post-login.
                var loginUrl = $"{options.LoginPath}?reason={reason}&{options.ReturnUrlParameter}={returnUrl}"; // Construir la URL del login con parámetros de motivo y de redirección.
                ctx.Response.Redirect(loginUrl);

                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = ctx => 
            {
                // Determinar si se trata de una petición por AJAX.
                bool isAjax = string.Equals(ctx.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
                bool acceptsJson = ctx.Request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

                // Fue una petición AJAX.
                if (isAjax || acceptsJson)
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    ctx.Response.Headers["X-Auth-Reason"] = "forbidden"; // Header que contiene el motivo para el cliente.

                    return Task.CompletedTask;
                }

                // Fue una petición directa al controlador.
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

app.UseStatusCodePages( // Middleware 
    async ctx =>
    {
        var request = ctx.HttpContext.Request;
        var response = ctx.HttpContext.Response;
        var statusCode = response.StatusCode;

        // Determinar si se trata de una petición por AJAX.
        bool isAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        bool acceptsJson = request.Headers[HeaderNames.Accept].Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);

        if (statusCode >= 400 && !isAjax && !acceptsJson) { // Si no es petición ajax...
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
