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
        {//start of event type choice
            Run,
            Walk,
            Cycle
        }//end of event type choice

        // The statuses an event can have (shows as a dropdown in Swagger)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum event_status_choice
        {//start of event status choice
            Planned,
            Open,
            Closed,
            Cancelled,
            Completed
        }//end of event status choice

        // GET: api/events
        [HttpGet]
        public string get_events()
        {//start of get events

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.get_all_events();

            return message;
        }//end of get events

        // GET: api/events/mine
        [HttpGet("mine")]
        public string get_my_events()
        {//start of get my events

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
                return "Forbidden: only organizers have events of their own...";
            }

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.get_my_events(user_id.Value);

            return message;
        }//end of get my events

        // GET: api/events/300000
        [HttpGet("{event_id}")]
        public string get_event(int event_id)
        {//start of get event

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
        }//end of get events

        // POST: api/events
        [HttpPost]
        public string create_event(string event_name, string event_description, DateTime event_date,
            string event_location, decimal distance, event_type_choice event_type,
            event_status_choice event_status = event_status_choice.Planned)
        {//start of create event

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
        }//end of create event

        // PUT: api/events/300000
        [HttpPut("{event_id}")]
        public string update_event(int event_id, string event_name, string event_description, DateTime event_date,
            string event_location, decimal distance, event_type_choice event_type,
            event_status_choice event_status = event_status_choice.Planned)
        {//start of update event

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
        }//end of update

        // DELETE: api/events/300000
        [HttpDelete("{event_id}")]
        public string delete_event(int event_id)
        {//start of delete event

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
                return "Forbidden: only organizers can delete events...";
            }

            // Create an instance of the events class
            manage_events event_manager = new manage_events();

            // Call the database method
            string message = event_manager.delete_event(event_id, user_id.Value);

            // Set the status code
            if (message == "Event not found...")
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
            else if (message != "Event deleted successfully...")
            {
                Response.StatusCode = 400;
            }

            return message;
        }//end of delete event

        // Method to validate the event fields
        private bool is_event_valid(string event_name, string event_location, decimal distance,
            string event_type, string event_status)
        {//start of is event valid

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
        }//end of is event valid


    }//end of class
}//end of namespace
