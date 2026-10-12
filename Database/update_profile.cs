using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class update_profile
    {//start of class

        // Method to update the logged in user's profile
        public string update_user( int user_id, string first_name, string last_name, string username, string email, string phone_number)
        {
            // Temporary message
            string message = "Profile update failed...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Query
            string query = @" update users set first_name = @first_name, last_name = @last_name,
                username = @username, email = @email, phone_number = @phone_number
                where userID = @userID;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@userID", user_id);

                        run_query.Parameters.AddWithValue("@first_name", first_name);

                        run_query.Parameters.AddWithValue("@last_name", last_name);

                        run_query.Parameters.AddWithValue("@username", username);

                        run_query.Parameters.AddWithValue("@email", email);

                        run_query.Parameters.AddWithValue("@phone_number", string.IsNullOrWhiteSpace(phone_number)
                                ? (object)DBNull.Value : phone_number);

                        // Execute the query
                        int affected_row = run_query.ExecuteNonQuery();

                        if (affected_row > 0)
                        {
                            message = "Profile updated successfully...";
                        }
                        else
                        {
                            message = "Update failed: user not found...";
                        }
                    }
                }
            }
            catch (SqlException error)
            {
                if (error.Number == 2627 || error.Number == 2601)
                {
                    message = "Username or email already exists...";
                }
                else
                {
                    message = error.Message;
                }
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }

    }//end of class
}//end of namespace
