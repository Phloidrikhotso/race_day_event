using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;
using System.Text.Json.Serialization;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class categories : ControllerBase
    {//start of class

        // The category names an organizer can choose (shows as a dropdown in Swagger)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum category_name_choice
        {//start of category name choice
            Run,
            Walk,
            Cycle
        }//end of categoty name choice

        // GET: api/events/300000/categories
        [HttpGet("~/api/events/{event_id}/categories")]
        public string get_categories(int event_id)
        {//start of get categories metheod

            // Create an instance of the categories class
            manage_categories category_manager = new manage_categories();

            // Call the database method
            string message = category_manager.get_event_categories(event_id);

            // Check whether the event exists
            if (message == "Event not found...")
            {
                Response.StatusCode = 404;
            }

            return message;
        }//end of get categories method

        // POST: api/events/300000/categories
        [HttpPost("~/api/events/{event_id}/categories")]
        public string create_category(int event_id, category_name_choice category_name, decimal entry_fee = 0,
            decimal? race_distance = null, int max_participants = 500)
        {//start of create category method 

            // Get the logged in user from the session
            int? user_id = HttpContext.Session.GetInt32("userID");
            string role = HttpContext.Session.GetString("role");

            // Check whether a session is active
            if (user_id == null)
            {
                Response.StatusCode = 401;
                return "No active session...";
            }

            // Check the role
            if (role != "Organizer")
            {
                Response.StatusCode = 403;
                return "Forbidden: only organizers can create categories...";
            }

            // Validate the fields
            if (!is_category_valid(category_name.ToString(), entry_fee, race_distance, max_participants))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the entry fee (0 or more), race distance (blank or above 0) and max participants (above 0)...";
            }

            // Create an instance of the categories class
            manage_categories category_manager = new manage_categories();

            // Call the database method
            string message = category_manager.add_category(event_id, user_id.Value, category_name.ToString(),
                entry_fee, race_distance, max_participants);

            // Set the status code
            if (message == "Category created successfully...")
            {
                Response.StatusCode = 201;
            }
            else if (message == "Event not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Forbidden"))
            {
                Response.StatusCode = 403;
            }
            else if (message.StartsWith("Conflict"))
            {
                Response.StatusCode = 409;
            }

            return message;
        }//end of create category

        // PUT: api/categories/400000
        [HttpPut("{category_id}")]
        public string update_category(int category_id, category_name_choice category_name, decimal entry_fee = 0,
            decimal? race_distance = null, int max_participants = 500)
        {//start of update category

            // Get the logged in user from the session
            int? user_id = HttpContext.Session.GetInt32("userID");
            string role = HttpContext.Session.GetString("role");

            // Check whether a session is active
            if (user_id == null)
            {
                Response.StatusCode = 401;
                return "No active session...";
            }

            // Check the role
            if (role != "Organizer")
            {
                Response.StatusCode = 403;
                return "Forbidden: only organizers can update categories...";
            }

            // Validate the fields
            if (!is_category_valid(category_name.ToString(), entry_fee, race_distance, max_participants))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the entry fee (0 or more), race distance (blank or above 0) and max participants (above 0)...";
            }

            // Create an instance of the categories class
            manage_categories category_manager = new manage_categories();

            // Call the database method
            string message = category_manager.update_category(category_id, user_id.Value, category_name.ToString(),
                entry_fee, race_distance, max_participants);

            // Set the status code
            if (message == "Category not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Forbidden"))
            {
                Response.StatusCode = 403;
            }
            else if (message.StartsWith("Conflict"))
            {
                Response.StatusCode = 409;
            }

            return message;
        }//end of update category

        // DELETE: api/categories/400000
        [HttpDelete("{category_id}")]
        public string delete_category(int category_id)
        {//start of delete category

            // Get the logged in user from the session
            int? user_id = HttpContext.Session.GetInt32("userID");
            string role = HttpContext.Session.GetString("role");

            // Check whether a session is active
            if (user_id == null)
            {
                Response.StatusCode = 401;
                return "No active session...";
            }

            // Check the role
            if (role != "Organizer")
            {
                Response.StatusCode = 403;
                return "Forbidden: only organizers can delete categories...";
            }

            // Create an instance of the categories class
            manage_categories category_manager = new manage_categories();

            // Call the database method
            string message = category_manager.delete_category(category_id, user_id.Value);

            // Set the status code
            if (message == "Category not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Forbidden"))
            {
                Response.StatusCode = 403;
            }
            else if (message.StartsWith("Cannot delete"))
            {
                Response.StatusCode = 409;
            }
            else if (message != "Category deleted successfully...")
            {
                Response.StatusCode = 400;
            }

            return message;
        }//end of delete category method

        // Method to validate the category fields
        private bool is_category_valid(string category_name, decimal entry_fee,
            decimal? race_distance, int max_participants)
        {//start of is category valid method

            if (string.IsNullOrWhiteSpace(category_name))
            {//start of if 
                return false;
            }//end of if

            if (entry_fee < 0)
            {//start of if
                return false;
            }//end of if 

            if (race_distance.HasValue && race_distance.Value <= 0)
            {
                return false;
            }

            if (max_participants <= 0)
            {
                return false;
            }

            return true;
        }//end of is category valid method
    }//end of class
}//end of namespace
