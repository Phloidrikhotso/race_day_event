namespace race_day_event.Models
{
    public class events
    {

        public int event_id { get; set; }

        public int organizer_id { get; set; }

        public string event_name { get; set; } = string.Empty;

        public string? event_description { get; set; }

        public DateTime event_date { get; set; }

        public string event_location { get; set; } = string.Empty;

        public decimal distance { get; set; }

        public string event_type { get; set; } = string.Empty;

        public string event_status { get; set; } = "Planned";

        public DateTime? date_created { get; set; }

    }
}
