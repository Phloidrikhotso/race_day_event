using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class results : ControllerBase
    {//start of class

        // POST: api/enrolments/500000/result
        [HttpPost("~/api/enrolments/{enrolment_id}/result")]
        public string capture_result(int enrolment_id, TimeSpan? finish_time = null,
            int? overall_position = null, int? category_position = null, string results_status = "Finished")
        {//start of capture results method

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
                return "Forbidden: only organizers can capture results...";
            }

            // Validate the fields
            if (!is_result_valid(finish_time, overall_position, category_position, results_status))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the finish time (00:00:00 to 23:59:59), positions (above 0) and status (Finished, DNF, DSQ, DNS)...";
            }

            // Create an instance of the results class
            manage_results result_manager = new manage_results();

            // Call the database method
            string message = result_manager.add_result(enrolment_id, user_id.Value, finish_time,
                overall_position, category_position, results_status);

            // Set the status code
            if (message == "Result captured successfully...")
            {
                Response.StatusCode = 201;
            }
            else if (message == "Enrolment not found...")
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
        }//end of capture results methods

        // PUT: api/enrolments/500000/result
        [HttpPut("~/api/enrolments/{enrolment_id}/result")]
        public string update_result(int enrolment_id, TimeSpan? finish_time = null,
            int? overall_position = null, int? category_position = null, string results_status = "Finished")
        {//start of update results method

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
                return "Forbidden: only organizers can update results...";
            }

            // Validate the fields
            if (!is_result_valid(finish_time, overall_position, category_position, results_status))
            {
                Response.StatusCode = 400;
                return "Validation failed: check the finish time (00:00:00 to 23:59:59), positions (above 0) and status (Finished, DNF, DSQ, DNS)...";
            }

            // Create an instance of the results class
            manage_results result_manager = new manage_results();

            // Call the database method
            string message = result_manager.update_result(enrolment_id, user_id.Value, finish_time,
                overall_position, category_position, results_status);

            // Set the status code
            if (message == "Enrolment not found..." || message == "Result not found...")
            {
                Response.StatusCode = 404;
            }
            else if (message.StartsWith("Forbidden"))
            {
                Response.StatusCode = 403;
            }

            return message;
        }//end of update results method

        // GET: api/results/me
        [HttpGet("me")]
        public string get_my_results()
        {//start of get my results method

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
                return "Forbidden: only participants have a personal race history...";
            }

            // Create an instance of the results class
            manage_results result_manager = new manage_results();

            // Call the database method
            string message = result_manager.get_my_results(user_id.Value);

            return message;
        }//end of get my results method

        // GET: api/events/300000/results
        [HttpGet("~/api/events/{event_id}/results")]
        public string get_event_results(int event_id)
        {//start of get event results

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
                return "Forbidden: only organizers can view the results of an event...";
            }

            // Create an instance of the results class
            manage_results result_manager = new manage_results();

            // Call the database method
            string message = result_manager.get_event_results(event_id, user_id.Value);

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
        }//end of get event results method

        // Method to validate the result fields
        private bool is_result_valid(TimeSpan? finish_time, int? overall_position,
            int? category_position, string results_status)
        {//start of is results valid method
            string[] valid_statuses = { "Finished", "DNF", "DSQ", "DNS" };

            if (finish_time.HasValue && (finish_time.Value < TimeSpan.Zero || finish_time.Value >= TimeSpan.FromHours(24)))
            {//start
                return false;
            }//end 

            if (overall_position.HasValue && overall_position.Value <= 0)
            {//start
                return false;
            }//end 

            if (category_position.HasValue && category_position.Value <= 0)
            {//start
                return false;
            }//end

            if (!valid_statuses.Contains(results_status))
            {
                return false;
            }

            return true;
        }//end of is valid results method


    }//end of class
}//end of namespace
