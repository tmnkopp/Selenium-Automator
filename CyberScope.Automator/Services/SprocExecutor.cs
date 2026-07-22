using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;

namespace CyberScope.Selenium
{
    public class SprocExecutor
    {
        public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();
        public SprocExecutor()
        {
        }
        public DataTable Execute(string SQL)
        {
            var dataTable = new DataTable();
            using (SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CAClientConnectionString"].ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(SQL, connection))
                {
                    if (!Regex.IsMatch(SQL, @"\s")) command.CommandType = CommandType.StoredProcedure;
                    foreach (var kvp in Params)
                        command.Parameters.AddWithValue($"@{kvp.Key.Replace("@", "")}", kvp.Value);

                    SqlDataReader reader = command.ExecuteReader();
                    dataTable.Load(reader);
                    reader.Close();
                }
            }
            return dataTable;
        }
    }

}
