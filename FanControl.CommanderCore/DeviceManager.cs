using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderCore
{
    public class DeviceManager : ICommander
    {
        #region Private objects

        private Dictionary<Int32, Device> devices;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core;

        #endregion

        #region Constructor

        public DeviceManager()
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

                devices = new Dictionary<Int32, Device>();
            }
            catch (Exception exception)
            {

            }
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            if (AreAllDevicesConnected()) return;

            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Looking for Commander CORE devices" + Environment.NewLine);
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

            return result;
        }

        public void SetFanSpeed(Int32 channel, Int32 speed)
        {

        }

        public void SetFanPower(Int32 channel, Int32 power)
        {

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
                    if (hidDevice.ProductID == Constants.PRODUCT_ID && hidDevice.GetMaxInputReportLength() > 0 && hidDevice.GetMaxOutputReportLength() > 0)
                    {
                        if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Found Commander CORE device with S/N: {hidDevice.GetSerialNumber()}" + Environment.NewLine);
                        }

                        devices.Add(devices.Count + 1, new Device(devices.Count + 1, hidDevice));
                    }
                    else
                    {

                    }
                }
            }
        }

        #endregion
    }
}