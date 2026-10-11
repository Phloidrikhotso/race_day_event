using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class login_logout
    {//start of class

        // Method to log in a user
        public string login_user(string username, string password)
        {//start of method login user

            // Temporary message
            string message = "Login failed...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Query to retrieve the stored password hash
            string query = @" select password_hash from users where username = @username;";

            try
            {//start of try method

                // Instance for connection
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    // Open the connection
                    connection.Open();

                    // Instance of SqlCommand
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using method 

                        // Add parameters
                        run_query.Parameters.AddWithValue("@username", username);

                        // Retrieve the stored password hash
                        object result = run_query.ExecuteScalar();

                        // Check whether the user exists
                        if (result != null && result != DBNull.Value)
                        {//start of if statement 

                            string stored_password_hash = result.ToString();

                            // Verify the entered password
                            bool password_correct = BCrypt.Net.BCrypt.Verify( password, stored_password_hash);

                            // Check the password
                            if (password_correct)
                            {//start of if staement

                                message = "Login successful...";
                            }//end of if statement 
                            else
                            {//start of else

                                message = "Invalid username or password...";
                            }//end of else
                        }//end of if staement
                        else
                        {//start of else statement

                            message = "Invalid username or password...";
                        }//end of else statement
                    }//end of using statement 
                }//end of using statement 
            }//end of try method

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

        // Method to log out a user
        public string logout_user()
        {//start of method logout user

            // Temporary message
            string message = "Logout failed...";

            try
            {//start of try method

                // Logout confirmation
                message = "Logout method reached successfully...";

            }//end of try method

            catch (Exception error)
            {//start of catch

                message = error.Message;
            }//end of catch method

            return message;
        }//end of method lgout user

    }//end of class
}//end of namespace
