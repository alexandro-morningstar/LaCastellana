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
                            user.User_id     = userReader.GetInt32("user_id");
                            user.Username    = userReader.GetString("username");
                            user.Name        = userReader.GetString("name");
                            user.Middlename  = (userReader["middlename"] as string) ?? "";
                            user.Pat_surname = userReader.GetString("pat_surname");
                            user.Mat_surname = (userReader["mat_surname"] as string) ?? "";
                            user.Email       = userReader.GetString("email");
                            user.Is_deleted  = userReader.GetBoolean("is_deleted");
                            user.Rol         = userReader.GetString("rol_name");
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


    public void AddUser(UserCreateDTO user, string hashedPassword, int created_by)
    {
        string checkUserQuery = "SELECT user_id, username, is_deleted FROM users WHERE username = @username;";
        string reactivateUserQuery = "UPDATE users SET is_deleted = 1 WHERE user_id = @user_id";
        string createUserQuery = @"
            INSERT INTO users(username, name, middlename, pat_surname, mat_surname, email, rol_id, created_by)
            VALUES(@username, @name, @middlename, @pat_surname, @mat_surname, @email, @rol_id, @created_by);
        ";
        bool user_exist = false; // === Se asume que no existe el usuario.
        bool deletion_flag = false;
        int user_id = 0;

        using (MySqlConnection addUserConn = new MySqlConnection(_connectionString))
        {
            addUserConn.Open();

            // === 1. Revisar si ya existe el usuario y si está eliminado o no.
            using (MySqlCommand checkUserCmd = new MySqlCommand(checkUserQuery, addUserConn))
            {
                // TODO: Hacer la lógica del chequeo
                // Luego la reactivación
                checkUserCmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = user.Username!.Trim();

                using (MySqlDataReader checkReader = checkUserCmd.ExecuteReader())
                {
                    if (checkReader.Read())
                    { // === Si existe el usuario, definir bandera de borrado y obtener user_id.
                        deletion_flag = checkReader.GetBoolean("is_deleted") == true ? true : false;
                        user_id = checkReader.GetInt32("user_id");
                    }
                }
            }

            // El insert o notificar que ya existe y está activo.
            using (MySqlTransaction addUserTran = addUserConn.BeginTransaction())
            {
                if (!user_exist) // === A. El usuario no existe: Crearlo.
                {
                    using (MySqlCommand createCmd = new MySqlCommand(createUserQuery, addUserConn))
                    {
                        createCmd.Parameters.Add("@username", MySqlDbType.VarChar).Value = user.Username;
                        createCmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = user.Name;
                        createCmd.Parameters.Add("@middlename", MySqlDbType.VarChar).Value = user.Middlename;
                        createCmd.Parameters.Add("@pat_surname", MySqlDbType.VarChar).Value = user.Pat_surname;
                        createCmd.Parameters.Add("@mat_surname", MySqlDbType.VarChar).Value = user.Mat_surname;
                        createCmd.Parameters.Add("@email", MySqlDbType.VarChar).Value = user.Email;
                        createCmd.Parameters.Add("@rol_id", MySqlDbType.Int32).Value = user.Rol_id;
                        createCmd.Parameters.Add("@created_by", MySqlDbType.Int32).Value = created_by;

                        createCmd.ExecuteNonQuery();
                    }
                }

                else
                {
                    if (deletion_flag) // B. El usuario ya existe y está desactivado: Reactivarlo.
                    {
                        using (MySqlCommand activateCmd = new MySqlCommand(reactivateUserQuery, addUserConn))
                        {
                            activateCmd.Parameters.Add("@user_id", MySqlDbType.Int32).Value = user_id;
                            activateCmd.ExecuteNonQuery();
                        }
                    }

                    else // C. El usuario sí existe y no está desactivado.
                    {
                        
                    }
                }
                addUserTran.Commit();
                _logger.LogInformation($"Usuario {user.Name} {user.Pat_surname} registrador exitosamente.");
            }
        }
    }
}