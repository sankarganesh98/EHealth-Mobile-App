using System.Threading;

namespace eHealth.Service.IService
{
    public interface INotifyService
    {
        void ShowNotification(string title, string message, int notificationId);
        void NotifyWithCountdown(string title, string message, int countdownMinutes, int notificationId, CancellationTokenSource cts);
        void CancelNotification(int notificationId);
    }
}
