using System.Data.SqlClient;

namespace race_day_event.Database
{
    public class manage_results
    {

        // Connection string (used by all the methods in this class)
        private string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

        // Method to capture the result of an enrolment for the owning organizer
        public string add_result(int enrolment_id, int organizer_id, TimeSpan? finish_time,
            int? overall_position, int? category_position, string results_status)
        {
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
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the enrolment exists and who owns the event
                    int owner_id = 0;
                    string status = "";
                    bool found = false;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        using (SqlDataReader reader = check_owner.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                found = true;
                                owner_id = Convert.ToInt32(reader["organizer_id"]);
                                status = reader["enrolment_status"].ToString();
                            }
                        }
                    }

                    if (!found)
                    {
                        return "Enrolment not found...";
                    }

                    if (owner_id != organizer_id)
                    {
                        return "Forbidden: you are not the owning organizer...";
                    }

                    if (status == "Cancelled")
                    {
                        return "Conflict: a cancelled enrolment cannot have a result...";
                    }

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
            }
            catch (SqlException error)
            {
                // 2627 and 2601 are unique key errors (a result already exists)
                if (error.Number == 2627 || error.Number == 2601)
                {
                    message = "Conflict: a result already exists for this enrolment, use PUT to update it...";
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

        // Method to update a captured result for the owning organizer
        public string update_result(int enrolment_id, int organizer_id, TimeSpan? finish_time,
            int? overall_position, int? category_position, string results_status)
        {
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

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the enrolment exists and who owns the event
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@enrolment_id", enrolment_id);

                        owner = check_owner.ExecuteScalar();
                    }

                    if (owner == null)
                    {
                        message = "Enrolment not found...";
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

        // Method to list the logged in participant's results
        public string get_my_results(int user_id)
        {
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
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    using (SqlCommand run_query = new SqlCommand(query, connection))
                    {
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
                            }
                        }
                    }
                }

                // Check whether any results were found
                if (message == "")
                {
                    message = "No results found...";
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

        // Method to list the results of an event owned by the organizer
        public string get_event_results(int event_id, int organizer_id)
        {
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
                            }
                        }
                    }

                    // Check whether any results were found
                    if (message == "")
                    {
                        message = "No results found for this event...";
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

    }
}
