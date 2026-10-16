using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class users : ControllerBase
    {//start of class

        // GET: api/users/me
        [HttpGet("me")]
        public string get_my_profile()
        {//start of get my profile method

            // Get the logged in user id from the session
            int? userID = HttpContext.Session.GetInt32("userID");

            // Check whether a session is active
            if (userID == null)
            {
                Response.StatusCode = 401;
                return "No active session...";
            }

            // Create an instance of the profile class
            get_profile profile = new get_profile();

            // Call the database method
            string message = profile.get_user_profile(userID.Value);

            return message;
        }//end of get user profile method 

        // PUT: api/users/me
        [HttpPut("me")]
        public string update_my_profile(string first_name, string last_name, string username, string email, string phone_number)
        {//start of update my profile 

            // Get the logged in user id from the session
            int? userID = HttpContext.Session.GetInt32("userID");

            // Check whether a session is active
            if (userID == null)
            {
                Response.StatusCode = 401;
                return "No active session...";
            }

            // Validate the required fields
            if (string.IsNullOrWhiteSpace(first_name) || string.IsNullOrWhiteSpace(last_name)
                || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                Response.StatusCode = 400;
                return "Validation failed: first name, last name, username and email are required...";
            }

            // Create an instance of the update class
            update_profile update = new update_profile();

            // Call the database method
            string message = update.update_user(userID.Value, first_name, last_name, username, email, phone_number);

            return message;
        }//end of update user profile


    }//end of class
}//end of namespace
