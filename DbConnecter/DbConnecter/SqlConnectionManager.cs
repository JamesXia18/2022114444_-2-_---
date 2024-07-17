using System.Data.SqlClient;
using System;

using System;
using System.Data;
using System.Data.SqlClient;

public class SqlConnectionManager
{
    private readonly string _connectionString;

    public SqlConnectionManager(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public void ExecuteNonQuery(string sql, params SqlParameter[] parameters)
    {
        using (SqlConnection connection = GetConnection())
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
    {
        SqlConnection connection = GetConnection();
        SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        connection.Open();
        return command.ExecuteReader();
    }

    public T ExecuteScalar<T>(string sql, params SqlParameter[] parameters)
    {
        using (SqlConnection connection = GetConnection())
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            connection.Open();
            object result = command.ExecuteScalar();
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }

    /// <summary>
    /// 测试数据库是否正确连接
    /// </summary>
    /// <returns></returns>
    public bool TestConnection()
    {
        try
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                // 一旦 Open() 调用没有抛出异常，我们可以认为连接是成功的。
                return true;
            }
        }
        catch (Exception ex)
        {
            // 如果捕获到任何异常，可能是连接失败。
            Console.WriteLine($"Connection test failed with error: {ex.Message}");
            return false;
        }
    }
}