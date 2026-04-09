using Models;
using MySqlConnector;

public class UsersData
{
    private readonly ILogger<AuthData> _logger;
    private readonly string _connectionString;
    
    public UsersData(ILogger<UsersData> logger, IConfiguration servicesConfiguration)
    {
        _logger = logger;
        _connectionString = servicesConfiguration.GetConnectionString("DevConnection") ?? throw new Exception("No se encontró la cadena de conexión");
    }

    public DataTablesResponse<UserListDTO> GetUsers(int draw, int start, int length, string searchValue, string sortColumn, string sortDirection)
    {
        var response = new DataTablesResponse<UserListDTO> { Draw = draw };

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
                    
                }
            }
        }
    }
}