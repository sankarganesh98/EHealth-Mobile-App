using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using eHealth.Droid.Service;
using eHealth.Droid.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using eHealth.Service.IService;


namespace eHealth.Droid
{
    [Activity(Label = "eHealth", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        const int RequestPermissionsId = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            DependencyService.Register<INotifyService, NotificationService>();

            LoadApplication(new App());

            RequestPermissions();
            CreateNotificationChannel();
            StartForegroundServiceCompat();
        }

        void RequestPermissions()
        {
            var permissions = new string[]
            {
                Manifest.Permission.WakeLock,
                Manifest.Permission.ForegroundService,
                Manifest.Permission.RequestIgnoreBatteryOptimizations,
                Manifest.Permission.ReadExternalStorage,
                Manifest.Permission.WriteExternalStorage,
                Manifest.Permission.CallPhone,
                Manifest.Permission.SendSms,
                Manifest.Permission.Internet
            };

            if (!CheckPermissions(permissions))
            {
                ActivityCompat.RequestPermissions(this, permissions, RequestPermissionsId);
            }
        }

        bool CheckPermissions(string[] permissions)
        {
            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != Permission.Granted)
                {
                    return false;
                }
            }
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelName = "eHealth Service Channel";
                var channelDescription = "Channel for eHealth background service";
                var channel = new NotificationChannel("ehealth_channel", channelName, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void StartForegroundServiceCompat()
        {
            var intent = new Intent(this, typeof(AccelerometerService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(intent);
            }
            else
            {
                StartService(intent);
            }
        }
    }
}
