using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using eHealth.Service.IService;
using Xamarin.Forms;
using System.Threading;
using System;

[assembly: Dependency(typeof(eHealth.Droid.Service.NotificationService))]
namespace eHealth.Droid.Service
{
    public class NotificationService : INotifyService
    {
        private const string CancelAction = "eHealth.Droid.Service.CANCEL_NOTIFICATION";
        private readonly Context _context;
        private CancellationTokenSource _cts; // Use a field to manage the cancellation

        public NotificationService()
        {
            _context = Android.App.Application.Context;
            RegisterCancelReceiver();
        }

        public void ShowNotification(string title, string message, int notificationId)
        {
            var intent = new Intent(_context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            var pendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.OneShot);

            var cancelIntent = new Intent(CancelAction);
            cancelIntent.PutExtra("notificationId", notificationId);
            var cancelPendingIntent = PendingIntent.GetBroadcast(_context, notificationId, cancelIntent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(_context, "default_channel")
                .SetSmallIcon(Resource.Drawable.icon_feed) // Replace with your app icon
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.High)
                .SetContentIntent(pendingIntent)
                .AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.icon_about, "Cancel", cancelPendingIntent).Build());

            var notificationManagerCompat = NotificationManagerCompat.From(_context);

            // Create notification channel for Android Oreo and above
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }

            // Use the unique notificationId to ensure each notification is treated separately
            notificationManagerCompat.Notify(notificationId, notificationBuilder.Build());
        }

        public void NotifyWithCountdown(string title, string message, int countdownMinutes, int notificationId, CancellationTokenSource cts)
        {
            _cts = cts ?? new CancellationTokenSource(); // Initialize if null

            var intent = new Intent(CancelAction);
            intent.PutExtra("notificationId", notificationId);
            var cancelPendingIntent = PendingIntent.GetBroadcast(_context, notificationId, intent, PendingIntentFlags.OneShot);

            var notificationBuilder = new NotificationCompat.Builder(_context, "default_channel")
                .SetSmallIcon(Resource.Drawable.icon_feed) // Replace with your app icon
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetPriority((int)NotificationPriority.High)
                .SetContentIntent(PendingIntent.GetActivity(_context, 0, new Intent(_context, typeof(MainActivity)), PendingIntentFlags.OneShot))
                .AddAction(new NotificationCompat.Action.Builder(Resource.Drawable.icon_about, "Cancel", cancelPendingIntent).Build());

            var notificationManagerCompat = NotificationManagerCompat.From(_context);

            // Create notification channel for Android Oreo and above
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                CreateNotificationChannel();
            }

            // Use the unique notificationId to ensure each notification is treated separately
            notificationManagerCompat.Notify(notificationId, notificationBuilder.Build());
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel("default_channel", "General Notifications", NotificationImportance.Default)
                {
                    Description = "General notifications"
                };

                var notificationManager = (NotificationManager)_context.GetSystemService(Context.NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        public void CancelNotification(int notificationId)
        {
            var notificationManager = NotificationManagerCompat.From(_context);
            notificationManager.Cancel(notificationId);  // Cancel specific notification by ID
        }

        private void RegisterCancelReceiver()
        {
            var cancelReceiver = new NotificationCancelReceiver(this);
            var intentFilter = new IntentFilter(CancelAction);
            _context.RegisterReceiver(cancelReceiver, intentFilter);
        }

        // Custom BroadcastReceiver to handle cancellation
        private class NotificationCancelReceiver : BroadcastReceiver
        {
            private readonly NotificationService _notificationService;

            public NotificationCancelReceiver(NotificationService notificationService)
            {
                _notificationService = notificationService;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action == CancelAction)
                {
                    int notificationId = intent.GetIntExtra("notificationId", 0);
                    // Cancel the notification
                    _notificationService.CancelNotification(notificationId);

                    // Cancel the operation by triggering the cancellation token
                    _notificationService._cts?.Cancel(); // Check if _cts is not null before calling Cancel

                    // Cancel the emergency contact procedure
                    var eContactService = DependencyService.Get<IEContactService<eHealth.Data.Models.EmergencyContacts>>();
                    eContactService?.CancelAlert(); // Ensure it's not null before calling
                }
            }
        }
    }
}
