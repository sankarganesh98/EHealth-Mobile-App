using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace eHealth.Data.Models
{
    public class AlertRecord
    {
        [PrimaryKey, AutoIncrement]

        public int alertId { get; set; }
        public DateTime alert_DateTime { get; set; }
        public int userId { get; set; }
        public string userEmail { get; set; }
        public string alertReason {  get; set; }
       


    }
}
