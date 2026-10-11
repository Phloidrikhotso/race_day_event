namespace race_day_event.Models
{//start of namespace
    public class result
    {//start of class

        public int result_id { get; set; }

        public int enrolment_id { get; set; }

        public TimeSpan? finish_time { get; set; }

        public int? overall_position { get; set; }

        public int? category_position { get; set; }

        public string results_status { get; set; } = "Finished";

    }//end of class
}//end of namespace
