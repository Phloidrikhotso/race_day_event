using System.Data.SqlClient;

namespace race_day_event.Database
{//start of namespace
    public class manage_results
    {//start of class

        // Connection string (used by all the methods in this class)
        private string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

        // Method to capture the result of an enrolment for the owning organizer
        public string add_result(int enrolment_id, int organizer_id, TimeSpan? finish_time,
            int? overall_position, int? category_position, string results_status)
        {//start of method add result

            // Temporary message
            string message = "Result capture failed...";

            // Query to find the owner of the event and the status of the enrolment
            string owner_query = @" select e.organizer_id, en.enrolment_status
                from enrolment en inner join event e on en.event_id = e.event_id
                where en.enrolment_id = @enrolment_id;";

            // Query to check whether a result already exists
            string result_query = @" select count(*) from results where enrolment_id = @enrolment_id;";

            // Query
            string query = @" insert into results (enrolment_id, finish_time, overall_position, category_position, results_status)
                values (@enrolment_id, @finish_time, @overall_position, @category_position, @results_status);";

            try
            {//start of try method

                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of uing method

                    //open connection
                    connection.Open();

                    // Check that the enrolment exists and who owns the event
                    int owner_id = 0;
                    string status = "";
                    bool found = false;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {//start of using check owner

                        check_owner.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        using (SqlDataReader reader = check_owner.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {
                                found = true;
                                owner_id = Convert.ToInt32(reader["organizer_id"]);
                                status = reader["enrolment_status"].ToString();
                            }
                        }//end of using 
                    }//end of using check owner

                    if (!found)
                    {//start of if found 

                        //return message to the user
                        return "Enrolment not found...";

                    }//end of if found

                    if (owner_id != organizer_id)
                    {//start of if 

                        //return message 
                        return "Forbidden: you are not the owning organizer...";
                    }//end of if 

                    if (status == "Cancelled")
                    {//start of if status

                        return "Conflict: a cancelled enrolment cannot have a result...";
                    }//end of if status

                    // Check whether a result already exists
                    int result_count = 0;

                    using (SqlCommand check_result = new SqlCommand(result_query, connection))
                    {
                        check_result.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        result_count = Convert.ToInt32(check_result.ExecuteScalar());
                    }

                    if (result_count > 0)
                    {
                        return "Conflict: a result already exists for this enrolment, use PUT to update it...";
                    }

                    // Capture the result
                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        run_query.Parameters.AddWithValue("@finish_time", finish_time.HasValue
                                ? (object)finish_time.Value : DBNull.Value);

                        run_query.Parameters.AddWithValue("@overall_position", overall_position.HasValue
                                ? (object)overall_position.Value : DBNull.Value);

                        run_query.Parameters.AddWithValue("@category_position", category_position.HasValue
                                ? (object)category_position.Value : DBNull.Value);

                        run_query.Parameters.AddWithValue("@results_status", results_status);

                        // Execute the query
                        int affected_row = run_query.ExecuteNonQuery();

                        if (affected_row > 0)
                        {
                            message = "Result captured successfully...";
                        }
                    }
                }
            }//end of try method
            catch (SqlException error)
            {//start of catch method

                // 2627 and 2601 are unique key errors (a result already exists)
                if (error.Number == 2627 || error.Number == 2601)
                {
                    message = "Conflict: a result already exists for this enrolment, use PUT to update it...";
                }
                else
                {
                    message = error.Message;
                }
            }//end of catch method
            catch (Exception error)
            {//start of catch method

                message = error.Message;

            }//end of catch method

            return message;
        }//end of add result

        // Method to update a captured result for the owning organizer
        public string update_result(int enrolment_id, int organizer_id, TimeSpan? finish_time,
            int? overall_position, int? category_position, string results_status)
        {//start of update result method

            // Temporary message
            string message = "Result update failed...";

            // Query to find the owner of the event that the enrolment belongs to
            string owner_query = @" select e.organizer_id
                from enrolment en inner join event e on en.event_id = e.event_id
                where en.enrolment_id = @enrolment_id;";

            // Query
            string query = @" update results set finish_time = @finish_time, overall_position = @overall_position,
                category_position = @category_position, results_status = @results_status
                where enrolment_id = @enrolment_id;";

            //try and catch method
            try
            {//start of try method
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using method

                    //open connection 
                    connection.Open();

                    // Check that the enrolment exists and who owns the event
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        owner = check_owner.ExecuteScalar();
                    }

                    if (owner == null)
                    {//start of if null

                        message = "Enrolment not found...";

                    }//end of if null

                    else if (Convert.ToInt32(owner) != organizer_id)
                    {//start of else if

                        message = "Forbidden: you are not the owning organizer...";

                    }//end of else if
                    else
                    {//start of else using 
                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {//start of using run query

                            // Add parameters
                            run_query.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                            run_query.Parameters.AddWithValue("@finish_time", finish_time.HasValue
                                    ? (object)finish_time.Value : DBNull.Value);

                            run_query.Parameters.AddWithValue("@overall_position", overall_position.HasValue
                                    ? (object)overall_position.Value : DBNull.Value);

                            run_query.Parameters.AddWithValue("@category_position", category_position.HasValue
                                    ? (object)category_position.Value : DBNull.Value);

                            run_query.Parameters.AddWithValue("@results_status", results_status);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Result updated successfully...";
                            }
                            else
                            {
                                message = "Result not found...";
                            }
                        }//end of using run query

                    }//end of else method
                }//endof using method
            }//end of try method
            catch (SqlException error)
            {//start of catch  SqlException
                message = error.Message;
            }//end of catch  SqlException
            catch (Exception error)
            {//start of catch exception

                message = error.Message;
            }//end of catch exception 

            return message;
        }//end of update result method

        // Method to list the logged in participant's results
        public string get_my_results(int user_id)
        {//start of get my results method

            // Temporary message
            string message = "";

            // SQL query
            string query = @" select r.result_id, e.event_name, e.event_date, c.category_name,
                r.finish_time, r.overall_position, r.category_position, r.results_status
                from results r
                inner join enrolment en on r.enrolment_id = en.enrolment_id
                inner join event e on en.event_id = e.event_id
                inner join category c on en.category_id = c.category_id
                where en.userID = @userID order by e.event_date desc;";

            try
            {//start of try method 
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {//start of using run query

                        // Add parameters
                        run_query.Parameters.AddWithValue("@userID", user_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Read all the results
                            while (reader.Read())
                            {
                                message += reader["result_id"].ToString() + " - "
                                    + reader["event_name"].ToString() + " - "
                                    + Convert.ToDateTime(reader["event_date"]).ToString("yyyy-MM-dd HH:mm") + " - "
                                    + reader["category_name"].ToString() + " - "
                                    + reader["finish_time"].ToString() + " - "
                                    + reader["overall_position"].ToString() + " - "
                                    + reader["category_position"].ToString() + " - "
                                    + reader["results_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using 
                    }//end of using run query
                }//end of using connection

                // Check whether any results were found
                if (message == "")
                {
                    message = "No results found...";
                }
            }//end of try 
            catch (SqlException error)
            {//start of catch SqlException

                message = error.Message;

            }//start of catch SqlException
            catch (Exception error)
            {//start of exception 

                message = error.Message;
            }//end of exception

            //return the message to the user
            return message;

        }//end of get my results method

        // Method to list the results of an event owned by the organizer
        public string get_event_results(int event_id, int organizer_id)
        {//start of get event results

            // Temporary message
            string message = "";

            // Query to find the owner of the event
            string owner_query = @" select organizer_id from event where event_id = @event_id;";

            // SQL query
            string query = @" select r.result_id, u.first_name, u.last_name, c.category_name,
                r.finish_time, r.overall_position, r.category_position, r.results_status
                from results r
                inner join enrolment en on r.enrolment_id = en.enrolment_id
                inner join users u on en.userID = u.userID
                inner join category c on en.category_id = c.category_id
                where en.event_id = @event_id
                order by case when r.overall_position is null then 1 else 0 end, r.overall_position;";

            try
            {//start of try 
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

                    //open connection
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
                        return "Event not found...";
                    }

                    if (Convert.ToInt32(owner) != organizer_id)
                    {
                        return "Forbidden: you are not the owning organizer...";
                    }

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        run_query.Parameters.AddWithValue("@event_id", event_id);

                        using (SqlDataReader reader = run_query.ExecuteReader())
                        {
                            // Read all the results
                            while (reader.Read())
                            {
                                message += reader["result_id"].ToString() + " - "
                                    + reader["first_name"].ToString() + " "
                                    + reader["last_name"].ToString() + " - "
                                    + reader["category_name"].ToString() + " - "
                                    + reader["finish_time"].ToString() + " - "
                                    + reader["overall_position"].ToString() + " - "
                                    + reader["category_position"].ToString() + " - "
                                    + reader["results_status"].ToString()
                                    + Environment.NewLine;
                            }//end of while
                        }//end of using run query
                    }//end of using connection

                    // Check whether any results were found
                    if (message == "")
                    {
                        message = "No results found for this event...";
                    }
                }//end of using
            }//end of try
            catch (SqlException error)
            {//start of catch

                message = error.Message;
            }//end of catch
            catch (Exception error)
            {//start of catch exception

                message = error.Message;

            }//end of exception

            return message;
        }//end of get event results

    }//end of class
}//end of namespace
