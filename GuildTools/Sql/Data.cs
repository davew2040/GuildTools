using GuildTools.Sql.SqlModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Sql
{
    public class Data
    {
        private static class Tables
        {
            public const string ValueStore = "[dbo].[ValueStore]";
        }

        private string connectionString;

        public Data(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SetStoredValue(string key, string value)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;

                string table = Tables.ValueStore;

                command.CommandText =
                    $@" IF EXISTS ( SELECT * FROM {table} WHERE Id = '{key}' )
 
                        UPDATE {table}
                           SET Value = '{value}'
                           WHERE Id = '{key}';
 
                        ELSE 
 
                            INSERT INTO {table} ( Id, Value )
                            VALUES ( '{key}', '{value}' );";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public string GetStoredValue(string key)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;

                string table = Tables.ValueStore;

                command.CommandText = $"SELECT Value FROM {table} WHERE Id = '{key}'";

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();

                    return reader["Value"].ToString();
                }
            }

            return string.Empty;
        }
        
        public void DeleteStoredValue(string key)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;

                string table = Tables.ValueStore;

                command.CommandText = $"DELETE FROM {table} WHERE Id = '{key}'";

                connection.Open();

                command.ExecuteNonQuery();
            }
        }
        
        public void SetCachedValue(string key, string value, string type, TimeSpan duration)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand("AddCachedValue"))
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@id", key));
                command.Parameters.Add(new SqlParameter("@value", value));
                command.Parameters.Add(new SqlParameter("@type", type));
                command.Parameters.Add(new SqlParameter("@expiresOn", DateTime.Now+duration));
                
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public CachedValue GetCachedValue(string key, string type)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand("GetCachedValue"))
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@id", key));
                command.Parameters.Add(new SqlParameter("@type", type));
                command.Parameters.Add("@Out_Id", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
                command.Parameters.Add("@Out_Type", SqlDbType.NVarChar, 50).Direction = ParameterDirection.Output;
                command.Parameters.Add("@Out_Value", SqlDbType.NVarChar, -1).Direction = ParameterDirection.Output;
                command.Parameters.Add("@Out_ExpiresOn", SqlDbType.DateTime).Direction = ParameterDirection.Output;
                
                connection.Open();

                int result = command.ExecuteNonQuery();

                if (command.Parameters["@Out_Id"].Value is DBNull)
                {
                    return null;
                }

                return new CachedValue()
                {
                    Key = command.Parameters["@Out_Id"].Value.ToString(),
                    Type = command.Parameters["@Out_Type"].Value.ToString(),
                    Value = command.Parameters["@Out_Value"].Value.ToString(),
                    ExpiresOn = (DateTime)command.Parameters["@Out_ExpiresOn"].Value
                };
            }
        }

        public GuildProfile GetGuildProfile(int id)
        {
            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand())
            {
                connection.ConnectionString = this.connectionString;

                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = $"SELECT * FROM GuildProfile WHERE Id = {id}";

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    string guildName = reader["GuildName"].ToString();
                    string realm = reader["realm"].ToString();

                    return new GuildProfile()
                    {
                        Name = guildName,
                        Realm = realm
                    };
                }

                return null;
            }
        }
    }
}
