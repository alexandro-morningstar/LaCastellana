/*********************************************************************************************************************
 *     █████╗ ██╗     ███████╗██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██████╗  ██████╗ ███████╗    ███████╗███╗   ███╗     *
 *    ██╔══██╗██║     ██╔════╝╚██╗██╔╝██╔══██╗████╗  ██║██╔══██╗██╔══██╗██╔═══██╗██╔════╝    ██╔════╝████╗ ████║     *
 *    ███████║██║     █████╗   ╚███╔╝ ███████║██╔██╗ ██║██║  ██║██████╔╝██║   ██║███████╗    █████╗  ██╔████╔██║     *
 *    ██╔══██║██║     ██╔══╝   ██╔██╗ ██╔══██║██║╚██╗██║██║  ██║██╔══██╗██║   ██║╚════██║    ██╔══╝  ██║╚██╔╝██║     *
 *    ██║  ██║███████╗███████╗██╔╝ ██╗██║  ██║██║ ╚████║██████╔╝██║  ██║╚██████╔╝███████║    ███████╗██║ ╚═╝ ██║     *
 *    ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ╚══════╝╚═╝     ╚═╝     *
 *                                                                                                                   *
 *                                                                                                                   *
 *                                 Copyright (c) 2025 Sinuhé Alejandro Gómez Hernández                               *
 *                                                                                                                   *
 *                              Permission is granted for free use, but NOT for sale/rent.                           *
 *                             Commercial use is prohibited without explicit authorization.                          *
 *                                                                                                                   *
 *********************************************************************************************************************/

using La_Castellana.Models;
using MySqlConnector;

public class AuthData
{
    private readonly ILogger<AuthData> _logger;
    private readonly string _connectionString;
    private readonly AuthService _authService = new(); // AuthService se puede instanciar porque es una clase autocontenida.
    
    public AuthData(ILogger<AuthData> logger, IConfiguration servicesConfiguration)
    {
        _logger = logger;
        _connectionString = servicesConfiguration.GetConnectionString("DevConnection") ?? throw new Exception("No se encontró la cadena de conexión.");
    }

    public bool LoginAuth(UserLoginDTO user)
    {
        string getHashQuery = "SELECT password_hash FROM users WHERE username=@username";
        string? storedHash = null;

        try
        {
            using (MySqlConnection loginConn = new MySqlConnection(_connectionString))
            {
                loginConn.Open();

                using (MySqlCommand loginCmd = new MySqlCommand(getHashQuery, loginConn))
                {
                    loginCmd.Parameters.AddWithValue("@username", user.Username);

                    using (MySqlDataReader loginReader = loginCmd.ExecuteReader())
                    {
                        if (loginReader.Read()) // === 1. ¿Existe algún hash asociado al usuario que se solicita?
                        {
                            storedHash = loginReader.GetString("password_hash");
                        }
                    }

                    if (storedHash == null || !_authService.VerifyPassword(user.Password!, storedHash)) // === 2. ¿Se recuperó el password? / ¿El hash generado con la contraseña proporcionada con el hash almacenado?
                    {
                        return false;
                    }

                    return true; // === Todo OK.
                }
            }
        }

        catch (MySqlException sqlex)
        {
            _logger.LogError($"Error de MySqlConnection en AuthData.cs => LoginAuth(). Error: {sqlex.Message}");
            throw;
        }

        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado en AuthData.cs => LoginAuth(). Error: {ex.Message}");
            throw;
        }
    }


    public LoggedInUser GetUserData(string username)
    {
        LoggedInUser user = new LoggedInUser();
        string getUserQuery = @"
            SELECT
                u.user_id,
                u.username,
                u.name,
                u.middlename,
                u.pat_surname,
                u.mat_surname,
                u.email,
                u.is_deleted,
                r.name AS rol_name
            FROM
                users AS u
            INNER JOIN
                roles AS r ON u.rol_id = r.rol_id
            WHERE
                u.username = @username
                AND u.is_deleted <> 1;
        ";

        try
        {
            using  (MySqlConnection userConn = new MySqlConnection(_connectionString))
            {
                userConn.Open();

                using (MySqlCommand userCmd = new MySqlCommand(getUserQuery, userConn))
                {
                    userCmd.Parameters.AddWithValue("@username", username);

                    using (MySqlDataReader userReader = userCmd.ExecuteReader())
                    {
                        if (userReader.Read())
                        {
                            user.User_id = userReader.GetInt32("user_id");
                            user.Username = userReader.GetString("username");
                            user.Middlename = userReader.GetString("middlename");
                            user.Pat_surname = userReader.GetString("pat_surname");
                            user.Email = userReader.GetString("email");
                            user.Is_deleted = userReader.GetBoolean("is_deleted");
                            user.Rol = userReader.GetString("rol_name");
                        }

                        return user;
                    }
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogError($"Error inesperado en AuthData.cs => GetUserData() al intentar obtener la información del usuario. Error: {ex.Message}");
            throw;
        }
    }
}