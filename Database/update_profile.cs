using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class update_profile
    {//start of class

        // Method to update the logged in user's profile
        public string update_user( int user_id, string first_name, string last_name, string username, string email, string phone_number)
        {//start of update user method

            // Temporary message
            string message = "Profile update failed...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Query
            string query = @" update users set first_name = @first_name, last_name = @last_name,
                username = @username, email = @email, phone_number = @phone_number
                where userID = @userID;";

            try
            {//start of try method 
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using  connection object
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run_query

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
                        {//start of if affected row
                            message = "Profile updated successfully...";
                        }//end of if affected
                        else
                        {//start of else method

                            //message to display if updating the user fails
                            message = "Update failed: user not found...";
                        }//end of else
                    }//end of using run_query
                }//end of using connection
            }//end of try
            catch (SqlException error)
            {//start of catch 
                if (error.Number == 2627 || error.Number == 2601)
                {//start of if error

                    //message to display if username or email already exist
                    message = "Username or email already exists...";
                }//end of if
                else
                {//start of else

                    //error message
                    message = error.Message;
                }//end of else
            }//end of carch
            catch (Exception error)
            {//start of catch 

                message = error.Message;
            }//end of catch

            //return message
            return message;
        }//end of update user method

    }//end of class
}//end of namespace
