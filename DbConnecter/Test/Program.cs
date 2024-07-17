using static SqlConnectionManager;
using Leveling;

namespace Test
{

    internal class Program
    {
        static void Main()
        {
            //string connectionString = "Data Source=Localhost;Initial Catalog=master;User ID=sa;Password=123456";

            //SqlConnectionManager connectionManager = new(connectionString);
            //if (connectionManager.TestConnection()) {
            //    Console.WriteLine("连接成功");
            //}
            Console.WriteLine("请输入文件路径");
            string? path = Console.ReadLine();
            if (path == null) {
                Console.WriteLine("路径错误!!!");
            }
            


        }
    }
}
