using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Controllers;
using race_day_event.Database;
using System;
using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class register_user
    {//start of class

        // Method to register a new user
        public string add_user( string role_name,string first_name, string last_name, string username,
            string email, string phone_number, string password)
        {//start of method 
            // Temporary message
            string message = "User registration failed...";

            // Connection string
            string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

            // Hash the user's password
            string hashed_password = BCrypt.Net.BCrypt.HashPassword(password);

            // Query
            string query = @" INSERT INTO users (role_id, first_name, last_name, username,email, phone_number, password_hash)
                SELECT role_id, @first_name, @last_name, @username, @email, @phone_number, @password_hash FROM roles
                where role_name = @role_name;";

            try
            {//start of try method

                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query

                        // Add parameters
                        run_query.Parameters.AddWithValue("@role_name", role_name);

                        run_query.Parameters.AddWithValue("@first_name", first_name);

                        run_query.Parameters.AddWithValue("@last_name", last_name);

                        run_query.Parameters.AddWithValue("@username", username);

                        run_query.Parameters.AddWithValue("@email", email);

                        run_query.Parameters.AddWithValue("@phone_number", string.IsNullOrWhiteSpace(phone_number)
                                ? (object)DBNull.Value : phone_number);

                        run_query.Parameters.AddWithValue("@password_hash", hashed_password);

                        // Execute the query
                        int affected_row = run_query.ExecuteNonQuery();

                        if (affected_row > 0)
                        {//start of if affected row

                            message = "User registered successfully...";

                        }//end of if statement
                        else
                        {//start of else

                            message = "Registration failed: role not found...";

                        }//end of else

                    }//end of using connection
                }//end of try method
            }//end of try method 
            catch (SqlException error)
            {//start of catch error
                if (error.Number == 2627 || error.Number == 2601)
                {
                    message = "Username or email already exists...";
                }
                else
                {
                    message = error.Message;
                }
            }//end of catch
            catch (Exception error)
            {//start of catch

                message = error.Message;
            }//end of catch

            return message;

        }//end of method register user
    }//end of class
}//end of namespace

