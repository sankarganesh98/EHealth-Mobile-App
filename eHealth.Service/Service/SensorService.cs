using System;
using System.Collections.Generic;
using System.Diagnostics; // Added for Debug.WriteLine
using System.Threading.Tasks;
using Xamarin.Essentials;
using eHealth.Data;
using eHealth.Data.Models;
using eHealth.Service.IService;
using System.Timers;
using System.Globalization;

namespace eHealth.Service.Service
{
    public class SensorService : ISensorService
    {
        private readonly eHealthDatabase _database;
        private bool _isAccelerometerActive;
        private DateTime _lastMovementTime;
        private List<SensorData> _currentHourData;
        private Timer _hourlyTimer;

        public SensorService(eHealthDatabase database)
        {
            _database = database;
            _isAccelerometerActive = false;
            _lastMovementTime = DateTime.Now;
            _currentHourData = new List<SensorData>();
            _hourlyTimer = new Timer(3600000); // 1 hour interval (3600000 milliseconds)
            //_hourlyTimer.Elapsed += AggregateHourlyData; // Not uncommented, just logging set up
            _hourlyTimer.Start();
            Debug.WriteLine("SensorService initialized with timer set for hourly aggregation.");
        }

        public async Task InitializeAsync()
        {
            Debug.WriteLine("Initializing database...");
            await _database.InitializeAsync();
            Debug.WriteLine("Database initialized.");
        }

        public void StartAccelerometer()
        {
            Debug.WriteLine("Attempting to start accelerometer monitoring...");
            if (Accelerometer.IsMonitoring)
            {
                Debug.WriteLine("Accelerometer already monitoring.");
                return;
            }

            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            Accelerometer.Start(SensorSpeed.UI);
            _isAccelerometerActive = true;
            Debug.WriteLine("Accelerometer monitoring started.");
        }

        public void StopAccelerometer()
        {
            Debug.WriteLine("Attempting to stop accelerometer monitoring...");
            if (!Accelerometer.IsMonitoring)
            {
                Debug.WriteLine("Accelerometer not monitoring.");
                return;
            }

            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            Accelerometer.Stop();
            _isAccelerometerActive = false;
            Debug.WriteLine("Accelerometer monitoring stopped.");
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            Debug.WriteLine($"Accelerometer reading changed: X={data.Acceleration.X}, Y={data.Acceleration.Y}, Z={data.Acceleration.Z}");
            _lastMovementTime = DateTime.Now; // Update last movement time

            var sensorData = new SensorData
            {
                ValueX = data.Acceleration.X,
                ValueY = data.Acceleration.Y,
                ValueZ = data.Acceleration.Z,
                DateTime = DateTime.Now,
            };
            _currentHourData.Add(sensorData);
            Debug.WriteLine($"Sensor data added. Current hour data count: {_currentHourData.Count}");
        }

        // Method remains commented out and unchanged
        // private async void AggregateHourlyData(object sender, ElapsedEventArgs e)
        // {
        //     if (_currentHourData.Any())
        //     {
        //         ...
        //     }
        // }

        public async Task<List<SensorData>> GetSensorDataAsync()
        {
            Debug.WriteLine("Inside GetSensorDataAsync");
            if (_database == null)
                Debug.WriteLine("Database reference is null");
            else
                Debug.WriteLine("Database is initialized");

            try
            {
                Debug.WriteLine("Attempting to fetch all sensor data...");
                var data = await _database.GetAllSensorDataAsync();
                Debug.WriteLine($"Fetched {data.Count} sensor data entries.");
                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetSensorDataAsync: {ex}");
                throw; // Retain the stack trace by rethrowing the exception without specifying it.
            }
        }

    }
}
