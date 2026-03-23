using Models;
using MySqlConnector;
using System.Data;

public class AuthData
{
    private readonly ILogger<AuthData> _logger;
    private readonly string _connectionString;
    private readonly AuthService _authService = new(); // AuthService se puede instanciar porque es una clase autocontenida (no requiere inyección de parámetros IConfiguration con Config["JwtKey"], no depende de otros servicios y solo usa librerías estándar).
    
    public AuthData(ILogger<AuthData> logger, IConfiguration servicesConfiguration)
    {
        _logger = logger;
        _connectionString = servicesConfiguration.GetConnectionString("DevConnection") ?? throw new Exception("No se encontró la cadena de conexión \"DevConnection\"");
    }

    public bool LoginAuth(UserLogin user)
    {
        string getHashQuery = "SELECT password FROM users WHERE user_id=@user_id";
        string? storedHash = null;

        try
        {
            using (MySqlConnection loginConn = new MySqlConnection(_connectionString))
            {
                loginConn.Open();

                using (MySqlCommand loginCmd = new MySqlCommand(getHashQuery, loginConn))
                {
                    loginCmd.Parameters.AddWithValue("@user_id", user.Username);

                    using (MySqlDataReader loginReader = loginCmd.ExecuteReader())
                    {
                        if (loginReader.Read()) // 1. ¿Existe algún hash asociado al usuario que se solicita
                        {
                            storedHash = loginReader.GetString("password"); // Se llama password, pero en realidad es el hash.
                        }
                    }

                    if (storedHash == null || !_authService.VerifyPassword(user.Password!, storedHash)) // 2. ¿Se recuperó el password? / ¿El hash generado con la contraseña proporcionada coincide con el hash almacenado?
                    {
                        return false;
                    }

                    return true; // Todo OK.
                }
            }
        }

        catch (MySqlException sqlex)
        {
            _logger.LogError($"❌ Ocurrió un error inesperado asociado con MySqlConnection en AuthData.cs -> LoginAuth(). Error: ${sqlex.Message}");
            throw;
        }

        catch (Exception ex)
        {
            _logger.LogError($"❌ Ocurrió un error inesperado en AuthData.cs -> LoginAuth(). Error: ${ex}");
            throw;
        }
    }

    public LoggedInUser GetUserData(string username)
    {
        LoggedInUser user = new LoggedInUser();
        string getUserQuery = @"
            SELECT
                u.user_id AS user_id,
                u.username AS username,
                u.name AS name,
                u.middlename AS middlename,
                u.pat_surname AS pat_surname,
                u.mat_surname AS mat_surname,
                u.email AS email,
                u.is_active AS is_active,
                alc.accessLevel AS accessLevel
            FROM
                users AS u
            INNER JOIN
                access_level_cat AS alc ON u.fk_accessLevel_id = alc.accessLevel_id
            WHERE
                u.username = @username
                AND u.is_active <> 'borrado';
        ";

        try
        {
            using (MySqlConnection userConn = new MySqlConnection(_connectionString))
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
                            user.Mat_surname = userReader.GetString("mat_surname");
                            user.Email = userReader.GetString("email");
                            user.Is_active = userReader.GetString("is_active");
                            user.AccessLevel = userReader.GetString("accessLevel");
                        }

                        return user;
                    }
                }
            }
        }

        catch (Exception ex)
        {
            _logger.LogError($"❌ Ocurrió un error inesperado al intentar obtener la información del usuario loggeado. AuthData.cs -> GetUserData() .Error: {ex.Message}.");
            throw;
        }
    }
}