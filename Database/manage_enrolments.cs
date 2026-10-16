using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class manage_enrolments
    {//start of class

        // Connection string (used by all the methods in this class)
        private string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

        // Method to enrol a participant into an event under a category
        public string add_enrolment(int event_id, int user_id, int category_id)
        {//start of add enrolment
            // Temporary message
            string message = "Enrolment failed...";

            // Query to check that the event exists and get its status
            string event_query = @" select event_status from event where event_id = @event_id;";

            // Query to check that the category belongs to the event and get its limit
            string category_query = @" select max_participants from category
                where category_id = @category_id and event_id = @event_id;";

            // Query to check whether the participant is already enrolled in the event
            string enrolled_query = @" select count(*) from enrolment
                where userID = @userID and event_id = @event_id and enrolment_status <> 'Cancelled';";

            // Query to count the people already in the category
            string full_query = @" select count(*) from enrolment
                where category_id = @category_id and enrolment_status <> 'Cancelled';";

            // Query
            string query = @" insert into enrolment (userID, event_id, category_id)
                values (@userID, @event_id, @category_id);";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    // Check that the event exists and get its status
                    object event_status = null;

                    using (SqlCommand check_event = new SqlCommand(event_query, connection))
                    {
                        check_event.Parameters.AddWithValue("@event_id", event_id);

                        event_status = check_event.ExecuteScalar();
                    }

                    if (event_status == null)
                    {
                        return "Event not found...";
                    }

                    // Check that the event is open for enrolment
                    if (event_status.ToString() != "Open")
                    {
                        return "Conflict: the event is not open for enrolment...";
                    }

                    // Check that the category belongs to the event
                    object max_participants = null;

                    using (SqlCommand check_category = new SqlCommand(category_query, connection))
                    {//start of using
                        check_category.Parameters.AddWithValue("@category_id", category_id);

                        check_category.Parameters.AddWithValue("@event_id", event_id);

                        max_participants = check_category.ExecuteScalar();
                    }//end of using 

                    if (max_participants == null)
                    {
                        return "Invalid category: it does not exist or does not belong to this event...";
                    }

                    // Check whether the participant is already enrolled
                    int enrolled_count = 0;

                    using (SqlCommand check_enrolled = new SqlCommand(enrolled_query, connection))
                    {//start of using

                        check_enrolled.Parameters.AddWithValue("@userID", user_id);

                        check_enrolled.Parameters.AddWithValue("@event_id", event_id);

                        enrolled_count = Convert.ToInt32(check_enrolled.ExecuteScalar());
                    }//end of using 

                    if (enrolled_count > 0)
                    {
                        return "Conflict: you are already enrolled in this event...";
                    }

                    // Check whether the category is full
                    int category_count = 0;

                    using (SqlCommand check_full = new SqlCommand(full_query, connection))
                    {//start of using

                        check_full.Parameters.AddWithValue("@category_id", category_id);

                        category_count = Convert.ToInt32(check_full.ExecuteScalar());
                    }//end of using

                    if (category_count >= Convert.ToInt32(max_participants))
                    {
                        return "Conflict: this category is full...";
                    }

                    // Create the enrolment
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of run query

                        // Add parameters
                        run_query.Parameters.AddWithValue("@userID", user_id);

                        run_query.Parameters.AddWithValue("@event_id", event_id);

                        run_query.Parameters.AddWithValue("@category_id", category_id);

                        // Execute the query
                        int affected_row = run_query.ExecuteNonQuery();

                        if (affected_row > 0)
                        {
                            message = "Enrolment created successfully...";
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
        }//end of add enrolments

        // Method to list the logged in participant's enrolments
        public string get_my_enrolments(int user_id)
        {//start of get my enrolments

            // Temporary message
            string message = "";

            // SQL query
            string query = @" select en.enrolment_id, e.event_name, e.event_date, c.category_name,
                en.enrolment_date, en.enrolment_status
                from enrolment en
                inner join event e on en.event_id = e.event_id
                inner join category c on en.category_id = c.category_id
                where en.userID = @userID order by en.enrolment_date desc;";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query
                        // Add parameters
                        run_query.Parameters.AddWithValue("@userID", user_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {//start of using
                            // Read all the enrolments
                            while (reader.Read())
                            {//start of while
                                message += reader["enrolment_id"].ToString() + " - "
                                    + reader["event_name"].ToString() + " - "
                                    + Convert.ToDateTime(reader["event_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["category_name"].ToString() + " - "
                                    + Convert.ToDateTime(reader["enrolment_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["enrolment_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using 
                    }//end of using run query
                }//end of using connection

                // Check whether any enrolments were found
                if (message == "")
                {
                    message = "No enrolments found...";
                }
            }//end of try
            catch (SqlException error)
            {
                message = error.Message;
            }
            catch (Exception error)
            {//start of catch
                message = error.Message;
            }//end of catch

            return message;
        }//end of get my enrolments

        // Method to list the participants enrolled in an event owned by the organizer
        public string get_event_enrolments(int event_id, int organizer_id)
        {//start of get event enrolments

            // Temporary message
            string message = "";

            // Query to find the owner of the event
            string owner_query = @" select organizer_id from event where event_id = @event_id;";

            // SQL query
            string query = @" select en.enrolment_id, u.first_name, u.last_name, u.username,
                c.category_name, en.enrolment_date, en.enrolment_status
                from enrolment en
                inner join users u on en.userID = u.userID
                inner join category c on en.category_id = c.category_id
                where en.event_id = @event_id order by en.enrolment_date;";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    // Check that the event exists and who owns it
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {//start of using check owner

                        check_owner.Parameters.AddWithValue("@event_id", event_id);

                        owner = check_owner.ExecuteScalar();
                    }//end of using check owner

                    if (owner == null)
                    {
                        return "Event not found...";
                    }

                    if (Convert.ToInt32(owner) != organizer_id)
                    {
                        return "Forbidden: you are not the owning organizer...";
                    }

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query
                        // Add parameters
                        run_query.Parameters.AddWithValue("@event_id", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {//start of using
                            // Read all the enrolments
                            while (reader.Read())
                            {//start of while
                                message += reader["enrolment_id"].ToString() + " - "
                                    + reader["first_name"].ToString() + " "
                                    + reader["last_name"].ToString() + " - "
                                    + reader["username"].ToString() + " - "
                                    + reader["category_name"].ToString() + " - "
                                    + Convert.ToDateTime(reader["enrolment_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["enrolment_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using
                    }//end of usingrun query

                    // Check whether any enrolments were found
                    if (message == "")
                    {
                        message = "No enrolments found for this event...";
                    }
                }//end of using connection
            }//end of try
            catch (SqlException error)
            {//start of catch

                message = error.Message;
            }//end of catch
            catch (Exception error)
            {//start of catch

                message = error.Message;
            }//end of catch

            return message;
        }//end of get events enrolments

        // Method to cancel the logged in participant's own enrolment
        public string cancel_enrolment(int enrolment_id, int user_id)
        {//end of cancel enrolment method

            // Temporary message
            string message = "Enrolment cancellation failed...";

            // Query to find the owner and the status of the enrolment
            string owner_query = @" select userID, enrolment_status from enrolment where enrolment_id = @enrolment_id;";

            // Query
            string query = @" update enrolment set enrolment_status = 'Cancelled' where enrolment_id = @enrolment_id;";

            try
            {//start of try 
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    // Check that the enrolment exists and who owns it
                    int owner_id = 0;
                    string status = "";
                    bool found = false;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {//start of using

                        check_owner.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        using (SqlDataReader reader = check_owner.ExecuteReader())
                        {//start of using 
                            if (reader.Read())
                            {//start of if 
                                found = true;
                                owner_id = Convert.ToInt32(reader["userID"]);
                                status = reader["enrolment_status"].ToString();
                            }//end of if
                        }//end of using 
                    }//end of using check owner 

                    if (!found)
                    {
                        message = "Enrolment not found...";
                    }
                    else if (owner_id != user_id)
                    {
                        message = "Forbidden: this is not your enrolment...";
                    }
                    else if (status == "Cancelled")
                    {
                        message = "Conflict: the enrolment is already cancelled...";
                    }
                    else
                    {//start of else
                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {//start of run query

                            // Add parameters
                            run_query.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Enrolment cancelled successfully...";
                            }
                        }//end of using run query
                    }//end of using 
                }//end of using
            }//end of catch
            catch (SqlException error)
            {//start of catch

                message = error.Message;
                
            }//end of catch
            catch (Exception error)
            {//start of catch

                message = error.Message;
            }//end of catch

            return message;
        }//end of cancel enrolment method

    }//end of class
}//end of namespace 
