namespace race_day_event.Models
{//start of namespace
    public class user
    {//start of class

        public int userID { get; set; }

        public int role_id { get; set; }

        public string first_name { get; set; } = string.Empty;

        public string last_name { get; set; } = string.Empty;

        public string username { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string? phone_number { get; set; }

        public string password_hash { get; set; } = string.Empty;

        public DateTime? date_created { get; set; }

    }//end of class
}//end of namespace
