using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;

namespace race_day_event.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class enrolments : ControllerBase
    {

        // POST: api/events/300000/enrolments
        [HttpPost("~/api/events/{event_id}/enrolments")]
        public string enrol_in_event(int event_id, int category_id)
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
            if (role != "Participant")
            {
                Response.StatusCode = 403;
                return "Forbidden: only participants can enrol in events...";
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.add_enrolment(event_id, user_id.Value, category_id);

            // Set the status code
            if (message == "Enrolment created successfully...")
            {
                Response.StatusCode = 201;
            }
            else if (message == "Event not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Invalid category"))
            {
                Response.StatusCode = 400;
            }
            else if (message.StartsWith("Conflict"))
            {
                Response.StatusCode = 409;
            }

            return message;
        }

        // GET: api/enrolments/me
        [HttpGet("me")]
        public string get_my_enrolments()
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
            if (role != "Participant")
            {
                Response.StatusCode = 403;
                return "Forbidden: only participants have personal enrolments...";
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.get_my_enrolments(user_id.Value);

            return message;
        }

        // GET: api/events/300000/enrolments
        [HttpGet("~/api/events/{event_id}/enrolments")]
        public string get_event_enrolments(int event_id)
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
                return "Forbidden: only organizers can view the enrolments of an event...";
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.get_event_enrolments(event_id, user_id.Value);

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

        // DELETE: api/enrolments/500000
        [HttpDelete("{enrolment_id}")]
        public IActionResult cancel_enrolment(int enrolment_id)
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
            if (role != "Participant")
            {
                return StatusCode(403, "Forbidden: only participants can cancel their enrolments...");
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.cancel_enrolment(enrolment_id, user_id.Value);

            // Set the response based on the message
            if (message == "Enrolment cancelled successfully...")
            {
                return NoContent();
            }
            else if (message == "Enrolment not found...")
            {
                return NotFound(message);
            }
            else if (message.StartsWith("Forbidden"))
            {
                return StatusCode(403, message);
            }
            else if (message.StartsWith("Conflict"))
            {
                return Conflict(message);
            }

            return BadRequest(message);
        }


    }
}
