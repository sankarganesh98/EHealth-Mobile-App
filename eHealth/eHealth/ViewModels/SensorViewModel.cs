using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using eHealth.Service.IService;
using eHealth.Data.Models;
using Microcharts;
using SkiaSharp;
using System.Collections.Generic;
using System.Diagnostics;  // Add this line

namespace eHealth.ViewModels
{
    public class SensorViewModel : BaseViewModel
    {
        private readonly ISensorService _sensorService;
        public ObservableCollection<SensorData> SensorDataList { get; }
        public ICommand LoadDataCommand { get; }

        private Chart _chart;
        public Chart Chart
        {
            get => _chart;
            set => SetProperty(ref _chart, value);
        }

        public SensorViewModel()
        {
            _sensorService = App.SensorService ?? throw new InvalidOperationException("Sensor service is not initialized.");
            SensorDataList = new ObservableCollection<SensorData>();

            LoadDataCommand = new Command(async () => await LoadDataAsync());

            // Properly handle the async call in the constructor
            Task.Run(async () => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _sensorService.GetSensorDataAsync();

                if (data == null || !data.Any())
                {
                    // Handle the case where there is no data
                    Chart = null;
                    return;
                }

                var todayData = data.Where(d => d.DateTime.Date == DateTime.Today).ToList();

                SensorDataList.Clear();
                foreach (var item in todayData)
                {
                    SensorDataList.Add(item);
                }

                CreateChart(todayData);
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately (log, notify user, etc.)
                Debug.WriteLine($"Error loading sensor data: {ex.Message}");
            }
        }

        private void CreateChart(IEnumerable<SensorData> data)
        {
            if (data == null || !data.Any())
            {
                // Handle the case where there is no data to create a chart
                Chart = null;
                return;
            }

            var hourlyData = data.GroupBy(d => d.DateTime.Hour)
                                 .Select(g => new
                                 {
                                     Hour = g.Key,
                                     MaxMagnitude = g.Max(d => d.Magnitude)
                                 })
                                 .OrderBy(d => d.Hour)
                                 .ToList();

            var entries = hourlyData.Select(d => new ChartEntry((float)d.MaxMagnitude)
            {
                Label = $"{d.Hour}:00",
                ValueLabel = d.MaxMagnitude.ToString("0.00"),
                Color = SKColor.Parse("#2c3e50")
            }).ToArray();

            Chart = new LineChart
            {
                Entries = entries,
                LineMode = LineMode.Straight,
                LineSize = 4,
                PointMode = PointMode.Circle,
                PointSize = 8,
                MinValue = 0,
                MaxValue = entries.Any() ? Math.Max(entries.Max(e => e.Value), 1) : 1, // Ensure a reasonable max value
                ValueLabelOrientation = Orientation.Horizontal,
                LabelTextSize = 30
            };
        }
    }
}
