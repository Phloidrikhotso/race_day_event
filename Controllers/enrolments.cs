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

        // Cleans the text of the entry form (the lists use "|" and new lines as separators, so those are removed)
        private string clean(string? text)
        {//start of clean
            return (text ?? "").Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim();
        }//end of clean

        // POST: api/events/300000/enrolments
        [HttpPost("~/api/events/{event_id}/enrolments")]
        public string enrol_in_event(int event_id, int category_id, string? emergency_contact_name = null,
            string? emergency_contact_relationship = null, string? emergency_contact_phone = null,
            string? address_street = null, string? address_city = null, string? address_postal_code = null,
            string? tshirt_size = null, bool indemnity_accepted = false, string? medical_notes = null)
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

            // Clean up the entry form details
            string contact_name = clean(emergency_contact_name);
            string contact_relationship = clean(emergency_contact_relationship);
            string contact_phone = clean(emergency_contact_phone);
            string street = clean(address_street);
            string city = clean(address_city);
            string postal_code = clean(address_postal_code);
            string size = clean(tshirt_size).ToUpper();
            string notes = clean(medical_notes);

            string[] sizes = { "XS", "S", "M", "L", "XL", "2XL", "3XL" };

            // Check the entry form details
            string? problem = null;

            if (contact_name == "" || contact_relationship == "" || contact_phone == "" || street == "" || city == "")
            {
                problem = "please fill in the emergency contact and the address.";
            }
            else if (contact_name.Length > 100 || contact_relationship.Length > 50 || contact_phone.Length > 30
                || street.Length > 200 || city.Length > 100 || notes.Length > 500)
            {
                problem = "one of the details is too long.";
            }
            else if (!contact_phone.All(c => char.IsDigit(c) || " +-()".Contains(c))
                || contact_phone.Count(char.IsDigit) < 7 || contact_phone.Count(char.IsDigit) > 15)
            {
                problem = "the emergency contact phone number is not valid.";
            }
            else if (postal_code.Length != 4 || !postal_code.All(char.IsDigit))
            {
                problem = "the postal code must be 4 digits.";
            }
            else if (!sizes.Contains(size))
            {
                problem = "please choose a T-shirt size.";
            }
            else if (!indemnity_accepted)
            {
                problem = "you must accept the indemnity to enter.";
            }

            if (problem != null)
            {
                Response.StatusCode = 400;
                return "Invalid details: " + problem;
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.add_enrolment(event_id, user_id.Value, category_id, contact_name,
                contact_relationship, contact_phone, notes, street, city, postal_code, size);

            // Set the status code
            if (message.StartsWith("Enrolment created successfully"))
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

        // GET: api/enrolments/me/race-numbers
        [HttpGet("me/race-numbers")]
        public string get_my_race_numbers()
        {//start of get my race numbers

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
                return "Forbidden: only participants have race numbers...";
            }

            // Create an instance of the enrolments class
            manage_enrolments enrolment_manager = new manage_enrolments();

            // Call the database method
            string message = enrolment_manager.get_my_race_numbers(user_id.Value);

            return message;
        }//end of get my race numbers

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
