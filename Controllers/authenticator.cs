using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using race_day_event.Database;

namespace race_day_event.Controllers
{//start of namespace
    [Route("api/[controller]")]
    [ApiController]
    public class authenticator : ControllerBase
    {//start of class


        [HttpPost("register")]
        public string register_user(string role_name, string first_name, string last_name,
            string username, string email, string phone_number, string password)
        {//start of method register user

            register_user register = new register_user();

            string message = register.add_user(role_name, first_name, last_name, username, email, phone_number, password);

            return message;

        }//end of method register user

        // POST: api/authenticator/login
        [HttpPost("login")]
        public string login_user(string username, string password)
        {
            // Create an instance of the login class
            login_logout login = new login_logout();

            // Variables to hold the user id and the role
            int user_id = 0;
            string role_name = "";

            // Call the database method
            string message = login.login_user(username, password, out user_id, out role_name);

            // Start the session if the login worked
            if (user_id != 0)
            {
                HttpContext.Session.SetInt32("userID", user_id);
                HttpContext.Session.SetString("role", role_name);
            }

            return message;
        }

        // POST: api/authenticator/logout
        [HttpPost("logout")]
        public string logout_user()
        {
            // Create an instance of the logout class
            login_logout logout = new login_logout();

            // Call the database method
            string message = logout.logout_user();

            // End the session
            HttpContext.Session.Clear();

            return message;
        }//end of method logout user

    }//end of class
}//end of namespace
