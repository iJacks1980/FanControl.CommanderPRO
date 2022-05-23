using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderCore
{
    public class DeviceManager : ICommander
    {
        #region Private objects

        private readonly Object devicesLock = new Object();

        private Dictionary<Int32, Device> devices;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core;

        #endregion

        #region Constructor

        public DeviceManager()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager created");

            devices = new Dictionary<Int32, Device>();
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "DeviceManager.Connect()");

            if (AreAllDevicesConnected()) return;

            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Looking for Commander CORE devices");

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

        //public void SetFanPower(Int32 channel, Int32 power)
        //{
        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected)
        //    {
        //        try
        //        {
        //            SendCommand(Constants.COMMAND_RESET);
        //            //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_SPEED_MODE);
        //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
        //            Byte[] response = SendCommand(Constants.COMMAND_READ);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_HW_FIXED_PERCENT))
        //            {

        //            }

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_HW_SPEED_MODE))
        //            {
        //                Byte[] data = new Byte[response[6] + 1];

        //                for (Int32 i = 0; i < data.Length; i++)
        //                {
        //                    data[i] = response[7 + i];
        //                }

        //                SendData(Constants.MODE_HW_SPEED_MODE, Constants.DATA_TYPE_HW_SPEED_MODE, data);

        //                SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
        //                response = SendCommand(Constants.COMMAND_READ);

        //                if (ChecksumMatches(response, Constants.DATA_TYPE_HW_FIXED_PERCENT))
        //                {
        //                    data = new Byte[response[6] * 2 + 1];

        //                    for (Int32 i = 0; i < data.Length; i++)
        //                    {
        //                        data[i] = response[7 + i];
        //                    }

        //                    SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

        //                    Byte[] powerData = BitConverter.GetBytes(power);

        //                    data[channel * 2 + 1] = powerData[0];
        //                    data[channel * 2 + 2] = powerData[1];

        //                    SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

        //                    //duty_le = int.to_bytes(clamp(duty, 0, 100), length = 2, byteorder = "little", signed = False)
        //                    //for chan in channels:
        //                    //    i = chan * 2 + 1
        //                    //    data[i: i + 2] = duty_le  # Update the device speed
        //                    //self._write_data(_MODE_HW_FIXED_PERCENT, _DATA_TYPE_HW_FIXED_PERCENT, data)
        //                }
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }
        //}

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
                                if (hidDevice.ProductID == Constants.PRODUCT_ID && hidDevice.GetMaxInputReportLength() > 0 && hidDevice.GetMaxOutputReportLength() > 0)
                                {
                                    Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Found Commander CORE device with S/N: {hidDevice.GetSerialNumber()}");

                                    devices.Add(devices.Count + 1, new Device(devices.Count + 1, hidDevice));
                                }
                                else
                                {

                                }
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