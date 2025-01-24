using System;
using SQLite;
using Xamarin.Forms;

namespace eHealth.Data.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; } 
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public int age { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
       

    }
}