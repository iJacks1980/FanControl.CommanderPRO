using FanControl.Plugins;
using System;

namespace FanControl.CommanderPro
{
    public class FanSensor : IPluginSensor
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
                        result = $"Commander PRO Channel {Channel + 1}";

                        break;
                    case DeviceType.Core:
                        if (Channel == 0)
                        {
                            result = "Commander CORE AIO Pump";
                        }
                        else
                        {
                            result = $"Commander CORE Channel {Channel}";
                        }

                        break;
                    case DeviceType.Core_Xt:
                        result = $"Commander CORE XT Channel {Channel}";

                        break;
                }

                return result;
            }
        }

        public Single? Value { get; set; }

        public void Update()
        {
            CommanderInstance.Connect();

            Value = CommanderInstance.GetFanSpeed(Channel);
        }
    }
}