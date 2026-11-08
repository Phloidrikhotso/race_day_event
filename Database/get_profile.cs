using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class get_profile
    {//start of class

        // Method to retrieve the logged in user's profile
        public string get_user_profile(int user_id)
        {//start of get user profile

            // Temporary message
            string message = "User not found...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // SQL query
            string query = @" select u.userID, u.first_name, u.last_name, u.username, u.email, u.phone_number,
                u.profile_picture_url, r.role_name
                from users u inner join roles r on u.role_id = r.role_id
                where u.userID = @userID;";

            try
            {//start of try

                // Instance for connection
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    // Open the connection
                    connection.Open();

                    // Instance of SqlCommand to run the query
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query

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
                                    + reader["phone_number"].ToString() + " - "
                                    + reader["profile_picture_url"].ToString() + " - "
                                    + reader["role_name"].ToString() + " - "
                                    + reader["userID"].ToString();
                            }
                        }
                    }//end of using run query
                }//end of using connection
            }//end of try 
            catch (SqlException error)
            {
                message = error.Message;
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;


        }//end of get user profile method
    }//end of class
}//end of namespace
