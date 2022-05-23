using FanControl.Commander.Common;
using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class Plugin : IPlugin2
    {
        #region Private objects

        private DeviceManager DeviceManager;

        #endregion

        #region Public objects

        public String Name => "Corsair Commander PRO";

        #endregion

        #region Constructor

        public Plugin()
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME) && System.IO.File.Exists(Constants.TRACE_LOG_FILE_NAME))
                {
                    System.IO.File.Delete(Constants.TRACE_LOG_FILE_NAME);
                }

                if (!String.IsNullOrWhiteSpace(Constants.ERROR_LOG_FILE_NAME) && System.IO.File.Exists(Constants.ERROR_LOG_FILE_NAME))
                {
                    System.IO.File.Delete(Constants.ERROR_LOG_FILE_NAME);
                }
            }
            catch (Exception exception)
            {

            }
        }

        #endregion

        #region Public methods

        public void Close()
        {
            if (DeviceManager != null)
            {
                DeviceManager.Disconnect();
            }
        }

        public void Initialize()
        {
            DeviceManager = new DeviceManager();

            DeviceManager.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            if (DeviceManager != null)
            {
                List<FanSensor> _fanSensors = new List<FanSensor>();
                List<TemperatureSensor> _temperatureSensor = new List<TemperatureSensor>();
                List<ControlSensor> _controlSensors = new List<ControlSensor>();

                foreach (Int32 channel in DeviceManager.GetFanChannels())
                {
                    _fanSensors.Add(new FanSensor { CommanderInstance = DeviceManager, Channel = channel });
                    _controlSensors.Add(new ControlSensor { CommanderInstance = DeviceManager, Channel = channel });
                }

                foreach (Int32 channel in DeviceManager.GetTemperatureChannels())
                {
                    _temperatureSensor.Add(new TemperatureSensor { CommanderInstance = DeviceManager, Channel = channel });
                }

                _container.FanSensors.AddRange(_fanSensors);
                _container.TempSensors.AddRange(_temperatureSensor);
                _container.ControlSensors.AddRange(_controlSensors);
            }
        }

        public void Update()
        {
            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Plugin Update method called" + Environment.NewLine);
            }
        }

        #endregion
    }
}