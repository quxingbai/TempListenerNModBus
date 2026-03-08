using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TempListenerNModBus.Controls;
using TempListenerNModBus.Helpers;

namespace TempListenerNModBus
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl
    {
        private class MainPageViewModel : IDisposable, INotifyPropertyChanged
        {
            public class LogData
            {
                public double Temp { get; set; }
                public double Humid { get; set; }
                public double Sun { get; set; }

                public bool IsHeating { get; set; }
                public bool IsHumid { get; set; }
                public bool IsLighting { get; set; }
                public DateTime Date { get; set; } = DateTime.Now;


                public bool IsHightTemp => Temp > 30;
            }
            public bool _HumidState { get; set; }
            public bool _HeatingState { get; set; }
            public bool _LightState { get; set; }
            public bool HumidState
            {
                get => _HumidState; set
                {
                    _HumidState = value;
                    PropertyChanged?.Invoke(this, new("HumidState"));
                }
            }
            public bool HeatingState
            {
                get => _HeatingState; set
                {
                    _HeatingState = value;
                    PropertyChanged?.Invoke(this, new("HeatingState"));
                }
            }
            public bool LightState
            {
                get => _LightState; set
                {
                    _LightState = value;
                    PropertyChanged?.Invoke(this, new("LightState"));
                }
            }
            public double Temp { get; set; }
            public double Humid { get; set; }
            public double Sun { get; set; }



            public ObservableCollection<LogData> Logs { get; set; } = new();


            private bool IsRunning = false;

            public event PropertyChangedEventHandler? PropertyChanged;

            public MainPageViewModel()
            {
                IsRunning = true;
                CreateListenTask();
            }

            private void WriteSunLightLog(double Val)
            {

            }
            private void WriteAllLog()
            {
                //ModbusDireivers.ControllerMaster.GetAllState(out var hea, out var humid, out var light);
                //ModbusDireivers.TempMaster.GetAllState(out var tempVal, out var humidVal);

                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (Logs.Count > 20)
                    {
                        Logs.RemoveAt(0);
                    }
                    var logdata = new LogData()
                    {
                        Date = DateTime.Now,
                        Humid = Humid,
                        IsHeating = HeatingState,
                        IsHumid = HumidState,
                        IsLighting = LightState,
                        Temp = Temp,
                        Sun = Sun
                    };
                    Logs.Add(logdata);
                    
                });

            }
            private void SetHeatingState(bool Stat)
            {
                var nowstate = ModbusDireivers.ControllerMaster.GetHeatingState();
                App.Current.Dispatcher.Invoke(() =>
                {
                    HeatingState = nowstate;
                });
                if (HeatingState == Stat) return;
                ModbusDireivers.ControllerMaster.SetHeatingState(Stat);
                App.Current.Dispatcher.Invoke(() =>
                {
                    HeatingState = Stat;
                });
                Debug.WriteLine("加热开关->" + Stat);
            }
            private void SetHumidState(bool Stat)
            {
                var nowstate = ModbusDireivers.ControllerMaster.GetHumidState();
                App.Current.Dispatcher.Invoke(() =>
                {
                    HumidState = nowstate;
                });
                if (HumidState == Stat) return;
                ModbusDireivers.ControllerMaster.SetHumidState(Stat);
                App.Current.Dispatcher.Invoke(() =>
                {
                    HumidState = Stat;
                });
                Debug.WriteLine("加湿开关->" + Stat);
            }

            private void SetLightState(bool Stat)
            {
                var nowstate = ModbusDireivers.ControllerMaster.GetLightState();
                App.Current.Dispatcher.Invoke(() =>
                {
                    LightState = nowstate;
                });
                if (LightState == Stat) return;
                ModbusDireivers.ControllerMaster.SetLightState(Stat);
                App.Current.Dispatcher.Invoke(() =>
                {
                    LightState = Stat;
                });
                Debug.WriteLine("警报灯开关->" + Stat);
            }

            private void SetDisplayTempHumid(double Temp, double Humid, double Sun)
            {
                this.Temp = Temp;
                this.Humid = Humid;
                this.Sun = Sun;
                App.Current.Dispatcher.BeginInvoke(() =>
                {
                    PropertyChanged?.Invoke(this, new("Temp"));
                    PropertyChanged?.Invoke(this, new("Humid"));
                    PropertyChanged?.Invoke(this, new("Sun"));
                });
            }


            private Task CreateListenTask()
            {
                return Task.Run(() =>
                {
                    while (IsRunning)
                    {
                        {
                            ModbusDireivers.ControllerMaster.GetAllState(out var heat, out var ishumid, out var islight);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                this.HeatingState = heat;
                                this.HumidState = ishumid;
                                this.LightState = islight;
                            });
                        }
                        double temp = ModbusDireivers.TempMaster.GetTemp();

                        if (temp < 20)
                        {
                            SetHeatingState(true);
                        }
                        else if (temp > 30)
                        {
                            SetHeatingState(false);
                            SetLightState(true);
                        }
                         if(temp < 30)
                        {
                            SetLightState(false);
                        }


                        double hmd = ModbusDireivers.TempMaster.GetHumidity();

                        if (hmd < 40)
                        {
                            SetHumidState(true);
                        }
                        else if (hmd > 50)
                        {
                            SetHumidState(false);
                        }

                        double sunLight = ModbusDireivers.SunTempMaster.GetSunLightVal();
                        if (sunLight > 800)
                        {
                            WriteSunLightLog(sunLight);
                        }

                        SetDisplayTempHumid(temp, hmd, sunLight);

                        WriteAllLog();
                        Thread.Sleep(500);
                    }
                });
            }

            public void Dispose()
            {
                IsRunning = false;
            }
        }
        public MainPage()
        {
            InitializeComponent();
            ModbusDireivers.InitMaster().ContinueWith(w =>
            {
                Dispatcher.Invoke(() =>
                {
                    this.DataContext = new MainPageViewModel();
                });
            });
        }
    }
}
