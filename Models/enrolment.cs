namespace race_day_event.Models
{//start of namespace
    public class enrolment
    {//end of class

        public int enrolment_id { get; set; }

        public int userID { get; set; }

        public int event_id { get; set; }

        public int category_id { get; set; }

        public DateTime? enrolment_date { get; set; }

        public string enrolment_status { get; set; } = "Confirmed";

    }//end of class
}//end of namespace
