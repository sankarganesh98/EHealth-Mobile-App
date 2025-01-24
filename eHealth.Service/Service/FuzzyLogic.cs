using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using eHealth.Data.Models;

namespace eHealth.Service.FuzzyLogic
{
    public class FuzzyLogic
    {
        private List<SensorData> _historicData;

        public FuzzyLogic(List<SensorData> historicData)
        {
            _historicData = historicData;
        }
        public double FuzzifyActivityLevel(double magnitude)
        {
            if (magnitude > 1.2)
            {
               // Debug.WriteLine("Activity level is High.");
                return 0.8; //High
            }
            else
            {
                //Debug.WriteLine("Activity level is Low.");
                return 0.2; //Low
            }
        }
        public double FuzzifyTimeOfDay(DateTime dateTime)
        {
            var hour = dateTime.Hour;
            if (hour < 6)
            {
                return 0.9; //Night
            }
            else if (hour < 18)
            {
                return 0.1; //Day
            }
            else
            {
                return 0.7;//Evening
            }
        }
        public double InferAbnormality(double magnitude, DateTime dateTime)
        {
            double fuzzyActivity = FuzzifyActivityLevel(magnitude);
            double fuzzyTime = FuzzifyTimeOfDay(dateTime);

            double expectedActivity = GetExpectedActivity(dateTime);

            if (fuzzyActivity < 0.5 && expectedActivity > 0.5)
            {
                Debug.WriteLine("Activity level is 0.2 - abnormal.");
                return 0.2; // High abnormality
            }
            else if (fuzzyActivity > 0.5 && expectedActivity < 0.5)
            {
                Debug.WriteLine("Activity level is 0.8 - normal.");
                return 0.8;
            }
            else 
            {
                Debug.WriteLine($"activity level is {fuzzyActivity} - fuzzy activity");
                return fuzzyActivity;
            }   
        }
        private double GetExpectedActivity(DateTime timeOfDay)
        {
            var similarTimes = _historicData.Where(record => Math.Abs((record.DateTime - timeOfDay).TotalHours) < 1).ToList();
            
           // Debug.WriteLine($"Found {similarTimes.Count} similar time records.");
            if (similarTimes.Count == 0)
            {
               // Debug.WriteLine("No similar times found, returning default abnormality value.");
                return 0.5;
            }

            var averageActivityLevel = similarTimes.Average(record => FuzzifyActivityLevel(record.Magnitude));
           // Debug.WriteLine($"Average activity level for similar times: {averageActivityLevel}");

            return averageActivityLevel;

        }

        
        
    }
}
