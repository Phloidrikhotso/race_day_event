using Microsoft.AspNetCore.Mvc;
using race_day_mvc.Services;
using System.Globalization;

namespace race_day_mvc.Controllers
{//start of namespace
    public class OrganizerController : Controller
    {//start of class

        private readonly api_client api;

        public OrganizerController(api_client api_service)
        {
            api = api_service;
        }

        // Checks the session and the role, returns a redirect when the user may not be here
        private IActionResult? check_organizer()
        {//start of check organizer

            if (HttpContext.Session.GetInt32("userID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (HttpContext.Session.GetString("role") != "Organizer")
            {
                TempData["message_error"] = "That page is only for organizers.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }//end of check organizer

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

        // Removes the extra zeros from a number such as 10.000
        private string tidy(string number)
        {//start of tidy

            if (!number.Contains('.') && !number.Contains(','))
            {
                return number;
            }

            return number.TrimEnd('0').TrimEnd('.', ',');
        }//end of tidy

        // Splits the API text into lines
        private string[] get_lines(string text)
        {
            return text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Turns one line from api/events/mine into an array:
        // 0 id, 1 name, 2 description, 3 date (readable), 4 location, 5 distance, 6 type, 7 status, 8 enrolments, 9 date (raw)
        private string[]? read_event(string line)
        {//start of read event

            // id - name - description - date - location - distance - type - status - enrolment count
            string[] parts = line.Split(" - ");

            if (parts.Length < 9)
            {
                return null;
            }

            int last = parts.Length - 1;

            // Any extra " - " pieces belong to the description
            string description = string.Join(" - ", parts.Skip(2).Take(parts.Length - 8));

            return new string[] { parts[0], parts[1], description, show_date(parts[last - 5]), parts[last - 4],
                tidy(parts[last - 3]), parts[last - 2], parts[last - 1], parts[last], parts[last - 5] };
        }//end of read event

        // Gets all the events of the logged in organizer
        private async Task<(int status, string message, List<string[]> events)> load_my_events()
        {//start of load my events

            var result = await api.get("api/events/mine");

            List<string[]> events = new List<string[]>();

            if (result.status == 200 && !result.message.StartsWith("No events"))
            {
                foreach (string line in get_lines(result.message))
                {
                    string[]? event_row = read_event(line);

                    if (event_row != null)
                    {
                        events.Add(event_row);
                    }
                }
            }

            return (result.status, result.message, events);
        }//end of load my events

        // Gets the categories of one event: 0 id, 1 name, 2 entry fee, 3 race distance (blank when none), 4 max participants
        private async Task<(int status, string message, List<string[]> categories)> load_event_categories(int event_id)
        {//start of load event categories

            var result = await api.get("api/events/" + event_id + "/categories");

            List<string[]> categories = new List<string[]>();

            if (result.status == 200 && !result.message.StartsWith("No categories"))
            {
                // id - name - entry fee - race distance - max participants
                foreach (string line in get_lines(result.message))
                {
                    string[] parts = line.Split(" - ");

                    if (parts.Length == 5)
                    {
                        parts[3] = tidy(parts[3]);
                        categories.Add(parts);
                    }
                }
            }

            return (result.status, result.message, categories);
        }//end of load event categories

        // Puts the event name in the ViewBag, returns false when the event is not one of the organizer's
        private async Task<bool> set_event_name(int event_id)
        {//start of set event name

            var mine = await load_my_events();

            string[]? event_row = mine.events.FirstOrDefault(e => e[0] == event_id.ToString());

            if (event_row == null)
            {
                return false;
            }

            ViewBag.event_name = event_row[1];
            return true;
        }//end of set event name

        // Checks the event form, returns an error text (or null) and the values for the API
        // form: 0 name, 1 description, 2 date, 3 location, 4 distance, 5 type, 6 status
        private string? check_event_form(string[] form, out Dictionary<string, string> values)
        {//start of check event form

            values = new Dictionary<string, string>();

            for (int i = 0; i < 5; i++)
            {
                if (string.IsNullOrWhiteSpace(form[i]))
                {
                    return "Please fill in all the fields.";
                }
            }

            if (!DateTime.TryParse(form[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime event_date))
            {
                return "Please enter a valid date and time.";
            }

            if (!decimal.TryParse(form[4].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal distance) || distance <= 0)
            {
                return "The distance must be a number above 0.";
            }

            if (!new string[] { "Run", "Walk", "Cycle" }.Contains(form[5]))
            {
                return "Please choose the race type.";
            }

            if (!new string[] { "Planned", "Open", "Closed", "Cancelled", "Completed" }.Contains(form[6]))
            {
                return "Please choose the event status.";
            }

            values.Add("event_name", form[0].Trim());
            values.Add("event_description", form[1].Trim());
            values.Add("event_date", event_date.ToString("yyyy-MM-ddTHH:mm:ss"));
            values.Add("event_location", form[3].Trim());
            values.Add("distance", distance.ToString(CultureInfo.InvariantCulture));
            values.Add("event_type", form[5]);
            values.Add("event_status", form[6]);

            return null;
        }//end of check event form

        // Checks the category form, returns an error text (or null) and the values for the API
        // form: 0 name, 1 entry fee, 2 race distance (optional), 3 max participants
        private string? check_category_form(string[] form, out Dictionary<string, string> values)
        {//start of check category form

            values = new Dictionary<string, string>();

            if (!new string[] { "Run", "Walk", "Cycle" }.Contains(form[0]))
            {
                return "Please choose the category type.";
            }

            if (!decimal.TryParse(form[1].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal entry_fee) || entry_fee < 0)
            {
                return "The entry fee must be 0 or more.";
            }

            decimal race_distance = 0;

            if (!string.IsNullOrWhiteSpace(form[2]))
            {
                if (!decimal.TryParse(form[2].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out race_distance) || race_distance <= 0)
                {
                    return "The distance must be a number above 0 (or leave it blank).";
                }
            }

            if (!int.TryParse(form[3], out int max_participants) || max_participants <= 0)
            {
                return "Max participants must be a whole number above 0.";
            }

            values.Add("category_name", form[0]);
            values.Add("entry_fee", entry_fee.ToString(CultureInfo.InvariantCulture));

            // The distance is only sent when the organizer filled it in
            if (!string.IsNullOrWhiteSpace(form[2]))
            {
                values.Add("race_distance", race_distance.ToString(CultureInfo.InvariantCulture));
            }

            values.Add("max_participants", max_participants.ToString());

            return null;
        }//end of check category form

        // GET: /Organizer/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {//start of dashboard

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            if (mine.status == 0)
            {
                ViewBag.message = mine.message;
            }

            // Numbers for the cards
            int total_enrolments = 0;

            foreach (string[] event_row in mine.events)
            {
                if (int.TryParse(event_row[8], out int count))
                {
                    total_enrolments += count;
                }
            }

            ViewBag.total_events = mine.events.Count;
            ViewBag.open_events = mine.events.Count(e => e[7] == "Open");
            ViewBag.planned_events = mine.events.Count(e => e[7] == "Planned");
            ViewBag.total_enrolments = total_enrolments;

            // The next 5 events that still have to happen
            List<string[]> upcoming = mine.events
                .Where(e => e[7] != "Cancelled" && e[7] != "Completed"
                    && DateTime.TryParse(e[9], out DateTime date) && date >= DateTime.Now)
                .OrderBy(e => DateTime.Parse(e[9]))
                .Take(5)
                .ToList();

            return View(upcoming);
        }//end of dashboard

        // GET: /Organizer/Events
        [HttpGet]
        public async Task<IActionResult> Events()
        {//start of events

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            if (mine.status == 0)
            {
                ViewBag.message = mine.message;
            }

            return View(mine.events);
        }//end of events

        // GET: /Organizer/CreateEvent
        [HttpGet]
        public IActionResult CreateEvent()
        {//start of create event page

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            ViewBag.mode = "Create";
            ViewBag.event_values = new string[] { "", "", "", "", "", "Run", "Planned" };

            return View("EventForm");
        }//end of create event page

        // POST: /Organizer/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(string event_name, string event_description, string event_date,
            string event_location, string distance, string event_type, string event_status)
        {//start of create event

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            // Keep what the user typed in case something goes wrong
            string[] form = new string[] { event_name ?? "", event_description ?? "", event_date ?? "",
                event_location ?? "", distance ?? "", event_type ?? "Run", event_status ?? "Planned" };

            ViewBag.mode = "Create";
            ViewBag.event_values = form;

            string? error = check_event_form(form, out Dictionary<string, string> values);

            if (error != null)
            {
                ViewBag.message = error;
                return View("EventForm");
            }

            var result = await api.post("api/events", values);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.status == 201)
            {
                TempData["message_success"] = "Your event has been created. Now add its distances and categories.";

                // Find the new event (the newest one with this name and location) and open its categories
                var mine = await load_my_events();

                string[]? created = mine.events
                    .Where(e => e[1] == values["event_name"] && e[4] == values["event_location"])
                    .OrderByDescending(e => int.Parse(e[0]))
                    .FirstOrDefault();

                if (created != null)
                {
                    return RedirectToAction("Categories", new { event_id = created[0] });
                }

                return RedirectToAction("Events");
            }

            ViewBag.message = result.status == 0 ? result.message : "We could not create the event, please check your details and try again.";

            return View("EventForm");
        }//end of create event

        // GET: /Organizer/EditEvent/300000
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {//start of edit event page

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            // Only the organizer's own events are in this list
            string[]? event_row = mine.events.FirstOrDefault(e => e[0] == id.ToString());

            if (event_row == null)
            {
                TempData["message_error"] = mine.status == 0 ? mine.message : "That event could not be found in your events.";
                return RedirectToAction("Events");
            }

            string date_value = event_row[9];

            if (DateTime.TryParse(event_row[9], out DateTime date))
            {
                date_value = date.ToString("yyyy-MM-ddTHH:mm");
            }

            ViewBag.mode = "Edit";
            ViewBag.event_id = id.ToString();
            ViewBag.event_values = new string[] { event_row[1], event_row[2], date_value, event_row[4],
                event_row[5], event_row[6], event_row[7] };

            return View("EventForm");
        }//end of edit event page

        // POST: /Organizer/EditEvent/300000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, string event_name, string event_description, string event_date,
            string event_location, string distance, string event_type, string event_status)
        {//start of edit event

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            string[] form = new string[] { event_name ?? "", event_description ?? "", event_date ?? "",
                event_location ?? "", distance ?? "", event_type ?? "Run", event_status ?? "Planned" };

            ViewBag.mode = "Edit";
            ViewBag.event_id = id.ToString();
            ViewBag.event_values = form;

            string? error = check_event_form(form, out Dictionary<string, string> values);

            if (error != null)
            {
                ViewBag.message = error;
                return View("EventForm");
            }

            var result = await api.put("api/events/" + id, values);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Event updated"))
            {
                TempData["message_success"] = "Your event has been updated.";
                return RedirectToAction("Events");
            }

            if (result.status == 403)
            {
                TempData["message_error"] = "You can only change your own events.";
                return RedirectToAction("Events");
            }

            ViewBag.message = result.status == 0 ? result.message : "We could not update the event, please check your details and try again.";

            return View("EventForm");
        }//end of edit event

        // POST: /Organizer/DeleteEvent/300000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {//start of delete event

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var result = await api.delete("api/events/" + id);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Event deleted"))
            {
                TempData["message_success"] = "The event has been deleted.";
            }
            else if (result.status == 409)
            {
                TempData["message_error"] = "This event still has categories or enrolments, so it cannot be deleted. Set its status to Cancelled instead.";
            }
            else if (result.status == 403)
            {
                TempData["message_error"] = "You can only delete your own events.";
            }
            else if (result.status == 0)
            {
                TempData["message_error"] = result.message;
            }
            else
            {
                TempData["message_error"] = "We could not delete that event, please try again.";
            }

            return RedirectToAction("Events");
        }//end of delete event

        // GET: /Organizer/Categories?event_id=300000
        [HttpGet]
        public async Task<IActionResult> Categories(int? event_id)
        {//start of categories

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            if (mine.status == 0)
            {
                ViewBag.message = mine.message;
            }

            // The events for the dropdown
            ViewBag.events = mine.events;

            List<string[]> categories = new List<string[]>();

            if (event_id.HasValue)
            {
                // Only the organizer's own events may be opened
                string[]? event_row = mine.events.FirstOrDefault(e => e[0] == event_id.Value.ToString());

                if (event_row == null)
                {
                    TempData["message_error"] = "That event could not be found in your events.";
                    return RedirectToAction("Categories");
                }

                ViewBag.selected_event = event_row;

                var result = await load_event_categories(event_id.Value);

                if (result.status == 401)
                {
                    return session_expired();
                }

                if (result.status == 0)
                {
                    ViewBag.message = result.message;
                }

                categories = result.categories;
            }

            return View(categories);
        }//end of categories

        // GET: /Organizer/CreateCategory?event_id=300000
        [HttpGet]
        public async Task<IActionResult> CreateCategory(int event_id)
        {//start of create category page

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            if (!await set_event_name(event_id))
            {
                TempData["message_error"] = "That event could not be found in your events.";
                return RedirectToAction("Categories");
            }

            ViewBag.mode = "Create";
            ViewBag.event_id = event_id.ToString();
            ViewBag.category_values = new string[] { "Run", "0", "", "500" };

            return View("CategoryForm");
        }//end of create category page

        // POST: /Organizer/CreateCategory?event_id=300000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(int event_id, string category_name, string entry_fee,
            string race_distance, string max_participants, string? add_another = null)
        {//start of create category

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            // Keep what the user typed in case something goes wrong
            string[] form = new string[] { category_name ?? "Run", entry_fee ?? "", race_distance ?? "", max_participants ?? "" };

            ViewBag.mode = "Create";
            ViewBag.event_id = event_id.ToString();
            ViewBag.category_values = form;

            string? error = check_category_form(form, out Dictionary<string, string> values);

            if (error != null)
            {
                await set_event_name(event_id);
                ViewBag.message = error;
                return View("CategoryForm");
            }

            var result = await api.post("api/events/" + event_id + "/categories", values);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.status == 201)
            {
                TempData["message_success"] = "The category has been added.";

                // "Save and add another" opens an empty form for the next distance
                if (add_another != null)
                {
                    return RedirectToAction("CreateCategory", new { event_id = event_id });
                }

                return RedirectToAction("Categories", new { event_id = event_id });
            }

            await set_event_name(event_id);

            if (result.status == 409)
            {
                ViewBag.message = "This event already has a category with the same type and distance.";
            }
            else if (result.status == 403)
            {
                ViewBag.message = "You can only add categories to your own events.";
            }
            else
            {
                ViewBag.message = result.status == 0 ? result.message : "We could not add the category, please check your details and try again.";
            }

            return View("CategoryForm");
        }//end of create category

        // GET: /Organizer/EditCategory/400000?event_id=300000
        [HttpGet]
        public async Task<IActionResult> EditCategory(int id, int event_id)
        {//start of edit category page

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            if (!await set_event_name(event_id))
            {
                TempData["message_error"] = "That event could not be found in your events.";
                return RedirectToAction("Categories");
            }

            var result = await load_event_categories(event_id);

            if (result.status == 401)
            {
                return session_expired();
            }

            string[]? category = result.categories.FirstOrDefault(c => c[0] == id.ToString());

            if (category == null)
            {
                TempData["message_error"] = result.status == 0 ? result.message : "That category could not be found.";
                return RedirectToAction("Categories", new { event_id = event_id });
            }

            ViewBag.mode = "Edit";
            ViewBag.event_id = event_id.ToString();
            ViewBag.category_id = id.ToString();
            ViewBag.category_values = new string[] { category[1], tidy(category[2]), category[3], category[4] };

            return View("CategoryForm");
        }//end of edit category page

        // POST: /Organizer/EditCategory/400000?event_id=300000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, int event_id, string category_name, string entry_fee,
            string race_distance, string max_participants)
        {//start of edit category

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            string[] form = new string[] { category_name ?? "Run", entry_fee ?? "", race_distance ?? "", max_participants ?? "" };

            ViewBag.mode = "Edit";
            ViewBag.event_id = event_id.ToString();
            ViewBag.category_id = id.ToString();
            ViewBag.category_values = form;

            string? error = check_category_form(form, out Dictionary<string, string> values);

            if (error != null)
            {
                await set_event_name(event_id);
                ViewBag.message = error;
                return View("CategoryForm");
            }

            var result = await api.put("api/categories/" + id, values);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Category updated"))
            {
                TempData["message_success"] = "The category has been updated.";
                return RedirectToAction("Categories", new { event_id = event_id });
            }

            if (result.status == 403)
            {
                TempData["message_error"] = "You can only change categories of your own events.";
                return RedirectToAction("Categories");
            }

            await set_event_name(event_id);

            if (result.status == 409)
            {
                ViewBag.message = "This event already has a category with the same type and distance.";
            }
            else
            {
                ViewBag.message = result.status == 0 ? result.message : "We could not update the category, please check your details and try again.";
            }

            return View("CategoryForm");
        }//end of edit category

        // POST: /Organizer/DeleteCategory/400000?event_id=300000
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id, int event_id)
        {//start of delete category

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var result = await api.delete("api/categories/" + id);

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Category deleted"))
            {
                TempData["message_success"] = "The category has been deleted.";
            }
            else if (result.status == 409)
            {
                TempData["message_error"] = "Participants are already enrolled in this category, so it cannot be deleted.";
            }
            else if (result.status == 403)
            {
                TempData["message_error"] = "You can only delete categories of your own events.";
            }
            else if (result.status == 0)
            {
                TempData["message_error"] = result.message;
            }
            else
            {
                TempData["message_error"] = "We could not delete that category, please try again.";
            }

            return RedirectToAction("Categories", new { event_id = event_id });
        }//end of delete category

        // GET: /Organizer/Enrolments?event_id=300000
        [HttpGet]
        public async Task<IActionResult> Enrolments(int? event_id)
        {//start of enrolments

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            if (mine.status == 0)
            {
                ViewBag.message = mine.message;
            }

            // The events for the dropdown
            ViewBag.events = mine.events;

            // 0 id, 1 participant, 2 username, 3 category, 4 entered on, 5 status
            List<string[]> enrolments = new List<string[]>();

            if (event_id.HasValue)
            {
                // Only the organizer's own events may be opened
                string[]? event_row = mine.events.FirstOrDefault(e => e[0] == event_id.Value.ToString());

                if (event_row == null)
                {
                    TempData["message_error"] = "That event could not be found in your events.";
                    return RedirectToAction("Enrolments");
                }

                ViewBag.selected_event = event_row;

                var result = await api.get("api/events/" + event_id.Value + "/enrolments");

                if (result.status == 401)
                {
                    return session_expired();
                }

                if (result.status == 0)
                {
                    ViewBag.message = result.message;
                }
                else if (result.status == 200 && !result.message.StartsWith("No enrolments"))
                {
                    // id - first and last name - username - category - enrolled on - status
                    foreach (string line in get_lines(result.message))
                    {
                        string[] parts = line.Split(" - ");

                        if (parts.Length >= 6)
                        {
                            int last = parts.Length - 1;

                            // Any extra " - " pieces belong to the name
                            string full_name = string.Join(" - ", parts.Skip(1).Take(parts.Length - 5));

                            enrolments.Add(new string[] { parts[0], full_name, parts[last - 3], parts[last - 2],
                                show_date(parts[last - 1]), parts[last] });
                        }
                    }
                }

                // Numbers for the summary (cancelled enrolments are not counted as entrants)
                ViewBag.active_count = enrolments.Count(e => e[5] != "Cancelled");
                ViewBag.cancelled_count = enrolments.Count(e => e[5] == "Cancelled");
                ViewBag.category_counts = enrolments
                    .Where(e => e[5] != "Cancelled")
                    .GroupBy(e => e[3])
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            return View(enrolments);
        }//end of enrolments

        // GET: /Organizer/Results?event_id=300000
        [HttpGet]
        public async Task<IActionResult> Results(int? event_id)
        {//start of results

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            var mine = await load_my_events();

            if (mine.status == 401)
            {
                return session_expired();
            }

            if (mine.status == 0)
            {
                ViewBag.message = mine.message;
            }

            // The events for the dropdown
            ViewBag.events = mine.events;

            // 0 enrolment id, 1 participant, 2 category, 3 finish time, 4 overall position, 5 category position, 6 result status (blank when none)
            List<string[]> rows = new List<string[]>();

            if (event_id.HasValue)
            {
                // Only the organizer's own events may be opened
                string[]? event_row = mine.events.FirstOrDefault(e => e[0] == event_id.Value.ToString());

                if (event_row == null)
                {
                    TempData["message_error"] = "That event could not be found in your events.";
                    return RedirectToAction("Results");
                }

                ViewBag.selected_event = event_row;

                // The people who entered the event
                var enrolment_result = await api.get("api/events/" + event_id.Value + "/enrolments");

                if (enrolment_result.status == 401)
                {
                    return session_expired();
                }

                if (enrolment_result.status == 0)
                {
                    ViewBag.message = enrolment_result.message;
                    return View(rows);
                }

                // The results that were already captured: name and category -> finish time, positions, status
                Dictionary<string, string[]> captured = new Dictionary<string, string[]>();

                var result_result = await api.get("api/events/" + event_id.Value + "/results");

                if (result_result.status == 200 && !result_result.message.StartsWith("No results"))
                {
                    // id - first and last name - category - finish time - overall position - category position - status
                    foreach (string line in get_lines(result_result.message))
                    {
                        string[] parts = line.Split(" - ");

                        if (parts.Length >= 7)
                        {
                            int last = parts.Length - 1;

                            // Any extra " - " pieces belong to the name
                            string full_name = string.Join(" - ", parts.Skip(1).Take(parts.Length - 6));

                            string finish_time = parts[last - 3];

                            if (finish_time.Length > 8)
                            {
                                finish_time = finish_time.Substring(0, 8);
                            }

                            captured.TryAdd(full_name + "|" + parts[last - 4],
                                new string[] { finish_time, parts[last - 2], parts[last - 1], parts[last] });
                        }
                    }
                }

                if (enrolment_result.status == 200 && !enrolment_result.message.StartsWith("No enrolments"))
                {
                    // id - first and last name - username - category - enrolled on - status
                    foreach (string line in get_lines(enrolment_result.message))
                    {
                        string[] parts = line.Split(" - ");

                        if (parts.Length >= 6)
                        {
                            int last = parts.Length - 1;

                            // A cancelled enrolment cannot have a result
                            if (parts[last] == "Cancelled")
                            {
                                continue;
                            }

                            string full_name = string.Join(" - ", parts.Skip(1).Take(parts.Length - 5));
                            string category = parts[last - 2];

                            if (captured.TryGetValue(full_name + "|" + category, out string[]? saved))
                            {
                                rows.Add(new string[] { parts[0], full_name, category, saved[0], saved[1], saved[2], saved[3] });
                            }
                            else
                            {
                                rows.Add(new string[] { parts[0], full_name, category, "", "", "", "" });
                            }
                        }
                    }
                }
            }

            return View(rows);
        }//end of results

        // POST: /Organizer/SaveResult
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveResult(int event_id, int enrolment_id, string finish_time,
            string overall_position, string category_position, string results_status)
        {//start of save result

            IActionResult? blocked = check_organizer();

            if (blocked != null)
            {
                return blocked;
            }

            string? error = null;
            TimeSpan time = TimeSpan.Zero;
            int overall = 0;
            int category = 0;

            bool has_time = !string.IsNullOrWhiteSpace(finish_time);
            bool has_overall = !string.IsNullOrWhiteSpace(overall_position);
            bool has_category = !string.IsNullOrWhiteSpace(category_position);

            if (!new string[] { "Finished", "DNF", "DSQ", "DNS" }.Contains(results_status))
            {
                error = "Please choose a valid result status.";
            }
            else if (has_time && (!TimeSpan.TryParse(finish_time, CultureInfo.InvariantCulture, out time)
                || time < TimeSpan.Zero || time >= TimeSpan.FromHours(24)))
            {
                error = "The finish time must look like 01:23:45 (hours:minutes:seconds).";
            }
            else if (results_status == "Finished" && !has_time)
            {
                error = "A finisher needs a finish time.";
            }
            else if (has_overall && (!int.TryParse(overall_position, out overall) || overall <= 0))
            {
                error = "The overall position must be a whole number above 0.";
            }
            else if (has_category && (!int.TryParse(category_position, out category) || category <= 0))
            {
                error = "The category position must be a whole number above 0.";
            }

            if (error != null)
            {
                TempData["message_error"] = error;
                return RedirectToAction("Results", new { event_id = event_id });
            }

            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("results_status", results_status);

            if (has_time)
            {
                values.Add("finish_time", time.ToString(@"hh\:mm\:ss"));
            }

            if (has_overall)
            {
                values.Add("overall_position", overall.ToString());
            }

            if (has_category)
            {
                values.Add("category_position", category.ToString());
            }

            // Try to capture a new result first
            var result = await api.post("api/enrolments/" + enrolment_id + "/result", values);

            // A result already exists, so update it instead
            if (result.status == 409 && result.message.Contains("already exists"))
            {
                result = await api.put("api/enrolments/" + enrolment_id + "/result", values);
            }

            if (result.status == 401)
            {
                return session_expired();
            }

            if (result.message.StartsWith("Result captured") || result.message.StartsWith("Result updated"))
            {
                TempData["message_success"] = "The result has been saved.";
            }
            else if (result.status == 403)
            {
                TempData["message_error"] = "You can only capture results for your own events.";
            }
            else if (result.status == 409)
            {
                TempData["message_error"] = "A cancelled enrolment cannot have a result.";
            }
            else if (result.status == 0)
            {
                TempData["message_error"] = result.message;
            }
            else
            {
                TempData["message_error"] = "We could not save that result, please check the details and try again.";
            }

            return RedirectToAction("Results", new { event_id = event_id });
        }//end of save result

    }//end of class
}//end of namespace
