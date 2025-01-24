using Android.Content;
using eHealth.Service.IService;
using Xamarin.Forms;
using eHealth.Data.Models;  // Ensure this namespace is correct for your SensorData class

[assembly: Dependency(typeof(eHealth.Droid.Services.AccelerometerServiceImplementation))]
namespace eHealth.Droid.Services
{
    public class AccelerometerServiceImplementation : IAccelerometerService
    {
        public void StartService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(AccelerometerService));
            Android.App.Application.Context.StartService(intent);
        }

        public void StopService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(AccelerometerService));
            Android.App.Application.Context.StopService(intent);
        }

        public void SaveSensorData(SensorData data)
        {
            var intent = new Intent(Android.App.Application.Context, typeof(AccelerometerService));
            intent.PutExtra("sensorData", Newtonsoft.Json.JsonConvert.SerializeObject(data));
            Android.App.Application.Context.StartService(intent);
        }
    }
}
