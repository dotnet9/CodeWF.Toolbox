using Dapper;
using Microsoft.Data.Sqlite;

namespace ConsoleAotDemo
{
    public static class DBHeper
    {
        public static void Test()
        {
            string connectionString = "Data Source=CodeWF.Toolbox.db";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var results = connection.Query("SELECT * FROM JsonPrettifyEntity");
                SqliteConnection.ClearPool(connection);
            }
            // 此时可以尝试删除数据库文件
            System.IO.File.Delete("CodeWF.Toolbox.db");
        }
    }
}
