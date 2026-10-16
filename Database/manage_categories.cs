using Microsoft.AspNetCore.Mvc;
using race_day_event.Controllers;
using race_day_event.Database;
using System.Data.SqlClient;
using System.Text.Json.Serialization;

namespace race_day_event.Database
{//start of namespace
    public class manage_categories
    {//start of class

        // Connection string (used by all the methods in this class)
        private string connection_string = @"Data source=(localdb)\api_both;database=raceday;";

        // Method to list all the categories of an event
        public string get_event_categories(int event_id)
        {//start of get event categories

            // Temporary message
            string message = "";

            // Query to check that the event exists
            string event_query = @" select count(*) from event where event_id = @event_id;";

            // SQL query
            string query = @" select category_id, category_name, entry_fee, race_distance, max_participants
                from category where event_id = @event_id order by category_id;";

            try
            {//start of try
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using

                    connection.Open();

                    // Check that the event exists
                    int event_count = 0;

                    using (SqlCommand check_event = new SqlCommand(event_query, connection))
                    {
                        check_event.Parameters.AddWithValue("@event_id", event_id);

                        event_count = Convert.ToInt32(check_event.ExecuteScalar());
                    }

                    if (event_count == 0)
                    {
                        message = "Event not found...";
                    }
                    else
                    {
                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            run_query.Parameters.AddWithValue("@event_id", event_id);

                            using (SqlDataReader reader = run_query.ExecuteReader())
                            {
                                // Read all the categories
                                while (reader.Read())
                                {
                                    message += reader["category_id"].ToString() + " - "
                                        + reader["category_name"].ToString() + " - "
                                        + reader["entry_fee"].ToString() + " - "
                                        + reader["race_distance"].ToString() + " - "
                                        + reader["max_participants"].ToString()
                                        + Environment.NewLine;
                                }
                            }
                        }

                        // Check whether any categories were found
                        if (message == "")
                        {
                            message = "No categories found for this event...";
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
        }//end of get event categories

        // Method to add a category to an event owned by the organizer
        public string add_category(int event_id, int organizer_id, string category_name,
            decimal entry_fee, decimal? race_distance, int max_participants)
        {//start of add category method 

            // Temporary message
            string message = "Category creation failed...";

            // Query to find the owner of the event
            string owner_query = @" select organizer_id from event where event_id = @event_id;";

            // Query to check whether the event already has a category with the same name and distance
            string duplicate_query = @" select count(*) from category
                where event_id = @event_id and category_name = @category_name
                and ((race_distance = @race_distance) or (race_distance is null and @race_distance is null));";

            // Query
            string query = @" insert into category (event_id, category_name, entry_fee, race_distance, max_participants)
                values (@event_id, @category_name, @entry_fee, @race_distance, @max_participants);";

            try
            {//start of try 
                using (SqlConnection connection = new SqlConnection(connection_string))
                {//start of using connection

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
                        // Check whether the same name and distance already exist in the event
                        int duplicate_count = 0;

                        using (SqlCommand check_duplicate = new SqlCommand(duplicate_query, connection))
                        {
                            check_duplicate.Parameters.AddWithValue("@event_id", event_id);

                            check_duplicate.Parameters.AddWithValue("@category_name", category_name);

                            check_duplicate.Parameters.AddWithValue("@race_distance", race_distance.HasValue
                                    ? (object)race_distance.Value : DBNull.Value);

                            duplicate_count = Convert.ToInt32(check_duplicate.ExecuteScalar());
                        }

                        if (duplicate_count > 0)
                        {
                            return "Conflict: this event already has a category with the same name and distance...";
                        }

                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            run_query.Parameters.AddWithValue("@event_id", event_id);

                            run_query.Parameters.AddWithValue("@category_name", category_name);

                            run_query.Parameters.AddWithValue("@entry_fee", entry_fee);

                            run_query.Parameters.AddWithValue("@race_distance", race_distance.HasValue
                                    ? (object)race_distance.Value : DBNull.Value);

                            run_query.Parameters.AddWithValue("@max_participants", max_participants);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Category created successfully...";
                            }
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
        }//end of add category method

        // Method to update a category of an event owned by the organizer
        public string update_category(int category_id, int organizer_id, string category_name,
            decimal entry_fee, decimal? race_distance, int max_participants)
        {//start of update category

            // Temporary message
            string message = "Category update failed...";

            // Query to find the owner of the event that the category belongs to
            string owner_query = @" select e.organizer_id, c.event_id from category c
                inner join event e on c.event_id = e.event_id
                where c.category_id = @category_id;";

            // Query to check whether another category of the event has the same name and distance
            string duplicate_query = @" select count(*) from category
                where event_id = @event_id and category_id <> @category_id and category_name = @category_name
                and ((race_distance = @race_distance) or (race_distance is null and @race_distance is null));";

            // Query
            string query = @" update category set category_name = @category_name, entry_fee = @entry_fee,
                race_distance = @race_distance, max_participants = @max_participants
                where category_id = @category_id;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the category exists and who owns it
                    int owner_id = 0;
                    int event_id = 0;
                    bool found = false;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@category_id", category_id);

                        using (SqlDataReader reader = check_owner.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                found = true;
                                owner_id = Convert.ToInt32(reader["organizer_id"]);
                                event_id = Convert.ToInt32(reader["event_id"]);
                            }
                        }
                    }

                    if (!found)
                    {
                        message = "Category not found...";
                    }
                    else if (owner_id != organizer_id)
                    {
                        message = "Forbidden: you are not the owning organizer...";
                    }
                    else
                    {
                        // Check whether another category has the same name and distance
                        int duplicate_count = 0;

                        using (SqlCommand check_duplicate = new SqlCommand(duplicate_query, connection))
                        {
                            check_duplicate.Parameters.AddWithValue("@event_id", event_id);

                            check_duplicate.Parameters.AddWithValue("@category_id", category_id);

                            check_duplicate.Parameters.AddWithValue("@category_name", category_name);

                            check_duplicate.Parameters.AddWithValue("@race_distance", race_distance.HasValue
                                    ? (object)race_distance.Value : DBNull.Value);

                            duplicate_count = Convert.ToInt32(check_duplicate.ExecuteScalar());
                        }

                        if (duplicate_count > 0)
                        {
                            return "Conflict: this event already has a category with the same name and distance...";
                        }

                        using (SqlCommand run_query = new SqlCommand(query, connection))
                        {
                            // Add parameters
                            run_query.Parameters.AddWithValue("@category_id", category_id);

                            run_query.Parameters.AddWithValue("@category_name", category_name);

                            run_query.Parameters.AddWithValue("@entry_fee", entry_fee);

                            run_query.Parameters.AddWithValue("@race_distance", race_distance.HasValue
                                    ? (object)race_distance.Value : DBNull.Value);

                            run_query.Parameters.AddWithValue("@max_participants", max_participants);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Category updated successfully...";
                            }
                        }
                    }//end of usiong run query
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
        }//end of update category

        // Method to delete a category of an event owned by the organizer
        public string delete_category(int category_id, int organizer_id)
        {//start of delete category

            // Temporary message
            string message = "Category deletion failed...";

            // Query to find the owner of the event that the category belongs to
            string owner_query = @" select e.organizer_id from category c
                inner join event e on c.event_id = e.event_id
                where c.category_id = @category_id;";

            // Query
            string query = @" delete from category where category_id = @category_id;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    connection.Open();

                    // Check that the category exists and who owns it
                    object owner = null;

                    using (SqlCommand check_owner = new SqlCommand(owner_query, connection))
                    {
                        check_owner.Parameters.AddWithValue("@category_id", category_id);

                        owner = check_owner.ExecuteScalar();
                    }

                    if (owner == null)
                    {
                        message = "Category not found...";
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
                            run_query.Parameters.AddWithValue("@category_id", category_id);

                            // Execute the query
                            int affected_row = run_query.ExecuteNonQuery();

                            if (affected_row > 0)
                            {
                                message = "Category deleted successfully...";
                            }
                        }
                    }
                }
            }
            catch (SqlException error)
            {
                // 547 is a foreign key error (participants are enrolled in the category)
                if (error.Number == 547)
                {
                    message = "Cannot delete: participants are already enrolled in this category...";
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
        }//end of delete category


    }//end of class
}//end of namespace
