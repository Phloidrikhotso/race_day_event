using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;
using System.Text.Json.Serialization;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class events : ControllerBase
    {//start of class

        // The race types an organizer can choose (shows as a dropdown in Swagger)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum event_type_choice
        {
            Run,
            Walk,
            Cycle
        }

        // The statuses an event can have (shows as a dropdown in Swagger)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum event_status_choice
        {
            Planned,
            Open,
            Closed,
            Cancelled,
            Completed
        }

        // GET: api/events
        [HttpGet]
        public string get_events()
        {
            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.get_all_events();

            return message;
        }

        // GET: api/events/300000
        [HttpGet("{event_id}")]
        public string get_event(int event_id)
        {
            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.get_one_event(event_id);

            // Check whether the event exists
            if (message == "Event not found...")
            {
                Response.StatusCode = 404;
            }

            return message;
        }

        // POST: api/events
        [HttpPost]
        public string create_event(string event_name, string event_description, DateTime event_date,
            string event_location, decimal distance, event_type_choice event_type,
            event_status_choice event_status = event_status_choice.Planned)
        {
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
                return "Forbidden: only organizers can create events...";
            }

            // Validate the fields
            if (!is_event_valid(event_name, event_location, distance, event_type.ToString(), event_status.ToString()))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the name, location and distance (above 0)...";
            }

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.add_event(user_id.Value, event_name, event_description, event_date,
                event_location, distance, event_type.ToString(), event_status.ToString());

            // Check whether the event was created
            if (message == "Event created successfully...")
            {
                Response.StatusCode = 201;
            }

            return message;
        }

        // PUT: api/events/300000
        [HttpPut("{event_id}")]
        public string update_event(int event_id, string event_name, string event_description, DateTime event_date,
            string event_location, decimal distance, event_type_choice event_type,
            event_status_choice event_status = event_status_choice.Planned)
        {
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
                return "Forbidden: only organizers can update events...";
            }

            // Validate the fields
            if (!is_event_valid(event_name, event_location, distance, event_type.ToString(), event_status.ToString()))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the name, location and distance (above 0)...";
            }

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.update_event(event_id, user_id.Value, event_name, event_description,
                event_date, event_location, distance, event_type.ToString(), event_status.ToString());

            // Set the status code
            if (message == "Event not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Forbidden"))
            {
                Response.StatusCode = 403;
            }

            return message;
        }

        // DELETE: api/events/300000
        [HttpDelete("{event_id}")]
        public IActionResult delete_event(int event_id)
        {
            // Get the logged in user from the session
            int? user_id = HttpContext.Session.GetInt32("userID");
            string role = HttpContext.Session.GetString("role");

            // Check whether a session is active
            if (user_id == null)
            {
                return Unauthorized("No active session...");
            }

            // Check the role
            if (role != "Organizer")
            {
                return StatusCode(403, "Forbidden: only organizers can delete events...");
            }

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.delete_event(event_id, user_id.Value);

            // Set the response based on the message
            if (message == "Event deleted successfully...")
            {
                return NoContent();
            }
            else if (message == "Event not found...")
            {
                return NotFound(message);
            }
            else if (message.StartsWith("Forbidden"))
            {
                return StatusCode(403, message);
            }
            else if (message.StartsWith("Cannot delete"))
            {
                return Conflict(message);
            }

            return BadRequest(message);
        }

        // Method to validate the event fields
        private bool is_event_valid(string event_name, string event_location, decimal distance,
            string event_type, string event_status)
        {
            string[] valid_types = { "Run", "Walk", "Cycle" };
            string[] valid_statuses = { "Planned", "Open", "Closed", "Cancelled", "Completed" };

            if (string.IsNullOrWhiteSpace(event_name) || string.IsNullOrWhiteSpace(event_location))
            {
                return false;
            }

            if (distance <= 0)
            {
                return false;
            }

            if (!valid_types.Contains(event_type) || !valid_statuses.Contains(event_status))
            {
                return false;
            }

            return true;
        }


    }
}
