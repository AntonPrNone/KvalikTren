using System.Data;
using System.Data.SqlClient;

public static partial class DB_Interaction
{
    private static SqlConnection _connection;
    private const string _connectionString = "Data Source=DESKTOP-KA59S37;Initial Catalog=Test;Integrated Security=True";

    public static void OpenConnection()
    {
        if (_connection == null)
        {
            _connection = new SqlConnection(_connectionString);
        }

        if (_connection.State == ConnectionState.Closed)
        {
            _connection.Open();
        }
    }

    public static void CloseConnection()
    {
        if (_connection != null && _connection.State == ConnectionState.Open)
        {
            _connection.Close();
        }
    }

    public static void Dispose()
    {
        _connection?.Dispose();
    }
}
