using Microsoft.AspNetCore.Mvc;
using race_day_mvc.Services;

namespace race_day_mvc.Controllers
{//start of namespace 
    public class HomeController : Controller
    {//start of class
        private readonly api_client api;

        public HomeController(api_client api_service)
        {
            api = api_service;
        }

        // GET: /Home/Index (visitors and participants see the upcoming events, participants also get filters)
        public async Task<IActionResult> Index(string? event_type, string? event_location, string? from_date, string? to_date)
        {//start of index

            List<string[]> events = new List<string[]>();

            string? role = HttpContext.Session.GetString("role");

            // Organizers have their own site
            if (role == "Organizer")
            {
                return RedirectToAction("Dashboard", "Organizer");
            }

            // Only send the filters that were filled in
            Dictionary<string, string> query = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(event_type))
            {
                query.Add("event_type", event_type);
            }

            if (!string.IsNullOrWhiteSpace(event_location))
            {
                query.Add("event_location", event_location);
            }

            if (!string.IsNullOrWhiteSpace(from_date))
            {
                query.Add("from_date", from_date);
            }

            if (!string.IsNullOrWhiteSpace(to_date))
            {
                query.Add("to_date", to_date);
            }

            // Ask the API for the events
            var result = await api.get("api/events", query);

            // The API could not be reached
            if (result.status == 0)
            {
                ViewBag.message = result.message;
                return View(events);
            }

            if (result.status == 200 && !result.message.StartsWith("No open"))
            {
                // One event per line: id - name - description - date - location - distance - type - status
                foreach (string line in result.message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = line.Split(" - ");

                    if (parts.Length >= 8)
                    {
                        // Any extra " - " pieces belong to the description
                        string description = string.Join(" - ", parts.Skip(2).Take(parts.Length - 7));

                        string date_text = parts[parts.Length - 5];

                        if (DateTime.TryParse(date_text, out DateTime event_date))
                        {
                            date_text = event_date.ToString("ddd, d MMM yyyy HH:mm");
                        }

                        events.Add(new string[] { parts[0], parts[1], description, date_text,
                            parts[parts.Length - 4], parts[parts.Length - 3], parts[parts.Length - 2], parts[parts.Length - 1] });
                    }
                }
            }

            return View(events);
        }//end of index

        // Shown when something unexpected goes wrong (used by Program.cs)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {//start of error

            return View();

        }//end of error
    }//end of class
}//end of namespace
