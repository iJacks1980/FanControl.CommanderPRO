using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro
{
    public class DeviceManager : ICommander
    {
        #region Private objects

        private readonly Object devicesLock = new Object();

        private Dictionary<Int32, Device> devices;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Pro;

        #endregion

        #region Constructor

        public DeviceManager()
        {
            devices = new Dictionary<Int32, Device>();

            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager created");
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.Connect()");

            if (AreAllDevicesConnected()) return;

            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Looking for Commander PRO devices");

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
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }
        }

        public void Disconnect()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.Disconnect()");

            try
            {
                if (devices != null)
                {
                    foreach (Device device in devices.Values)
                    {
                        device.Disconnect();
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }
        }

        public List<Int32> GetFanChannels()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetFanChannels()");

            List<Int32> result = new List<Int32>();

            try
            {
                if (devices != null)
                {
                    foreach (Device device in devices.Values)
                    {
                        result.AddRange(device.FanChannels);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }

            return result;
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetFanSpeed()");

            Int32 result = 0;

            try
            {
                if (devices != null)
                {
                    result = devices.First(x => x.Value.FanChannels.Contains(channel)).Value.GetFanSpeed(channel);
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }

            return result;
        }

        public Int32 GetFanPower(Int32 channel)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetFanPower()");

            Int32 result = 0;

            try
            {
                if (devices != null)
                {
                    result = devices.First(x => x.Value.FanChannels.Contains(channel)).Value.GetFanPower(channel);
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }

            return result;
        }

        public void SetFanSpeed(Int32 channel, Int32 speed)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.SetFanSpeed()");

            try
            {
                if (devices != null)
                {
                    devices.First(x => x.Value.FanChannels.Contains(channel)).Value.SetFanSpeed(channel, speed);
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }
        }

        public void SetFanPower(Int32 channel, Int32 power)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.SetFanPower()");

            try
            {
                if (devices != null)
                {
                    devices.First(x => x.Value.FanChannels.Contains(channel)).Value.SetFanPower(channel, power);
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }
        }

        public List<Int32> GetTemperatureChannels()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetTemperatureChannels()");

            List<Int32> result = new List<Int32>();

            try
            {
                if (devices != null)
                {
                    foreach (Device device in devices.Values)
                    {
                        result.AddRange(device.TemperatureChannels);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }

            return result;
        }

        public Single GetTemperature(Int32 channel)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetTemperature()");

            Single result = 0;

            try
            {
                if (devices != null)
                {
                    result = devices.First(x => x.Value.TemperatureChannels.Contains(channel)).Value.GetTemperature(channel);
                }
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            }

            return result;
        }

        #endregion

        #region Private methods

        private Boolean AreAllDevicesConnected()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.AreAllDevicesConnected()");

            Boolean result = false;

            lock (devicesLock)
            {
                try
                {
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
                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
                }
            }

            return result;
        }

        private void GetDevices()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.GetDevices()");

            lock (devicesLock)
            {
                try
                {
                    if (!devices.Any())
                    {
                        IEnumerable<HidSharp.HidDevice> hidDevices = HidSharp.DeviceList.Local.GetHidDevices(Constants.VENDOR_ID, Constants.PRODUCT_ID);

                        if (hidDevices != null)
                        {
                            //Order found devices by serial number - so if there are multiple devices they should be found in the same order.
                            foreach (HidSharp.HidDevice hidDevice in hidDevices.OrderBy(x => x.GetSerialNumber()))
                            {
                                Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Found Commander PRO device with S/N: {hidDevice.GetSerialNumber()}");

                                devices.Add(devices.Count + 1, new Device(devices.Count + 1, hidDevice));
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
                }
            }
        }

        #endregion
    }
}