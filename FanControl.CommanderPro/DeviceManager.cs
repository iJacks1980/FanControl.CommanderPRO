using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro
{
    public class DeviceManager : ICommander
    {
        #region Private objects

        private Dictionary<Int32, Device> devices;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Pro;

        #endregion

        #region Constructor

        public DeviceManager()
        {
            devices = new Dictionary<Int32, Device>();
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            if (AreAllDevicesConnected()) return;

            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Looking for Commander PRO devices" + Environment.NewLine);
            }

            try
            {
                GetDevices();

                if (devices != null)
                {
                    foreach (Device device in devices.Values)
                    {
                        device.Connect();
                    }
                }
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
            }
        }

        public void Disconnect()
        {
            if (devices != null)
            {
                foreach (Device device in devices.Values)
                {
                    device.Disconnect();
                }
            }
        }

        public List<Int32> GetFanChannels()
        {
            List<Int32> result = new List<Int32>();

            if (devices != null)
            {
                foreach (Device device in devices.Values)
                {
                    result.AddRange(device.FanChannels);
                }
            }

            return result;
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            Int32 result = 0;

            if (devices != null)
            {
                result = devices.First(x => x.Value.FanChannels.Contains(channel)).Value.GetFanSpeed(channel);
            }

            return result;
        }

        public Int32 GetFanPower(Int32 channel)
        {
            Int32 result = 0;

            if (devices != null)
            {
                result = devices.First(x => x.Value.FanChannels.Contains(channel)).Value.GetFanPower(channel);
            }

            return result;
        }

        public void SetFanSpeed(Int32 channel, Int32 speed)
        {
            if (devices != null)
            {
                devices.First(x => x.Value.FanChannels.Contains(channel)).Value.SetFanSpeed(channel, speed);
            }
        }

        public void SetFanPower(Int32 channel, Int32 power)
        {
            if (devices != null)
            {
                devices.First(x => x.Value.FanChannels.Contains(channel)).Value.SetFanPower(channel, power);
            }
        }

        public List<Int32> GetTemperatureChannels()
        {
            List<Int32> result = new List<Int32>();

            if (devices != null)
            {
                foreach (Device device in devices.Values)
                {
                    result.AddRange(device.TemperatureChannels);
                }
            }

            return result;
        }

        public Single GetTemperature(Int32 channel)
        {
            Single result = 0;

            if (devices != null)
            {
                result = devices.First(x => x.Value.TemperatureChannels.Contains(channel)).Value.GetTemperature(channel);
            }

            return result;
        }

        #endregion

        #region Private methods

        private Boolean AreAllDevicesConnected()
        {
            Boolean result = false;

            if (devices != null)
            {
                if (!devices.Any())
                {
                    result = false;
                }
                else
                {
                    result = devices.All(x => x.Value.IsConnected);
                }
            }

            return result;
        }

        private void GetDevices()
        {
            IEnumerable<HidSharp.HidDevice> hidDevices = HidSharp.DeviceList.Local.GetHidDevices(Constants.VENDOR_ID, Constants.PRODUCT_ID);

            if (hidDevices != null)
            {
                //Order found devices by serial number - so if there are multiple devices they should be found in the same order.
                foreach (HidSharp.HidDevice hidDevice in hidDevices.OrderBy(x => x.GetSerialNumber()))
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Found Commander PRO device with S/N: {hidDevice.GetSerialNumber()}" + Environment.NewLine);
                    }

                    devices.Add(devices.Count + 1, new Device(devices.Count + 1, hidDevice));
                }
            }
        }

        #endregion
    }
}