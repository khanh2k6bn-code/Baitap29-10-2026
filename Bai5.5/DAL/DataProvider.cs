using System.Data;
using MySql.Data.MySqlClient;

namespace InventoryManagementSystem.DAL
{
    public class DataProvider
    {
        private string connectionString = "Server=localhost;Port=3306;Database=InventoryDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public DataTable ExecuteQuery(string query, MySqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public object ExecuteScalar(string query, MySqlParameter[] parameters = null)
        {
            object result = null;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    result = cmd.ExecuteScalar();
                }
            }
            return result;
        }

        public int ExecuteNonQuery(string query, MySqlParameter[] parameters = null)
        {
            int rows = 0;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    rows = cmd.ExecuteNonQuery();
                }
            }
            return rows;
        }
    }
}
