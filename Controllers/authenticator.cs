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
        {//start of register user method

            //creating an instance register_user with object name register
            register_user register = new register_user();

            string message = register.add_user(role_name, first_name, last_name, username, email, phone_number, password);

            return message;

        }//end of register user method

        // POST: api/authenticator/login
        [HttpPost("login")]
        public string login_user(string username, string password)
        {//star of login user method

            // Create an instance of the login class
            login_logout login = new login_logout();

            // Call the database method
            string message = login.login_user(username, password);

            return message;
        }//end of login user method

        // POST: api/authenticator/logout
        [HttpPost("logout")]
        public string logout_user()
        {//start of logout user method
            // Create an instance of the logout class
            login_logout logout = new login_logout();

            // Call the database method
            string message = logout.logout_user();

            return message;
        }//end of method logout user

    }//end of class
}//end of namespace
