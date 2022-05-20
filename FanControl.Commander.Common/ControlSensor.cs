using FanControl.Plugins;
using System;

namespace FanControl.Commander.Common
{
    public class ControlSensor : IPluginControlSensor
    {
        public ICommander CommanderInstance { get; set; }

        public Int32 Channel { get; set; }

        public FanSpeedType FanSpeedType { get; set; } = FanSpeedType.Pwm;

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
                        result = $"Commander CORE Channel {Channel}";

                        break;
                    case DeviceType.Core_Xt:
                        result = $"Commander CORE XT Channel {Channel}";

                        break;
                }

                return result;
            }
        }

        public Single? Value { get; set; }

        public void Reset()
        {
            CommanderInstance.Connect();

            switch (FanSpeedType)
            {
                case FanSpeedType.Pwm:
                    CommanderInstance.SetFanPower(Channel, 50);

                    break;
                case FanSpeedType.Dc:
                    CommanderInstance.SetFanSpeed(Channel, 7500);

                    break;
            }
        }

        public void Set(Single val)
        {
            CommanderInstance.Connect();

            switch (FanSpeedType)
            {
                case FanSpeedType.Pwm:
                    CommanderInstance.SetFanPower(Channel, Convert.ToInt32(val));

                    break;
                case FanSpeedType.Dc:
                    CommanderInstance.SetFanSpeed(Channel, Convert.ToInt32(val));

                    break;
            }
        }

        public void Update()
        {
            CommanderInstance.Connect();

            Value = CommanderInstance.GetFanPower(Channel);
        }
    }
}