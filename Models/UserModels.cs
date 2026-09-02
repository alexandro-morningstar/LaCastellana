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

using System.ComponentModel.DataAnnotations;
using System.Timers;

namespace La_Castellana.Models
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string? Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string? Password { get; set; } = string.Empty;
    }

    public class LoggedInUser
    {
        public int User_id { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Middlename { get; set; }
        public string? Pat_surname { get; set; }
        public string? Mat_surname { get; set; }
        public string? Email { get; set; }
        public bool? Is_deleted { get; set; }
        public string? Rol { get; set; } // === Se usa como string para comparar el texto "Administrador", "Usuario" y no por Id.
    }

    public class UserCreateDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(255, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener un máximo de 32 carácteres y un mínimo de 3.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "El primer nombre es obligatorio.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "El primer nombre n opuede exceder los 32 caracteres ni tener menos de 3.")]
        public string? Name { get; set; }

        [StringLength(32, MinimumLength = 3, ErrorMessage = "El segundo nombre no puede exceder los 32 caracteres ni tener menos de 3.")]
        public string? Middlename { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "El primer apellido no puede exceder los 32 caracteres y debe tener un mínimo de 3.")]
        public string? Pat_surname { get; set; }

        [StringLength(32, MinimumLength = 3, ErrorMessage = "El segundo apellido no puede exceder los 32 caracteres y debe tener un mínimo de 3.")]
        public string? Mat_surname { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(255, ErrorMessage = "El correo electrónico no puede exceder los 255 caracteres.")]
        public string? Email { get; set; }

        public int Created_by { get; set; }

        [Required(ErrorMessage = "El nivel de accesso es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un Rol Válido.")]
        public int Rol_id { get; set; }
    }
}