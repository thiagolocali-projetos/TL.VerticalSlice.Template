using Microsoft.Data.SqlClient;
using System.Data;

namespace TL.VerticalSlice.Template.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlServerConnectionFactory(string connectionString)
        => _connectionString = connectionString;

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}

