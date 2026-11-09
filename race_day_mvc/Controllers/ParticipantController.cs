using Microsoft.AspNetCore.Mvc;
using race_day_mvc.Services;

namespace race_day_mvc.Controllers
{//start of namespace
    public class ParticipantController : Controller
    {//start of class
        private readonly api_client api;

        public ParticipantController(api_client api_service)
        {
            api = api_service;
        }

        // Checks the session and the role, returns a redirect when the user may not be here
        private IActionResult? check_participant()
        {//start of check participant

            if (HttpContext.Session.GetInt32("userID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (HttpContext.Session.GetString("role") != "Participant")
            {
                TempData["message_error"] = "That page is only for participants.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }//end of check participant

        // The API forgot the user (its session ended), so start again at the login page
        private IActionResult session_expired()
        {//start of session expired

            HttpContext.Session.Clear();
            TempData["message_error"] = "Your session has expired, please log in again.";
            return RedirectToAction("Login", "Account");
        }//end of session expired

        // Makes the date from the API easier to read
        private string show_date(string date_text)
        {//start of show date

            if (DateTime.TryParse(date_text, out DateTime date))
            {
                return date.ToString("ddd, d MMM yyyy HH:mm");
            }

            return date_text;
        }//end of show date

        // Turns 47 into 47th, 1 into 1st and so on
        private string get_ordinal(int number)
        {//start of get ordinal

            if (number % 100 >= 11 && number % 100 <= 13)
            {
                return number + "th";
            }

            switch (number % 10)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }//end of get ordinal

        // Splits the API text into lines
        private string[] get_lines(string text)
        {
            return text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // GET: /Participant/Details/300000
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {//start of details

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            // Get the event
            var event_result = await api.get("api/events/" + id);

            string[] parts = event_result.message.Split(" - ");

            if (event_result.status != 200 || parts.Length < 8)
            {
                TempData["message_error"] = event_result.status == 0 ? event_result.message : "That event could not be found.";
                return RedirectToAction("Index", "Home");
            }

            // id - name - description - date - location - distance - type - status
            string description = string.Join(" - ", parts.Skip(2).Take(parts.Length - 7));

            ViewBag.event_details = new string[] { parts[0], parts[1], description,
                show_date(parts[parts.Length - 5]), parts[parts.Length - 4], parts[parts.Length - 3],
                parts[parts.Length - 2], parts[parts.Length - 1] };

            // Get the categories of the event
            var category_result = await api.get("api/events/" + id + "/categories");

            List<string[]> categories = new List<string[]>();

            if (category_result.status == 200 && !category_result.message.StartsWith("No categories"))
            {
                // id - name - entry fee - race distance - max participants
                foreach (string line in get_lines(category_result.message))
                {
                    string[] category = line.Split(" - ");

                    if (category.Length == 5)
                    {
                        categories.Add(category);
                    }
                }
            }

            return View(categories);
        }//end of details

        // Removes the extra zeros from a distance such as 10.000
        private string tidy(string number)
        {//start of tidy

            if (!number.Contains('.') && !number.Contains(','))
            {
                return number;
            }

            return number.TrimEnd('0').TrimEnd('.', ',');
        }//end of tidy

        // Gets the event and the category the participant is entering (for the top of the entry form).
        // Returns false when the participant cannot enter (not found, or the event is not open)
        private async Task<bool> load_entry_summary(int event_id, int category_id)
        {//start of load entry summary

            var event_result = await api.get("api/events/" + event_id);

            // id - name - description - date - location - distance - type - status
            string[] parts = event_result.message.Split(" - ");

            if (event_result.status != 200 || parts.Length < 8 || parts[parts.Length - 1] != "Open")
            {
                return false;
            }

            var category_result = await api.get("api/events/" + event_id + "/categories");

            if (category_result.status != 200 || category_result.message.StartsWith("No categories"))
            {
                return false;
            }

            foreach (string line in get_lines(category_result.message))
            {
                // id - name - entry fee - race distance - max participants
                string[] category = line.Split(" - ");

                if (category.Length == 5 && category[0] == category_id.ToString())
                {
                    string distance_text = category[3].Trim();

                    ViewBag.event_id = event_id.ToString();
                    ViewBag.category_id = category_id.ToString();
                    ViewBag.event_name = parts[1];
                    ViewBag.event_date = show_date(parts[parts.Length - 5]);
                    ViewBag.category_text = category[1] + (distance_text == "" ? "" : " " + tidy(distance_text) + " km");
                    ViewBag.entry_fee = category[2];

                    return true;
                }
            }

            return false;
        }//end of load entry summary

        // GET: /Participant/EntryForm?event_id=300000&category_id=400000
        [HttpGet]
        public async Task<IActionResult> EntryForm(int event_id, int category_id)
        {//start of entry form page

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            if (!await load_entry_summary(event_id, category_id))
            {
                TempData["message_error"] = "You cannot enter that category right now.";
                return RedirectToAction("Index", "Home");
            }

            // 0 contact name, 1 relationship, 2 phone, 3 medical notes, 4 street, 5 city, 6 postal code, 7 t-shirt size
            ViewBag.entry_values = new string[] { "", "", "", "", "", "", "", "" };

            return View();
        }//end of entry form page

        // POST: /Participant/Enrol (sent by the entry form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int event_id, int category_id, string emergency_contact_name,
            string emergency_contact_relationship, string emergency_contact_phone, string medical_notes,
            string address_street, string address_city, string address_postal_code, string tshirt_size,
            bool indemnity_accepted)
        {//start of enrol

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            // Keep what the user typed in case something goes wrong
            string[] form = new string[] { emergency_contact_name ?? "", emergency_contact_relationship ?? "",
                emergency_contact_phone ?? "", medical_notes ?? "", address_street ?? "", address_city ?? "",
                address_postal_code ?? "", tshirt_size ?? "" };

            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("category_id", category_id.ToString());
            values.Add("emergency_contact_name", form[0]);
            values.Add("emergency_contact_relationship", form[1]);
            values.Add("emergency_contact_phone", form[2]);
            values.Add("medical_notes", form[3]);
            values.Add("address_street", form[4]);
            values.Add("address_city", form[5]);
            values.Add("address_postal_code", form[6]);
            values.Add("tshirt_size", form[7]);
            values.Add("indemnity_accepted", indemnity_accepted ? "true" : "false");

            var result = await api.post("api/events/" + event_id + "/enrolments", values);

            if (result.status == 401)
            {
                return session_expired();
            }

            // Enrolled, the API sends the race number back
            if (result.status == 201)
            {
                string race_number = result.message.Contains("Race number:")
                    ? result.message.Substring(result.message.IndexOf("Race number:") + 12).Trim()
                    : "";

                TempData["message_success"] = race_number == ""
                    ? "You are in! Your enrolment has been recorded."
                    : "You are in! Your race number is " + race_number + ".";

                return RedirectToAction("MyEnrolments");
            }

            // Something is wrong with the form, so show it again with what was typed
            if (result.status == 400 && result.message.StartsWith("Invalid details"))
            {
                if (!await load_entry_summary(event_id, category_id))
                {
                    TempData["message_error"] = "You cannot enter that category right now.";
                    return RedirectToAction("Details", new { id = event_id });
                }

                string reason = result.message.Replace("Invalid details:", "").Trim();

                ViewBag.message = reason.Length > 0 ? char.ToUpper(reason[0]) + reason.Substring(1) : "Please check your details.";
                ViewBag.entry_values = form;

                return View("EntryForm");
            }

            // The API explains why (already enrolled, event closed or category full)
            if (result.status == 409)
            {
                TempData["message_error"] = "Sorry, " + result.message.Replace("Conflict: ", "");
            }
            else if (result.status == 0)
            {
                TempData["message_error"] = result.message;
            }
            else
            {
                TempData["message_error"] = "We could not enter you for this event, please try again.";
            }

            return RedirectToAction("Details", new { id = event_id });
        }//end of enrol

        // GET: /Participant/MyEnrolments
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {//start of my enrolments

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            var result = await api.get("api/enrolments/me");

            if (result.status == 401)
            {
                return session_expired();
            }

            // The race numbers: enrolment id - race number
            Dictionary<string, string> race_numbers = new Dictionary<string, string>();

            var number_result = await api.get("api/enrolments/me/race-numbers");

            if (number_result.status == 200 && !number_result.message.StartsWith("No race numbers"))
            {
                foreach (string line in get_lines(number_result.message))
                {
                    string[] number_parts = line.Split(" - ");

                    if (number_parts.Length == 2)
                    {
                        race_numbers[number_parts[0]] = number_parts[1];
                    }
                }
            }

            // 0 id, 1 event name, 2 event date, 3 category, 4 entered on, 5 status, 6 race number
            List<string[]> rows = new List<string[]>();

            if (result.status == 0)
            {
                ViewBag.message = result.message;
            }
            else if (result.status == 200 && !result.message.StartsWith("No enrolments"))
            {
                // id - event name - event date - category - enrolled on - status
                foreach (string line in get_lines(result.message))
                {
                    string[] parts = line.Split(" - ");

                    if (parts.Length >= 6)
                    {
                        // Any extra " - " pieces belong to the event name
                        string event_name = string.Join(" - ", parts.Skip(1).Take(parts.Length - 5));

                        race_numbers.TryGetValue(parts[0], out string? race_number);

                        rows.Add(new string[] { parts[0], event_name, show_date(parts[parts.Length - 4]),
                            parts[parts.Length - 3], show_date(parts[parts.Length - 2]), parts[parts.Length - 1],
                            race_number ?? "" });
                    }
                }
            }

            return View(rows);
        }//end of my enrolments

        // POST: /Participant/Cancel/500000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {//start of cancel

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            var result = await api.delete("api/enrolments/" + id);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Enrolment cancelled"))
            {
                TempData["message_success"] = "Your enrolment has been cancelled.";
            }
            else if (result.status == 409)
            {
                TempData["message_error"] = "That enrolment is already cancelled.";
            }
            else if (result.status == 0)
            {
                TempData["message_error"] = result.message;
            }
            else
            {
                TempData["message_error"] = "We could not cancel that enrolment, please try again.";
            }

            return RedirectToAction("MyEnrolments");
        }//end of cancel

        // GET: /Participant/MyResults
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {//start of my results

            IActionResult? blocked = check_participant();

            if (blocked != null)
            {
                return blocked;
            }

            var result = await api.get("api/results/me");

            if (result.status == 401)
            {
                return session_expired();
            }

            List<string[]> rows = new List<string[]>();

            if (result.status == 0)
            {
                ViewBag.message = result.message;
            }
            else if (result.status == 200 && !result.message.StartsWith("No results"))
            {
                // id - event name - event date - category - finish time - overall position - category position - status - total finishers
                foreach (string line in get_lines(result.message))
                {
                    string[] parts = line.Split(" - ");

                    if (parts.Length >= 9)
                    {
                        int last = parts.Length - 1;

                        // Any extra " - " pieces belong to the event name
                        string event_name = string.Join(" - ", parts.Skip(1).Take(parts.Length - 8));

                        string status = parts[last - 1];

                        // Position text such as "47th out of 312"
                        string position_text = "-";
                        string category_text = "-";

                        if (status == "Finished")
                        {
                            if (int.TryParse(parts[last - 4], out int overall_position))
                            {
                                position_text = get_ordinal(overall_position);

                                if (int.TryParse(parts[last], out int total_finishers))
                                {
                                    position_text += " out of " + total_finishers;
                                }
                            }

                            if (int.TryParse(parts[last - 2], out int category_position))
                            {
                                category_text = get_ordinal(category_position);
                            }
                        }

                        string finish_time = string.IsNullOrWhiteSpace(parts[last - 5]) ? "-" : parts[last - 5];

                        rows.Add(new string[] { event_name, show_date(parts[last - 7]), parts[last - 6],
                            finish_time, position_text, status, category_text });
                    }
                }
            }

            return View(rows);
        }//end of my results
    }//end of class
}//end of namespace
