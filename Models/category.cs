namespace race_day_event.Models
{
    public class category
    {

        public int category_id { get; set; }

        public int event_id { get; set; }

        public string category_name { get; set; } = string.Empty;

        public decimal entry_fee { get; set; }

        public decimal? race_distance { get; set; }

        public int max_participants { get; set; } = 500;

    }
}
