using FanControl.Plugins;
using System;

namespace FanControl.Commander.Common
{
    public class TemperatureSensor : IPluginSensor
    {
        public ICommander CommanderInstance { get; set; }

        public Int32 Channel { get; set; }

        public String Id => Channel.ToString();

        public String Name
        {
            get
            {
                String result = null;

                switch (CommanderInstance.Type)
                {
                    case DeviceType.Pro:
                        result = $"Sensor {Channel + 1}";

                        break;
                    case DeviceType.Core:
                        if (Channel == 0)
                        {
                            result = $"AIO Liquid Temperature";
                        }
                        else
                        {
                            result = $"Sensor {Channel}";
                        }

                        break;
                    case DeviceType.Core_Xt:
                        result = $"Sensor {Channel}";

                        break;
                }

                return result;
            }
        }

        public Single? Value { get; set; }

        public void Update()
        {
            CommanderInstance.Connect();

            Value = CommanderInstance.GetTemperature(Channel);
        }
    }
}