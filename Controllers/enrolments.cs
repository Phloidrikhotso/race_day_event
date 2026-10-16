using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Controllers;
using race_day_event.Database;
using System.Text.Json.Serialization;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class enrolments : ControllerBase
    {//start of class

        // POST: api/events/300000/enrolments
        [HttpPost("~/api/events/{event_id}/enrolments")]
        public string enrol_in_event(int event_id, int category_id)
        {//start of enrol in event method

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
        }//end of enrol in event method

        // GET: api/enrolments/me
        [HttpGet("me")]
        public string get_my_enrolments()
        {//start ofget my enrolments

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
        }//end of get my enrolments

        // GET: api/events/300000/enrolments
        [HttpGet("~/api/events/{event_id}/enrolments")]
        public string get_event_enrolments(int event_id)
        {//start of get event enrolments

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
        }//end of get event enrolments

        // DELETE: api/enrolments/500000
        [HttpDelete("{enrolment_id}")]
        public string cancel_enrolment(int enrolment_id)
        {//start of cancel enrolments

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
                return "Forbidden: only participants can cancel their enrolments...";
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.cancel_enrolment(enrolment_id, user_id.Value);

            // Set the status code
            if (message == "Enrolment not found...")
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
            else if (message != "Enrolment cancelled successfully...")
            {
                Response.StatusCode = 400;
            }

            return message;
        }//end of cancel enrolments

    }//end of class
}//end of namespace
