using System.Data.SqlClient;

namespace race_day_event.Database
{
    public class get_roles
    {
        // Method to retrieve all roles
        public string get_role_info()
        {
            string message = "";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // SQL query
            string query = @"select role_id, role_name, role_description from roles;";

            try
            {
                // Instance for connection
                SqlConnection connection = new SqlConnection(connection_string);

                // Open the connection
                connection.Open();

                // Instance of SqlCommand to run the query
                SqlCommand run_query = new SqlCommand(query, connection);

                // Execute the query
                SqlDataReader reader = run_query.ExecuteReader();

                // Read the results
                while (reader.Read())
                {
                    message += reader["role_id"].ToString() + " - "
                        + reader["role_name"].ToString() + " - "
                        + reader["role_description"].ToString()
                        + Environment.NewLine;
                }

                // Close the reader
                reader.Close();

                // Close the connection
                connection.Close();
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }
    }
}