using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class manage_events
    {//start of manage events class

        // Connection string (used by all the methods in this class)
        private string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

        // Method to list all upcoming events that are open for enrolment
        public string get_all_events()
        {//start of get all events method

            // Temporary message
            string message = "";

            // SQL query
            string query = @" select event_id, event_name, event_description, event_date, event_location,
                distance, event_type, event_status
                from event where event_date >= getdate() and event_status = 'Open' order by event_date;";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using 

                    //open connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of run query

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {//start of using

                            // Read all the events
                            while (reader.Read())
                            {//start of while
                                message += reader["event_id"].ToString() + " - "
                                    + reader["event_name"].ToString() + " - "
                                    + reader["event_description"].ToString() + " - "
                                    + Convert.ToDateTime(reader["event_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["event_location"].ToString() + " - "
                                    + reader["distance"].ToString() + " - "
                                    + reader["event_type"].ToString() + " - "
                                    + reader["event_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using
                    }//end of run query using
                }//end of using connection

                // Check whether any events were found
                if (message == "")
                {
                    message = "No open upcoming events found...";
                }
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
        }//end of get all events

        // Method to list the events organized by the logged in organizer
        public string get_my_events(int organizer_id)
        {//start of get my events

            // Temporary message
            string message = "";

            // SQL query
            string query = @" select event_id, event_name, event_description, event_date, event_location,
                distance, event_type, event_status
                from event where organizer_id = @organizer_id order by event_date desc;";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@organizer_id", organizer_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Read all the events
                            while (reader.Read())
                            {
                                message += reader["event_id"].ToString() + " - "
                                    + reader["event_name"].ToString() + " - "
                                    + reader["event_description"].ToString() + " - "
                                    + Convert.ToDateTime(reader["event_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["event_location"].ToString() + " - "
                                    + reader["distance"].ToString() + " - "
                                    + reader["event_type"].ToString() + " - "
                                    + reader["event_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using
                    }//end of using run query

                    //close connection
                    connection.Close();
                }//end of using connection

                // Check whether any events were found
                if (message == "")
                {
                    message = "No events found...";
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

            //return message to the user
            return message;

        }// of get my events method

        // Method to get the details of one event
        public string get_one_event(int event_id)
        {//start of get one event method

            // Temporary message
            string message = "Event not found...";

            // SQL query
            string query = @" select event_id, organizer_id, event_name, event_description, event_date,
                event_location, distance, event_type, event_status
                from event where event_id = @event_id;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@event_id", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Read the event
                            if (reader.Read())
                            {
                                message = reader["event_id"].ToString() + " - "
                                    + reader["event_name"].ToString() + " - "
                                    + reader["event_description"].ToString() + " - "
                                    + Convert.ToDateTime(reader["event_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["event_location"].ToString() + " - "
                                    + reader["distance"].ToString() + " - "
                                    + reader["event_type"].ToString() + " - "
                                    + reader["event_status"].ToString();
                            }//end of while

                        }//end of using

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

            //return message to the user
            return message;

        }//end of get one event method

        // Method to create a new event for an organizer
        public string add_event(int organizer_id, string event_name, string event_description,
            DateTime event_date, string event_location, decimal distance, string event_type, string event_status)
        {//start of add event method

            // Temporary message
            string message = "Event creation failed...";

            // Query
            string query = @" insert into event (organizer_id, event_name, event_description, event_date,
                event_location, distance, event_type, event_status)
                values (@organizer_id, @event_name, @event_description, @event_date,
                @event_location, @distance, @event_type, @event_status);";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query

                        // Add parameters
                        run_query.Parameters.AddWithValue("@organizer_id", organizer_id);

                        run_query.Parameters.AddWithValue("@event_name", event_name);

                        run_query.Parameters.AddWithValue("@event_description", string.IsNullOrWhiteSpace(event_description)
                                ? (object)DBNull.Value : event_description);

                        run_query.Parameters.AddWithValue("@event_date", event_date);

                        run_query.Parameters.AddWithValue("@event_location", event_location);

                        run_query.Parameters.AddWithValue("@distance", distance);

                        run_query.Parameters.AddWithValue("@event_type", event_type);

                        run_query.Parameters.AddWithValue("@event_status", event_status);

                        // Execute the query
                        int affected_row = run_query.ExecuteNonQuery();

                        if (affected_row > 0)
                        {
                            message = "Event created successfully...";
                        }
                    }//end of using run query
                }//end of using connection
            }//end of try 
            catch (SqlException error)
            {//start of catch sqlexception

                message = error.Message;
            }//end of catch sqlexception
            catch (Exception error)
            {
                message = error.Message;
            }

            return message;

        }//end of add event method

        // Method to update an event owned by the organizer
        public string update_event(int event_id, int organizer_id, string event_name, string event_description,
            DateTime event_date, string event_location, decimal distance, string event_type, string event_status)
        {//start of update event 

            // Temporary message
            string message = "Event update failed...";

            // Query to find the owner of the event
            string owner_query = @" select organizer_id from event where event_id = @event_id;";

            // Query
            string query = @" update event set event_name = @event_name, event_description = @event_description,
                event_date = @event_date, event_location = @event_location, distance = @distance,
                event_type = @event_type, event_status = @event_status
                where event_id = @event_id;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the event exists and who owns it
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@event_id", event_id);

                        owner = check_owner.ExecuteScalar();
                    }

                    if (owner == null)
                    {
                        message = "Event not found...";
                    }
                    else if (Convert.ToInt32(owner) != organizer_id)
                    {
                        message = "Forbidden: you are not the owning organizer...";
                    }
                    else
                    {
                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            run_query.Parameters.AddWithValue("@event_id", event_id);

                            run_query.Parameters.AddWithValue("@event_name", event_name);

                            run_query.Parameters.AddWithValue("@event_description", string.IsNullOrWhiteSpace(event_description)
                                    ? (object)DBNull.Value : event_description);

                            run_query.Parameters.AddWithValue("@event_date", event_date);

                            run_query.Parameters.AddWithValue("@event_location", event_location);

                            run_query.Parameters.AddWithValue("@distance", distance);

                            run_query.Parameters.AddWithValue("@event_type", event_type);

                            run_query.Parameters.AddWithValue("@event_status", event_status);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Event updated successfully...";
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
        }//end of update event

        // Method to delete an event owned by the organizer
        public string delete_event(int event_id, int organizer_id)
        {//start of delete event 

            // Temporary message
            string message = "Event deletion failed...";

            // Query to find the owner of the event
            string owner_query = @" select organizer_id from event where event_id = @event_id;";

            // Query
            string query = @" delete from event where event_id = @event_id;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the event exists and who owns it
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@event_id", event_id);

                        owner = check_owner.ExecuteScalar();
                    }

                    if (owner == null)
                    {
                        message = "Event not found...";
                    }
                    else if (Convert.ToInt32(owner) != organizer_id)
                    {
                        message = "Forbidden: you are not the owning organizer...";
                    }
                    else
                    {
                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            run_query.Parameters.AddWithValue("@event_id", event_id);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Event deleted successfully...";
                            }
                        }
                    }
                }
            }
            catch (SqlException error)
            {
                // 547 is a foreign key error (the event still has categories or enrolments)
                if (error.Number == 547)
                {
                    message = "Cannot delete: the event still has categories or enrolments...";
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
        }//end of delete event method

    }//end of class
}//end of namespace
