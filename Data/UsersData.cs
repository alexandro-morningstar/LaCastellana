using Models;
using MySqlConnector;

public class UsersData
{
    private readonly ILogger<UsersData> _logger;
    private readonly string _connectionString;
    
    public UsersData(ILogger<UsersData> logger, IConfiguration servicesConfiguration)
    {
        _logger = logger;
        _connectionString = servicesConfiguration.GetConnectionString("DevConnection") ?? throw new Exception("No se encontró la cadena de conexión");
    }

    public DataTablesResponse<UserListElementDTO> GetUsers(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
    {
        var response = new DataTablesResponse<UserListElementDTO> { Draw = draw };

        var validColumns = new Dictionary<string, string>
        {
            { "0", "u.username" },
            { "1", "u.name" },
            { "2", "u.middlename"},
            { "3", "u.pat_surname"},
            { "4", "u.mat_surname" },
            { "5", "u.is_active" },
            { "6", "alc.accessLevel" }
        };
        string orderBy = validColumns.ContainsKey(sortColumn) ? validColumns[sortColumn] : "u.user_id";
        string dir = sortDirection.ToUpper() == "DESC" ? "DESC" : "ASC";

        try
        {
            using (MySqlConnection usersConn = new MySqlConnection(_connectionString))
            {
                usersConn.Open();
                
                // --- Consulta para obtener el conteo total.
                string countQuery = "SELECT COUNT(*) FROM users;";
                using (MySqlCommand countCmd = new MySqlCommand(countQuery, usersConn))
                {
                    response.RecordsTotal = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // --- Consulta principal con filtros y paginación.
                string whereClause = "";
                if (!string.IsNullOrEmpty(searchValue))
                {
                    whereClause = "WHERE u.username LIKE @search OR u.name LIKE @search OR u.pat_surname LIKE @search OR u.mat_surname LIKE @search OR alc.accessLevel LIKE @search";
                }
                string dataQuery = $@"
                    SELECT
                        u.user_id,
                        u.username,
                        u.name,
                        u.middlename,
                        u.pat_surname,
                        u.mat_surname,
                        u.is_active,
                        alc.accessLevel
                    FROM
                        users u
                    {whereClause}
                    ORDER BY {orderBy} {dir}
                    LIMIT @length OFFSET @start;
                ";
                using (MySqlCommand dataCmd = new MySqlCommand(dataQuery, usersConn))
                {
                    dataCmd.Parameters.AddWithValue("@search", $"%{searchValue}%");
                    dataCmd.Parameters.AddWithValue("@length", length);
                    dataCmd.Parameters.AddWithValue("@start", start);

                    using (MySqlDataReader usersReader = dataCmd.ExecuteReader())
                    {
                        while (usersReader.Read())
                        {
                            response.Data.Add(new UserListElementDTO
                            {
                                User_id = usersReader.GetInt32("user_id"),
                                Username = usersReader.GetString("username"),
                                Name = usersReader.GetString("name"),
                                Middlename = usersReader.GetString("middlename"),
                                Pat_surname = usersReader.GetString("pat_surname"),
                                Mat_surname = usersReader.GetString("mat_surname"),
                                Is_active = usersReader.GetString("is_active"),
                                AccessLevel = usersReader.GetString("accessLevel")
                            });
                        }
                    }
                }

                // --- Conteo filtrado.
                string filteredQuery = $"SELECT COUNT(*) FROM users u {whereClause}";
                using (MySqlCommand filterCmd = new MySqlCommand(filteredQuery, usersConn))
                {
                    response.RecordsFiltered = Convert.ToInt32(filterCmd.ExecuteScalar());
                }
            }

            return response;
        }

        catch (Exception ex)
        {
            _logger.LogError($"Ocurrió un error al intentar recuperar los usuarios. UsersData.cs -> GetUsers(). Error: {ex.Message}");
            throw;
        }
    }
}