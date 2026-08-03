using Microsoft.AspNetCore.Mvc;

namespace La_Castellana.Controllers
{
    [Controller]

    // Controlador Padre: Contiene utilidades, los demás controladores heredan de esta base para utilizar sus métodos.
    public abstract class BaseController : Controller
    {
        [NonAction] // Especificar a .NET que no se trata de una ruta Web, solamente un método interno.
        public IActionResult Abort(int statusCode, string description) // Intento por replicar el Abort de Python.
        {
            // Descripción personalizada se guarda en el contexto temporal de la petición.
            TempData["CustomErrorDescription"] = description;

            // Retornar el código de error para que el middleware ErrorHandler lo atrape.
            return StatusCode(statusCode);
        }
    }
}