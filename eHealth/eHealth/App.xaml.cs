using eHealth.Services;
using eHealth.Views;
using System;
using System.Diagnostics;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using eHealth.Data;
using eHealth.Service.IService;
using eHealth.Service.Service;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace eHealth
{
    public partial class App : Application
    {
        static eHealthDatabase database;
        static SensorService sensorService;

        // Create the database connection as a singleton.
        public static eHealthDatabase Database
        {
            get
            {
                if (database == null)
                {
                    string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "eHealth.db3");
                    Debug.WriteLine($"Database path: {dbPath}");
                    database = new eHealthDatabase(dbPath);
                }
                return database;
            }
        }

        public static SensorService SensorService
        {
            get
            {
                if (sensorService == null)
                {
                    sensorService = new SensorService(Database); // Ensure the database is initialized before SensorService
                }
                return sensorService;
            }
        }

        public App()
        {
            InitializeComponent();
            DependencyService.Register<UserService>(); // Registering only UserService
            MainPage = new AppShell();
        }

        protected override async void OnStart()
        {
            try
            {
                // Handle when your app starts
                await InitializeDatabaseAsync();
                StartAccelerometerService();

                await SecureStorage.SetAsync("email", "ehealthuseralert@gmail.com");
                await SecureStorage.SetAsync("password", "nvds nbze xzkz ytht");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during OnStart: {ex}");
                // Handle or report the error appropriately
            }
        }

        protected override void OnSleep()
        {
            Debug.WriteLine("OnSleep called");
            // Handle when your app sleeps
            StopAccelerometerService();
        }

        protected override void OnResume()
        {
            Debug.WriteLine("OnResume called");
            // Handle when your app resumes
            StartAccelerometerService();
        }

        private async Task InitializeDatabaseAsync()
        {
            try
            {
                // Ensuring the database is created and initialized asynchronously
                Debug.WriteLine("Initializing database...");
                await Database.InitializeAsync();
                Debug.WriteLine("Database initialization complete.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during database initialization: {ex}");
                // Handle or report the error appropriately
            }
        }

        private void StartAccelerometerService()
        {
            var accelerometerService = DependencyService.Get<IAccelerometerService>();
            if (accelerometerService != null)
            {
                accelerometerService.StartService();
            }
            else
            {
                Debug.WriteLine("AccelerometerService not found in DependencyService.");
            }
        }

        private void StopAccelerometerService()
        {
            var accelerometerService = DependencyService.Get<IAccelerometerService>();
            if (accelerometerService != null)
            {
                accelerometerService.StopService();
            }
            else
            {
                Debug.WriteLine("AccelerometerService not found in DependencyService.");
            }
        }
    }
}
