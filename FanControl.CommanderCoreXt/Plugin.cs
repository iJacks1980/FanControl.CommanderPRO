using FanControl.Commander.Common;
using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderCoreXt
{
    public class Plugin : IPlugin2
    {
        #region Private objects

        private DeviceManager CommanderCore;

        #endregion

        #region Public objects

        public String Name => "Corsair Commander CORE XT";

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
            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Plugin closing" + Environment.NewLine);
            }

            if (CommanderCore != null)
            {
                CommanderCore.Disconnect();
            }
        }

        public void Initialize()
        {
            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Plugin initializing" + Environment.NewLine);
            }

            CommanderCore = new DeviceManager();

            CommanderCore.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Plugin loading" + Environment.NewLine);
            }

            List<FanSensor> _fanSensors = new List<FanSensor>();
            List<ControlSensor> _controlSensors = new List<ControlSensor>();
            List<TemperatureSensor> _temperatureSensors = new List<TemperatureSensor>();

            foreach (Int32 channel in CommanderCore.GetFanChannels())
            {
                _fanSensors.Add(new FanSensor { CommanderInstance = CommanderCore, Channel = channel });
                //_controlSensors.Add(new ControlSensor { CommanderInstance = CommanderCore, Channel = channel });
            }

            foreach (Int32 channel in CommanderCore.GetTemperatureChannels())
            {
                _temperatureSensors.Add(new TemperatureSensor { CommanderInstance = CommanderCore, Channel = channel });
            }

            if (_fanSensors.Any()) _container.FanSensors.AddRange(_fanSensors);
            if (_controlSensors.Any()) _container.ControlSensors.AddRange(_controlSensors);
            if (_temperatureSensors.Any()) _container.TempSensors.AddRange(_temperatureSensors);
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