using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class get_profile
    {//start of class

        // Method to retrieve the logged in user's profile
        public string get_user_profile(int user_id)
        {
            // Temporary message
            string message = "User not found...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // SQL query
            string query = @" select first_name, last_name, username, email, phone_number from users where userID = @userID;";

            try
            {
                // Instance for connection
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    // Open the connection
                    connection.Open();

                    // Instance of SqlCommand to run the query
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@userID", user_id);

                        // Execute the query
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Read the result
                            if (reader.Read())
                            {
                                message = reader["first_name"].ToString() + " - "
                                    + reader["last_name"].ToString() + " - "
                                    + reader["username"].ToString() + " - "
                                    + reader["email"].ToString() + " - "
                                    + reader["phone_number"].ToString();
                            }
                        }
                    }
                }
            }
            catch (SqlException error)
            {
                message = error.Message;
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;


        }//end of method
    }//end of class
}//end
