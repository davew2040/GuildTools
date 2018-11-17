using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Sql
{
    public class Accounts
    {
        string connectionString;

        public Accounts(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void UpdateUsername(string userId, string username, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;

                string table = "[dbo].[UserData]";

                command.CommandText =
                    $@" IF EXISTS ( SELECT * FROM {table} WHERE UserId = '{userId}' )
 
                        UPDATE {table}
                           SET Username = '{username}'
                           WHERE UserId = '{userId}';
 
                        ELSE 
 
                            INSERT {table} ( UserId, Username )
                            VALUES ( '{userId}', '{username}' );";

                connection.Open();

                command.ExecuteNonQuery();
            }
        }
    }
}
