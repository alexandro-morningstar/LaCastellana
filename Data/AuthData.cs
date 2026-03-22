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
        string getHashQuery = "SELECT password FROM dbo.users WHERE user_id=@user_id";
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
}