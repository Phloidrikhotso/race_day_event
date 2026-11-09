using Microsoft.AspNetCore.Mvc;
using race_day_mvc.Services;

namespace race_day_mvc.Controllers
{//start of namespace 
    public class AccountController : Controller
    {//start of class

        private readonly api_client api;
        private readonly IWebHostEnvironment web_environment;

        // The avatars a user can pick (the API accepts these names)
        private readonly string[] avatar_names = { "Runner", "Cyclist", "Walker", "Medal", "Trophy", "Sneaker" };

        // The picture types that may be uploaded
        private readonly string[] picture_types = { ".jpg", ".jpeg", ".png", ".webp" };

        public AccountController(api_client api_service, IWebHostEnvironment environment)
        {
            api = api_service;
            web_environment = environment;
        }

        // The folder where the uploaded profile pictures are kept
        private string get_upload_folder()
        {//start of get upload folder

            string folder = Path.Combine(web_environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(folder);

            return folder;
        }//end of get upload folder

        // The picture to show: the uploaded photo when there is one, otherwise the chosen avatar
        private string get_avatar_url(int user_id, string picture)
        {//start of get avatar url

            string folder = get_upload_folder();

            foreach (string type in picture_types)
            {
                string file_path = Path.Combine(folder, user_id + type);

                if (System.IO.File.Exists(file_path))
                {
                    return "/uploads/avatars/" + user_id + type + "?v=" + System.IO.File.GetLastWriteTimeUtc(file_path).Ticks;
                }
            }

            return "https://api.dicebear.com/10.x/miniavs/svg?seed=" + Uri.EscapeDataString(picture);
        }//end of get avatar url

        // Removes the uploaded photo of a user (used when they pick an avatar instead)
        private void delete_uploaded_pictures(int user_id)
        {//start of delete uploaded pictures

            string folder = get_upload_folder();

            foreach (string type in picture_types)
            {
                string file_path = Path.Combine(folder, user_id + type);

                if (System.IO.File.Exists(file_path))
                {
                    System.IO.File.Delete(file_path);
                }
            }
        }//end of delete uploaded pictures

        // The picture of the logged in user (from the session)
        private string current_avatar(int user_id)
        {//start of current avatar

            string? saved = HttpContext.Session.GetString("avatar_url");

            if (!string.IsNullOrEmpty(saved))
            {
                return saved;
            }

            return get_avatar_url(user_id, HttpContext.Session.GetString("picture") ?? "Runner");
        }//end of current avatar

        // Each role starts on its own site
        private IActionResult go_home()
        {//start of go home

            if (HttpContext.Session.GetString("role") == "Organizer")
            {
                return RedirectToAction("Dashboard", "Organizer");
            }

            return RedirectToAction("Index", "Home");
        }//end of go home

        // The API forgot the user (its session ended), so start again at the login page
        private IActionResult session_expired()
        {//start of session expired

            HttpContext.Session.Clear();
            TempData["message_error"] = "Your session has expired, please log in again.";
            return RedirectToAction("Login");
        }//end of session expired

        // Logs the user in through the API and fills the session. Returns an error text, or null when it worked
        private async Task<string?> start_session(string email, string password)
        {//start of start session

            // Call the API to log in
            Dictionary<string, string> login_values = new Dictionary<string, string>();
            login_values.Add("email", email);
            login_values.Add("password", password);

            var login = await api.post("api/authenticator/login", login_values);

            // The API could not be reached
            if (login.status == 0)
            {
                return login.message;
            }

            // Wrong email or password
            if (!login.message.StartsWith("Login successful"))
            {
                if (login.message.StartsWith("Invalid"))
                {
                    return "Invalid email or password.";
                }

                return "Something went wrong, please try again.";
            }

            // Get the user's details from the API (name, sticker, role and id)
            var profile = await api.get("api/users/me");

            string[] parts = profile.message.Split(" - ");

            int user_id = 0;

            if (profile.status != 200 || parts.Length < 8 || !int.TryParse(parts[7], out user_id))
            {
                return "Something went wrong, please try again.";
            }

            string picture = string.IsNullOrWhiteSpace(parts[5]) ? "Runner" : parts[5];

            // Keep the user in the session
            HttpContext.Session.SetInt32("userID", user_id);
            HttpContext.Session.SetString("role", parts[6]);
            HttpContext.Session.SetString("first_name", parts[0]);
            HttpContext.Session.SetString("picture", picture);
            HttpContext.Session.SetString("avatar_url", get_avatar_url(user_id, picture));

            return null;
        }//end of start session

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {//start of login page

            // Send logged in users to their own site
            if (HttpContext.Session.GetInt32("userID") != null)
            {
                return go_home();
            }

            return View();
        }//end of login page

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {//start of login

            // Check the fields
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.message = "Please enter your email and password.";
                return View();
            }

            string? error = await start_session(email, password);

            if (error != null)
            {
                ViewBag.message = error;
                return View();
            }

            return go_home();
        }//end of login

        // GET: /Account/Register (step 1)
        [HttpGet]
        public IActionResult Register()
        {//start of register page

            // Send logged in users to their own site
            if (HttpContext.Session.GetInt32("userID") != null)
            {
                return go_home();
            }

            return View();
        }//end of register page

        // POST: /Account/Register (step 1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string role_name, string first_name, string last_name,
            string username, string email, string phone_number, string password, string confirm_password)
        {//start of register

            // Check the required fields
            if (string.IsNullOrWhiteSpace(first_name) || string.IsNullOrWhiteSpace(last_name)
                || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.message = "Please fill in all the required fields.";
                return View();
            }

            // Check the passwords
            if (password != confirm_password)
            {
                ViewBag.message = "The passwords do not match.";
                return View();
            }

            // Call the API to register (the picture is chosen in step 2)
            Dictionary<string, string> register_values = new Dictionary<string, string>();
            register_values.Add("role_name", role_name ?? "Participant");
            register_values.Add("first_name", first_name);
            register_values.Add("last_name", last_name);
            register_values.Add("username", username);
            register_values.Add("email", email);
            register_values.Add("phone_number", phone_number ?? "");
            register_values.Add("password", password);
            register_values.Add("profile_picture", "Runner");

            var register = await api.post("api/authenticator/register", register_values);

            // The API could not be reached
            if (register.status == 0)
            {
                ViewBag.message = register.message;
                return View();
            }

            // Registered
            if (register.message.StartsWith("User registered"))
            {
                // Log the new user in so they can choose their picture (step 2)
                string? error = await start_session(email, password);

                if (error != null)
                {
                    TempData["success"] = "Account created, you can log in now.";
                    return RedirectToAction("Login");
                }

                return RedirectToAction("ChoosePicture", new { first_time = true });
            }

            // Username or email already used
            if (register.message.StartsWith("Username or email"))
            {
                ViewBag.message = "That username or email is already registered. Try logging in instead.";
                return View();
            }

            // Anything else (no raw error is shown to the user)
            ViewBag.message = "We could not create your account, please check your details and try again.";
            return View();
        }//end of register

        // GET: /Account/ChoosePicture (step 2 of registering, also used to change the picture later)
        [HttpGet]
        public IActionResult ChoosePicture(bool first_time = false)
        {//start of choose picture page

            int? user_id = HttpContext.Session.GetInt32("userID");

            if (user_id == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.avatar_url = current_avatar(user_id.Value);
            ViewBag.first_time = first_time;

            return View();
        }//end of choose picture page

        // POST: /Account/ChoosePicture (pick one of the avatars)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChoosePicture(string profile_picture, bool first_time = false)
        {//start of choose picture

            int? user_id = HttpContext.Session.GetInt32("userID");

            if (user_id == null)
            {
                return RedirectToAction("Login");
            }

            if (!avatar_names.Contains(profile_picture ?? ""))
            {
                TempData["message_error"] = "Please pick one of the avatars.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            // The API saves all the profile details together, so get the current ones first
            var profile = await api.get("api/users/me");

            if (profile.status == 401)
            {
                return session_expired();
            }

            string[] parts = profile.message.Split(" - ");

            if (profile.status != 200 || parts.Length != 8)
            {
                TempData["message_error"] = profile.status == 0 ? profile.message : "We could not save your picture, please try again.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("first_name", parts[0]);
            values.Add("last_name", parts[1]);
            values.Add("username", parts[2]);
            values.Add("email", parts[3]);
            values.Add("phone_number", parts[4]);
            values.Add("profile_picture", profile_picture!);

            var result = await api.put("api/users/me", values);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (!result.message.StartsWith("Profile updated"))
            {
                TempData["message_error"] = result.status == 0 ? result.message : "We could not save your picture, please try again.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            // An avatar replaces an uploaded photo
            delete_uploaded_pictures(user_id.Value);

            HttpContext.Session.SetString("picture", profile_picture!);
            HttpContext.Session.SetString("avatar_url", get_avatar_url(user_id.Value, profile_picture!));

            TempData["message_success"] = "Your picture has been saved.";

            if (first_time)
            {
                return go_home();
            }

            return RedirectToAction("Profile");
        }//end of choose picture

        // POST: /Account/UploadPicture (upload your own photo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile? picture_file, bool first_time = false)
        {//start of upload picture

            int? user_id = HttpContext.Session.GetInt32("userID");

            if (user_id == null)
            {
                return RedirectToAction("Login");
            }

            if (picture_file == null || picture_file.Length == 0)
            {
                TempData["message_error"] = "Please choose a picture to upload.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            if (picture_file.Length > 2 * 1024 * 1024)
            {
                TempData["message_error"] = "That picture is too big, the limit is 2 MB.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            string type = Path.GetExtension(picture_file.FileName).ToLower();

            if (!picture_types.Contains(type))
            {
                TempData["message_error"] = "Please upload a JPG, PNG or WebP picture.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            // Check that the first bytes of the file really belong to a picture
            byte[] header = new byte[12];

            using (Stream stream = picture_file.OpenReadStream())
            {
                await stream.ReadAsync(header, 0, header.Length);
            }

            bool is_jpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            bool is_png = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            bool is_webp = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            bool matches;

            if (type == ".png")
            {
                matches = is_png;
            }
            else if (type == ".webp")
            {
                matches = is_webp;
            }
            else
            {
                matches = is_jpeg;
            }

            if (!matches)
            {
                TempData["message_error"] = "That file is not a valid picture.";
                return RedirectToAction("ChoosePicture", new { first_time = first_time });
            }

            // The file is named after the user id, so the user's own name for the file is never used
            delete_uploaded_pictures(user_id.Value);

            string file_path = Path.Combine(get_upload_folder(), user_id.Value + type);

            using (FileStream file_stream = new FileStream(file_path, FileMode.Create))
            {
                await picture_file.CopyToAsync(file_stream);
            }

            string picture = HttpContext.Session.GetString("picture") ?? "Runner";
            HttpContext.Session.SetString("avatar_url", get_avatar_url(user_id.Value, picture));

            TempData["message_success"] = "Your picture has been uploaded.";

            if (first_time)
            {
                return go_home();
            }

            return RedirectToAction("Profile");
        }//end of upload picture

        // GET: /Account/Profile (both roles)
        [HttpGet]
        public async Task<IActionResult> Profile()
        {//start of profile page

            int? user_id = HttpContext.Session.GetInt32("userID");

            if (user_id == null)
            {
                return RedirectToAction("Login");
            }

            var result = await api.get("api/users/me");

            if (result.status == 401)
            {
                return session_expired();
            }

            // first name - last name - username - email - phone - sticker - role - id
            string[] parts = result.message.Split(" - ");

            if (result.status != 200 || parts.Length != 8)
            {
                TempData["message_error"] = result.status == 0 ? result.message : "We could not load your profile, please try again.";
                return go_home();
            }

            ViewBag.profile = new string[] { parts[0], parts[1], parts[2], parts[3], parts[4],
                string.IsNullOrWhiteSpace(parts[5]) ? "Runner" : parts[5] };
            ViewBag.avatar_url = current_avatar(user_id.Value);

            return View();
        }//end of profile page

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string first_name, string last_name, string username,
            string email, string phone_number)
        {//start of update profile

            int? user_id = HttpContext.Session.GetInt32("userID");

            if (user_id == null)
            {
                return RedirectToAction("Login");
            }

            // Keep what the user typed in case something goes wrong
            ViewBag.profile = new string[] { first_name ?? "", last_name ?? "", username ?? "", email ?? "",
                phone_number ?? "", HttpContext.Session.GetString("picture") ?? "Runner" };
            ViewBag.avatar_url = current_avatar(user_id.Value);

            // Check the required fields
            if (string.IsNullOrWhiteSpace(first_name) || string.IsNullOrWhiteSpace(last_name)
                || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
            {
                ViewBag.message = "First name, last name, username and email are required.";
                return View();
            }

            // The picture is not sent, so the API keeps the one the user already has
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("first_name", first_name);
            values.Add("last_name", last_name);
            values.Add("username", username);
            values.Add("email", email);
            values.Add("phone_number", phone_number ?? "");

            var result = await api.put("api/users/me", values);

            if (result.status == 401)
            {
                return session_expired();
            }

            // Updated, so refresh the name shown in the menu
            if (result.message.StartsWith("Profile updated"))
            {
                HttpContext.Session.SetString("first_name", first_name);

                TempData["message_success"] = "Your profile has been updated.";
                return RedirectToAction("Profile");
            }

            if (result.message.StartsWith("Username or email"))
            {
                ViewBag.message = "That username or email is already used by another account.";
            }
            else if (result.status == 0)
            {
                ViewBag.message = result.message;
            }
            else
            {
                ViewBag.message = "We could not update your profile, please check your details and try again.";
            }

            return View();
        }//end of update profile

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {//start of logout

            // Tell the API to end its session
            await api.post("api/authenticator/logout");

            // End the MVC session
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }//end of logout


    }//end of class
}//end of namespace
