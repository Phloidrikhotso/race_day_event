using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class login_logout
    {//start of class

        // Method to log in a user
        public string login_user(string email, string password, out int user_id, out string role_name)
        {
            // Temporary message
            string message = "Login failed...";

            // Default values in case the login fails
            user_id = 0;
            role_name = "";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Query to retrieve the user id, the role and the stored password hash
            string query = @" select u.userID, u.password_hash, r.role_name
                from users u inner join roles r on u.role_id = r.role_id
                where u.email = @email;";

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
                        run_query.Parameters.AddWithValue("@email", email);

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
                                    user_id = Convert.ToInt32(reader["userID"]);
                                    role_name = reader["role_name"].ToString();
                                    message = "Login successful...";
                                }
                                else
                                {
                                    message = "Invalid email or password...";
                                }
                            }
                            else
                            {
                                message = "Invalid email or password...";
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
        }


    }//end of class
}//end of namespace
