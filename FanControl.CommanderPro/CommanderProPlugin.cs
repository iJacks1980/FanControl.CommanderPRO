using FanControl.Plugins;
using System;
using System.Collections.Generic;

namespace FanControl.CommanderPro
{
    public class CommanderProPlugin : IPlugin2
    {
        #region Private objects

        private const String ErrorLogFileName = "CommanderCORE.err.log";

#if DEBUG
        private const String TraceLogFileName = "CommanderCORE.trc.log";
#else
        private const String TraceLogFileName = "";
#endif

        private Pro.CommanderPro CommanderPro;

        #endregion

        public String Name => "Corsair Commander PRO";

        public void Close()
        {
            if (CommanderPro != null)
            {
                CommanderPro.Disconnect();
            }
        }

        public void Initialize()
        {
            CommanderPro = new Pro.CommanderPro();

            CommanderPro.Connect();
        }

        public void Load(IPluginSensorsContainer _container)
        {
            if (CommanderPro != null)
            {
                List<FanSensor> _fanSensors = new List<FanSensor>();
                List<TemperatureSensor> _temperatureSensor = new List<TemperatureSensor>();
                List<ControlSensor> _controlSensors = new List<ControlSensor>();

                foreach (Int32 channel in CommanderPro.GetFanChannels())
                {
                    _fanSensors.Add(new FanSensor { CommanderInstance = CommanderPro, Channel = channel });
                    _controlSensors.Add(new ControlSensor { CommanderInstance = CommanderPro, Channel = channel });
                }

                foreach (Int32 channel in CommanderPro.GetTemperatureChannels())
                {
                    _temperatureSensor.Add(new TemperatureSensor { CommanderInstance = CommanderPro, Channel = channel });
                }

                _container.FanSensors.AddRange(_fanSensors);
                _container.TempSensors.AddRange(_temperatureSensor);
                _container.ControlSensors.AddRange(_controlSensors);
            }
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