using FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro
{
    public class CommanderCorePlugin : IPlugin2
    {
        #region Private objects

        private const String ErrorLogFileName = "CommanderCORE.err.log";

#if DEBUG
        private const String TraceLogFileName = "CommanderCORE.trc.log";
#else
        private const String TraceLogFileName = "";
#endif

        private Core.CommanderCore CommanderCore;

        #endregion

        #region Public objects

        public String Name => "Corsair Commander CORE";

        #endregion
        
        public void Close()
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Plugin closing" + Environment.NewLine);
            }

            if (CommanderCore != null)
            {
                CommanderCore.Disconnect();
            }
        }

        public void Initialize()
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Plugin initializing" + Environment.NewLine);
            }

            CommanderCore = new Core.CommanderCore();

            CommanderCore.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Plugin loading" + Environment.NewLine);
            }

            List<FanSensor> _fanSensors = new List<FanSensor>();
            List<ControlSensor> _controlSensors = new List<ControlSensor>();
            List<TemperatureSensor> _temperatureSensors = new List<TemperatureSensor>();

            CommanderCore.GetFirmwareVersion();

            foreach (Int32 channel in CommanderCore.GetFanChannels())
            {
                _fanSensors.Add(new FanSensor { CommanderInstance = CommanderCore, Channel = channel });

                //Don't allow the AIO pump to be controlled
                if (channel == 0) continue;

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
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Plugin Update method called" + Environment.NewLine);
            }
        }
    }
}