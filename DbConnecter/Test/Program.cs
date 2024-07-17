using static SqlConnectionManager;

namespace Test
{

    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=Localhost;Initial Catalog=master;User ID=sa;Password=123456";

            SqlConnectionManager connectionManager = new SqlConnectionManager(connectionString);
            if (connectionManager.TestConnection()) {
                Console.WriteLine("连接成功");
            }


        }
    }
}
