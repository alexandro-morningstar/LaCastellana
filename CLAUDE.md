# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Reglas de Operación (obligatorias)

- **Nunca aplicar cambios sin autorización explícita.** Antes de modificar o crear cualquier archivo, primero debes mostrar el plan de trabajo (qué archivos y qué cambios) y esperar la aprobación explícita del usuario. No asumas aprobación implícita por haber hecho algo similar antes.
- **Nunca ejecutar comandos de git que modifiquen el repositorio o el remoto**: prohibido `git add`, `git commit`, `git push`, `git pull` (y cualquier operación equivalente). Comandos de solo lectura como `git status`, `git diff` o `git log` sí están permitidos.
- **Enfoque transversal en ciberseguridad (OWASP Top 10).** En toda capa del proyecto (vistas, controladores, acceso a datos, configuración, peticiones HTTP) se debe evaluar el impacto en seguridad de cualquier cambio propuesto y señalarlo explícitamente en el plan de trabajo. Puntos ya cubiertos que deben preservarse y usarse como referencia:
  - *Injection (A03):* SQL siempre parametrizado vía `MySqlCommand.Parameters` (nunca concatenar input de usuario en queries), como en `AuthData`.
  - *Broken Authentication / Cryptographic Failures (A02/A07):* hashing de contraseñas con PBKDF2-SHA256 + salt y comparación en tiempo constante (`AuthService`); no loguear ni exponer contraseñas o hashes.
  - *Security Misconfiguration (A05):* cookies `HttpOnly`, `SecurePolicy.Always`, `SameSite=Strict`; no relajar estas banderas sin justificarlo explícitamente.
  - *CSRF:* mantener `@Html.AntiForgeryToken()` en formularios y el middleware de antiforgery configurado en `Program.cs`.
  - *XSS (A03):* confiar en el escapado automático de Razor (`@valor`); evitar `Html.Raw` con input no confiable.
  - *Broken Access Control (A01):* respetar `[AllowAnonymous]` solo donde es intencional; cualquier acción nueva que exponga datos de usuarios/roles/pedidos debe revisar autorización por rol.
  - *Security Logging/Error Handling (A09):* no filtrar detalles internos (stack traces, mensajes de excepción crudos) al usuario final a través del pipeline de errores; seguir usando `Abort()`/mensajes genéricos para el cliente y loguear el detalle solo server-side.

## Project Overview

La_Castellana is an ASP.NET Core 8 MVC web application (restaurant/POS-style management: users, roles, items, tables, orders). UI text, logs, and most comments are written in Spanish — match that convention in new code.

## Commands

```bash
dotnet build              # Build the project
dotnet run                # Run (Development profile: http://localhost:5032, https://localhost:7152)
dotnet watch run          # Run with hot reload
```

There is no test project in this repository yet.

### Database

The schema lives in `DDL.sql` (MySQL/MariaDB) — run it manually against a local MySQL instance to create the `LaCastellana` database and tables. The app connects via `MySqlConnector` using a connection string named `DevConnection`. This string is **not** present in `appsettings.json`/`appsettings.Development.json` and the `.csproj` has no `UserSecretsId`, so it must be supplied locally (e.g. via `dotnet user-secrets` after adding a `UserSecretsId`, or by adding a local, untracked `ConnectionStrings:DevConnection` entry) — otherwise `AuthData` throws on startup of any request that touches the DB.

## Architecture

**Layering:** Controllers → `Data/*Data.cs` (ADO.NET data access via `MySqlConnector`, raw parameterized SQL, no ORM) → `Data/Services/*Service.cs` (stateless business/crypto helpers, e.g. password hashing/verification). DTOs and view models live in `Models/` (one `UserModels.cs` file for all user-related DTOs today; follow the `*Models.cs` naming if adding more).

- `AuthData` depends on `IConfiguration` (connection string) and instantiates `AuthService` directly (not DI) since the latter is stateless/self-contained — follow this pattern for similarly stateless helper services.
- Password hashing format: `"{saltBase64}.{hashBase64}"` using PBKDF2-SHA256, 100,000 iterations (see `AuthService.VerifyPassword`). Compare hashes with `CryptographicOperations.FixedTimeEquals`, never `==`.

**Controllers:** `BaseController` is the abstract root all controllers inherit from; it exposes `Abort(statusCode, description)`, which stashes a custom message in `TempData["CustomErrorDescription"]` and returns `StatusCode(...)` so the central error pipeline (below) renders it. Routes are defined with explicit `[HttpGet("/path")]`/`[HttpPost("/path")]` attributes rather than relying purely on the conventional `{controller=Home}/{action=Index}/{id?}` route in `Program.cs`.

**Auth:** Cookie authentication (`Microsoft.AspNetCore.Authentication.Cookies`) is configured in `Program.cs`, not `appsettings`. Key behaviors to preserve when touching this:
- `OnRedirectToLogin`/`OnRedirectToAccessDenied` branch on whether the request looks like a fetch/JSON call (`X-Requested-With: XMLHttpRequest` or `Accept: application/json`) — fetch calls get a raw status code + `X-Auth-Reason` header; normal browser navigation gets redirected to `/Home/Login` or the error handler instead.
- `AuthController.Login` (`POST /Login`) validates the model, calls `AuthData.LoginAuth`, then `AuthData.GetUserData`, builds `ClaimTypes.NameIdentifier/Name/Role` claims, and signs in via `HttpContext.SignInAsync`.
- Roles are compared by name (string), not by `rol_id` — see `LoggedInUser.Rol`.

**Error handling pipeline** (central, in `Program.cs` + `HomeController`): non-Development envs use `UseExceptionHandler("/Home/Error")`; `UseStatusCodePages` intercepts any 4xx/5xx response and — for non-fetch requests — redirects to `GET /Home/ErrorHandler?statusCode=...`. `HomeController.ErrorHandler` resolves the description with this priority: `Abort()`'s `TempData["CustomErrorDescription"]` > caught exception message > default HTTP reason phrase, then renders `Views/Home/Error.cshtml`. Use `Abort()` from a controller action when you want a custom error message rendered through this pipeline instead of throwing.

**Views:** Most top-level pages (`Login.cshtml`, `Error.cshtml`, `Main.cshtml`-to-be) set `Layout = null` and are fully self-contained HTML documents with their own `<head>`/CSS rather than using `_Layout.cshtml` — check what the existing view does before assuming the shared layout applies. Client-side notifications use SweetAlert (`wwwroot/js/SweetAlert.js`) driven by `TempData["NotificationType"]`/`TempData["NotificationMessage"]` set from controllers.

**File header convention:** Most `.cs` source files start with an ASCII-art banner comment containing a copyright notice. Preserve this header when editing existing files; it's not something to clean up.
