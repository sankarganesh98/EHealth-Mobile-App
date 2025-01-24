using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Essentials;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using eHealth.Data.Models;
using eHealth.Data;
using eHealth.Service.FuzzyLogic;
using eHealth.Service.IService;
using eHealth.Service.Service;
using static Android.OS.PowerManager;

namespace eHealth.Droid.Services
{
    [Service(Enabled = true, Exported = true)]
    public class AccelerometerService : Android.App.Service
    {
        private readonly eHealthDatabase _database;
        private WakeLock wakeLock;
        private readonly List<AccelerometerData> _accelerometerDataList = new List<AccelerometerData>();
        private readonly List<AccelerometerData> _greatestMagnitudeDataList = new List<AccelerometerData>();
        private readonly Timer _dataCollectionTimer = new Timer(1000); // 1 second interval
        private int _secondsCount = 0;
        private int _abnormalityCount = 0;
        private FuzzyLogic _fuzzyLogic;
        private string _senderEmail;
        private string _senderPassword;
        private readonly IEContactService<EmergencyContacts> _econtactService;

        public AccelerometerService()
        {
            _database = new eHealthDatabase(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "eHealth.db3"));
            _econtactService = new EContactService();
            InitializeFuzzyLogic();
        }

        private void InitializeFuzzyLogic()
        {
            try
            {
                var historicData = new List<SensorData>(); // Retrieve historical data if needed
                _fuzzyLogic = new FuzzyLogic(historicData);
                System.Diagnostics.Debug.WriteLine("FuzzyLogic initialized.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing FuzzyLogic: {ex.Message}");
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;  // No binding provided
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            StartForegroundService();
            RequestIgnoreBatteryOptimizations();
            AcquireWakeLock();
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            _dataCollectionTimer.Elapsed += OnDataCollectionTimerElapsed;

            if (!Accelerometer.IsMonitoring)
            {
                Accelerometer.Start(SensorSpeed.Default);
            }
            _dataCollectionTimer.Start();

            return StartCommandResult.Sticky;
        }

        private void RequestIgnoreBatteryOptimizations()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
                if (!pm.IsIgnoringBatteryOptimizations(PackageName))
                {
                    Intent intent = new Intent(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                    intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                }
            }
        }

        private void AcquireWakeLock()
        {
            PowerManager powerManager = (PowerManager)GetSystemService(Context.PowerService);
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "eHealth::WakeLockTag");
            wakeLock.Acquire();
        }

        private void ReleaseWakeLock()
        {
            if (wakeLock != null && wakeLock.IsHeld)
            {
                wakeLock.Release();
                wakeLock = null;
            }
        }

        private void StartForegroundService()
        {
            string channelId = "ehealth_channel";
            string channelName = "eHealth Service Channel";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
                {
                    Description = "Channel for eHealth background service"
                };
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(notificationChannel);
            }

            var notification = new Notification.Builder(this, channelId)
                .SetContentTitle("eHealth Service")
                .SetContentText("eHealth accelerometer service is running")
                .SetSmallIcon(Resource.Drawable.icon_about)
                .SetOngoing(true)
                .Build();

            StartForeground(1, notification);
        }

        private void StopForegroundService()
        {
            StopForeground(true);
            StopSelf();
        }

        private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            var data = e.Reading;
            var sensorData = new AccelerometerData
            {
                X = data.Acceleration.X,
                Y = data.Acceleration.Y,
                Z = data.Acceleration.Z,
                Timestamp = DateTime.Now
            };

            lock (_accelerometerDataList)
            {
                _accelerometerDataList.Add(sensorData);
            }
        }

        private async void OnDataCollectionTimerElapsed(object sender, ElapsedEventArgs e)
        {
            AccelerometerData greatestMagnitudeData;
            lock (_accelerometerDataList)
            {
                if (_accelerometerDataList.Count == 0) return;
                greatestMagnitudeData = _accelerometerDataList.OrderByDescending(d => d.Magnitude).First();
                _accelerometerDataList.Clear();
            }

            lock (_greatestMagnitudeDataList)
            {
                _greatestMagnitudeDataList.Add(greatestMagnitudeData);
            }

            _secondsCount++;
            System.Diagnostics.Debug.WriteLine($"Magnitude : {greatestMagnitudeData.Magnitude}");

            if (greatestMagnitudeData.Magnitude > 10)
            {
                await HandleEmergency("Fall Detected");
            }

            if (greatestMagnitudeData.Magnitude < 1.2)
            {

                double abnormality = _fuzzyLogic.InferAbnormality(greatestMagnitudeData.Magnitude, greatestMagnitudeData.Timestamp);
                if (abnormality < 0.5)
                {
                    _abnormalityCount++;
                    System.Diagnostics.Debug.WriteLine($"abnormality count : {_abnormalityCount}");

                }
            }
            else
            {
                _abnormalityCount = 0;
            }
            System.Diagnostics.Debug.WriteLine($"Seconds count : {_secondsCount}");

            if (_secondsCount >= 60)
            {
                var greatestOverallData = _greatestMagnitudeDataList.OrderByDescending(d => d.Magnitude).First();
                var dbSensorData = new SensorData
                {
                    ValueX = greatestOverallData.X,
                    ValueY = greatestOverallData.Y,
                    ValueZ = greatestOverallData.Z,
                    Magnitude = greatestOverallData.Magnitude,
                    DateTime = DateTime.Now,
                  
                };

                await _database.SaveSensorDataAsync(dbSensorData);
                System.Diagnostics.Debug.WriteLine($"values saved : {dbSensorData}");
                _secondsCount = 0;
                await _database.DeleteOldSensorDataAsync(); // Call to delete old data
            }
            //4 hours
            if (_abnormalityCount >= 1800)
            {
                await HandleEmergency("Idleness detected ");
                _abnormalityCount = 0; // Reset abnormality count after handling emergency

            }
        }

        

        private async Task HandleEmergency(string emergencyReason)
        {
            _senderEmail = await SecureStorage.GetAsync("email");
            _senderPassword = await SecureStorage.GetAsync("password");

            if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
            {
                System.Diagnostics.Debug.WriteLine("Email or password not found in secure storage.");
                throw new InvalidOperationException("Email or password not found in secure storage.");
            }

            System.Diagnostics.Debug.WriteLine($"Handling emergency with email: {_senderEmail}");
            System.Diagnostics.Debug.WriteLine($"Handling emergency with Password: {_senderPassword}");
            await _econtactService.NotifyUserBeforeAlert(_senderEmail, _senderPassword, emergencyReason);
           // await _econtactService.HandleEmergency(_senderEmail, _senderPassword);
            System.Diagnostics.Debug.WriteLine("Emergency handled.");
        }

        public override void OnDestroy()
        {
            Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.Stop();
            }
            _dataCollectionTimer.Elapsed -= OnDataCollectionTimerElapsed;
            _dataCollectionTimer.Stop();
            ReleaseWakeLock();
            base.OnDestroy();

            // Restart service if destroyed
            Intent broadcastIntent = new Intent(this, typeof(RestartServiceReceiver));
            SendBroadcast(broadcastIntent);
        }
    }

    public class AccelerometerData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public DateTime Timestamp { get; set; }
        public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
    }

    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class RestartServiceReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            context.StartService(new Intent(context, typeof(AccelerometerService)));
        }
    }
}