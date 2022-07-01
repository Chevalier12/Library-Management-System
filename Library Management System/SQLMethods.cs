using MySqlConnector;
using System.Data;

namespace Library_Management_System
{
    public class SQLMethods
    {
        public static DataTable SQLTable_Load(string tableName, string SqlConnectionString)
        {
            var table = new DataTable();
            using (var connection = new MySqlConnection(SqlConnectionString))
            {
                using (var command = new MySqlCommand("SELECT * FROM " + tableName, connection))
                {
                    connection.Open();
                    table.Load(command.ExecuteReader());
                    table.TableName = tableName;
                }
            }

            return table;
        }

        public static void SQLTable_Update(DataTable dataTable, string TableName, string SqlConnectionString)
        {
            using (var connection = new MySqlConnection(SqlConnectionString))
            {
                using (var command = new MySqlCommand("SELECT * from " + TableName, connection))
                {
                    connection.Open();
                    var da = new MySqlDataAdapter(command);
                    var daUP = new MySqlCommandBuilder(da);
                    da.Update(dataTable);
                }
            }

            using (var connection = new MySqlConnection(SqlConnectionString))
            {
                using (var command = new MySqlCommand("SET @count = 0; UPDATE `" + TableName + "` SET ID = @count:= @count + 1;", connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }


        }
    }
}
