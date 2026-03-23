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

namespace Models
{
    public class User
    {
        public int User_id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? Middlename { get; set; }
        public string? Pat_surname { get; set; }
        public string? Mat_surname { get; set; }
        public string? Email { get; set; }
        public string? Is_active { get; set; }
        public int Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public int Updated_by { get; set; }
        public DateTime Updated_at { get; set; }
        public int AccessLevel_id { get; set; }
    }

    public class UserLogin
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string? Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña de usuario es obligatoria.")]
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
        public string? Is_active { get; set; }
        public string? AccessLevel { get; set; } // Se pasa a string para comparar por texto "administrador", "usuario" y no por int (id).
    }

    public class UserCreateDTO
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [MaxLength(32, ErrorMessage = "El nombre de usuario no puede exceder los 32 caracteres.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "La contraseña es un campo obligatorio.")]
        [MaxLength(128, ErrorMessage = "La contraseña no puede exceder los 128 caracteres.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "El primer nombre es obligatorio")]
        [MaxLength(32, ErrorMessage = "El primer nombre no puede exceder los 32 caracteres.")]
        public string? Name { get; set; }

        [MaxLength(32, ErrorMessage = "El segundo nombre no puede exceder los 32 caracteres.")]
        public string? Middlename { get; set; }

        [Required(ErrorMessage = "El apellido paterno es un campo obligatorio.")]
        [MaxLength(32, ErrorMessage = "El apellido paterno no puede exceder los 32 caracteres.")]
        public string? Pat_surname { get; set; }

        [MaxLength(32, ErrorMessage = "El apellido materno no puede exceder los 32 caracteres.")]
        public string? Mat_surname { get; set; }

        [Required(ErrorMessage = "El correo electrónico es un campo obligatorio.")]
        [MaxLength(255, ErrorMessage = "El correo electrónico no puede exceder los 255 caracteres.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El estado del usuario es un campo obligatorio.")]
        [MaxLength(16, ErrorMessage = "El estado del usuario no puede exceder los 16 caracteres.")]
        public string? Is_active { get; set; }

        [Required(ErrorMessage = "\"Creado por\" es un campo obligatorio.")]
        public int Created_by { get; set; }

        [Required(ErrorMessage = "El nivel de acceso debe especificarse.")]
        public int Fk_accessLevel_id { get; set; }
    }

    public class UserUpdateDTO
    {
        [Required(ErrorMessage = "El id del usuario es un campo obligatorio.")]
        public int User_id { get; set; } 

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [MaxLength(32, ErrorMessage = "El nombre de usuario no puede exceder los 32 caracteres.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "El primer nombre es obligatorio")]
        [MaxLength(32, ErrorMessage = "El primer nombre no puede exceder los 32 caracteres.")]
        public string? Name { get; set; }

        [MaxLength(32, ErrorMessage = "El segundo nombre no puede exceder los 32 caracteres.")]
        public string? Middlename { get; set; }

        [Required(ErrorMessage = "El apellido paterno es un campo obligatorio.")]
        [MaxLength(32, ErrorMessage = "El apellido paterno no puede exceder los 32 caracteres.")]
        public string? Pat_surname { get; set; }

        [MaxLength(32, ErrorMessage = "El apellido materno no puede exceder los 32 caracteres.")]
        public string? Mat_surname { get; set; }

        [Required(ErrorMessage = "El correo electrónico es un campo obligatorio.")]
        [MaxLength(255, ErrorMessage = "El correo electrónico no puede exceder los 255 caracteres.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El estado del usuario es un campo obligatorio.")]
        [MaxLength(16, ErrorMessage = "El estado del usuario no puede exceder los 16 caracteres.")]
        public string? Is_active { get; set; }

        [Required(ErrorMessage = "\"Actualizado por\" es un campo obligatorio.")]
        public int Updated_by { get; set; }

        [Required(ErrorMessage = "El nivel de acceso debe especificarse.")]
        public int Fk_accessLevel_id { get; set; }
    }

    public class UserUpdatePasswordDTO
    {
        [Required(ErrorMessage = "El id del usuario es un campo obligatorio.")]
        public int User_id { get; set; }

        [Required(ErrorMessage = "La contraseña es un campo obligatorio.")]
        [MaxLength(128, ErrorMessage = "La contraseña no puede exceder los 128 caracteres")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "\"Actualizado por\" es un campo obligatorio.")]
        [MaxLength(32, ErrorMessage = "\"Actualizado por\" no puede exceder los 32 caracteres.")]
        public int Updated_by { get; set; }
    }
}