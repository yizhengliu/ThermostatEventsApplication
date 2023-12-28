using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermostatEventsApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HeatSensor heatSebsir = new HeatSensor(1, 2);
            heatSebsir.RunHeatSensor();
        }

    }

    public interface IDevice 
    {
        void RunDevice();
        void HandleEmergency();
    }

    public class CoolingMechanism : ICoolingMechanism
    {
        public void Off()
        {
            Console.WriteLine();
            Console.WriteLine("Switching cooling mechanism off...");
            Console.WriteLine();
        }

        public void On()
        {
            Console.WriteLine();
            Console.WriteLine("Switching cooling mechanism on...");
            Console.WriteLine();
        }
    }

    public interface ICoolingMechanism 
    {
        void On();
        void Off();
    }
    public class HeatSensor : IHeatSenor
    {
        double _warningLevel = 0;
        double _emergencyLevel = 0;

        bool _hasReachedWarningTemperature = false;

        protected EventHandlerList _listEventDelegates = new EventHandlerList();

        private double[] _temperatureData = null;

        //key values for accessing delegates in the list
        static readonly object _temperatureReachesEmergencyLevelKey = new object();
        static readonly object _temperatureReachesWarningLevelKey = new object();
        static readonly object _temperatureFallsBelowWarningLevelKey = new object();


        public HeatSensor(double warningLevel, double emergencyLevel) 
        {
            _warningLevel = warningLevel;
            _emergencyLevel = emergencyLevel;

            SeedData();
        }

        private void SeedData() 
        {
            _temperatureData = new double[] { 16, 17, 16.5, 18, 19, 22, 24, 26.75, 28.7, 27.6, 26, 24, 22, 45, 68, 86.45 };
        }

        private void MonitorTemperature() 
        {
            foreach (double temperature in _temperatureData) 
            {
                Console.ResetColor();
                Console.WriteLine($"DateTime: {DateTime.Now}, Temperature {temperature}");

                if (temperature >= _emergencyLevel)
                {
                    TemperatureEventArgs e = new TemperatureEventArgs
                    {
                        Temperature = temperature,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureReachesEmergencyLevel(e);
                }
                else if (temperature >= _warningLevel)
                {
                    _hasReachedWarningTemperature = true;
                    TemperatureEventArgs e = new TemperatureEventArgs
                    {
                        Temperature = temperature,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureReachesWarningLevel(e);
                }
                else if (temperature < _warningLevel && _hasReachedWarningTemperature) 
                {
                    _hasReachedWarningTemperature = false;
                    TemperatureEventArgs e = new TemperatureEventArgs
                    {
                        Temperature = temperature,
                        CurrentDateTime = DateTime.Now
                    };
                    OnTemperatureFallsBelowWarningLevel(e);
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        event EventHandler<TemperatureEventArgs> IHeatSenor.TemperatureReachesEmergencyLevelEventHandler
        {
            //fired when subscribe
            add
            {
                _listEventDelegates.AddHandler(_temperatureReachesEmergencyLevelKey, value);
            }
            //fired when unsubscribe
            remove
            {
                _listEventDelegates.RemoveHandler(_temperatureReachesEmergencyLevelKey, value);
            }
        }

        event EventHandler<TemperatureEventArgs> IHeatSenor.TemperatureReachesWarningLevelEventHandler
        {
            add
            {
                _listEventDelegates.AddHandler(_temperatureReachesWarningLevelKey, value);
            }

            remove
            {
                _listEventDelegates.RemoveHandler(_temperatureReachesWarningLevelKey, value);
            }
        }

        event EventHandler<TemperatureEventArgs> IHeatSenor.TemperatureFallsBelowWarningLevelEventHandler
        {
            add
            {
                _listEventDelegates.AddHandler(_temperatureFallsBelowWarningLevelKey, value);
            }

            remove
            {
                _listEventDelegates.AddHandler(_temperatureFallsBelowWarningLevelKey, value);
            }
        }

        protected void OnTemperatureReachesWarningLevel(TemperatureEventArgs e) 
        {
            EventHandler<TemperatureEventArgs> handler = (EventHandler<TemperatureEventArgs>)_listEventDelegates[_temperatureReachesWarningLevelKey];

            if (handler != null) 
            {
                handler(this, e);
            }
        }

        protected void OnTemperatureReachesEmergencyLevel(TemperatureEventArgs e)
        {
            EventHandler<TemperatureEventArgs> handler = (EventHandler<TemperatureEventArgs>)_listEventDelegates[_temperatureReachesEmergencyLevelKey];

            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnTemperatureFallsBelowWarningLevel(TemperatureEventArgs e)
        {
            EventHandler<TemperatureEventArgs> handler = (EventHandler<TemperatureEventArgs>)_listEventDelegates[_temperatureFallsBelowWarningLevelKey];

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RunHeatSensor()
        {
            Console.WriteLine("Head sensor is running...");
            MonitorTemperature();
        }
    }

    public interface IHeatSenor 
    {
        event EventHandler<TemperatureEventArgs> TemperatureReachesEmergencyLevelEventHandler;
        event EventHandler<TemperatureEventArgs> TemperatureReachesWarningLevelEventHandler;
        event EventHandler<TemperatureEventArgs> TemperatureFallsBelowWarningLevelEventHandler;

        void RunHeatSensor();
    }

    public class TemperatureEventArgs : EventArgs 
    {
        public double Temperature {  get; set; }
        public DateTime CurrentDateTime { get; set; }

    }
}
