using SQLite;
using System;

namespace eHealth.Data.Models
{
    public class SensorData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public double ValueX { get; set; }
        public double ValueY { get; set; }
        public double ValueZ { get; set; }
        public double Magnitude { get; set; }
        
        public string UserEmail { get; set; }

      

    }


}
