using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class login_logout
    {//start of class

        // Method to log in a user
        public string login_user(string username, string password, out int userID)
        {//start of method login user

            // Temporary message
            string message = "Login failed...";

            // Default value in case the login fails
            userID = 0;

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Query to retrieve the user id and the stored password hash
            string query = @" select userID, password_hash from users where username = @username;";

            try
            {
                // Instance for connection
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    // Open the connection
                    connection.Open();

                    // Instance of SqlCommand
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@username", username);

                        // Retrieve the user details
                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Check whether the user exists
                            if (reader.Read())
                            {
                                string stored_password_hash = reader["password_hash"].ToString();

                                // Verify the entered password
                                bool password_correct =
                                    BCrypt.Net.BCrypt.Verify(
                                        password,
                                        stored_password_hash);

                                // Check the password
                                if (password_correct)
                                {
                                    userID = Convert.ToInt32(reader["userID"]);
                                    message = "Login successful...";
                                }
                                else
                                {
                                    message = "Invalid username or password...";
                                }
                            }
                            else
                            {
                                message = "Invalid username or password...";
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
        }

        // Method to log out a user
        public string logout_user()
        {
            // Temporary message
            string message = "Logout failed...";

            try
            {
                // Logout confirmation
                message = "Logout method reached successfully...";
            }
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;
        }//end of method lgout user

    }//end of class
}//end of namespace
