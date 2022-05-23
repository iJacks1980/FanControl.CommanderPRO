﻿using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderCoreXt
{
    public class DeviceManager : ICommander
    {
        #region Private objects

        private Dictionary<Int32, Device> devices;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core_Xt;

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
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Looking for Commander CORE XT devices" + Environment.NewLine);
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
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"{DateTime.UtcNow:R} Found Commander CORE XT device with S/N: {hidDevice.GetSerialNumber()}" + Environment.NewLine);
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

        //#region Public methods

        //public void Connect()
        //{
        //    if (IsConnected) return;

        //    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //    {
        //        System.IO.File.AppendAllText(TraceLogFileName, "Looking for a Commander CORE family device" + Environment.NewLine);
        //    }

        //    IsConnected = false;

        //    try
        //    {
        //        FirmwareVersion = "0.0.0";
        //        FanChannels.Clear();
        //        TemperatureChannels.Clear();

        //        ConnectToDevice();

        //        if (device != null)
        //        {
        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                System.IO.File.AppendAllText(TraceLogFileName, $"Found device: {device.GetProductName()}" + Environment.NewLine);
        //            }

        //            HidSharp.OpenConfiguration openConfiguration = new HidSharp.OpenConfiguration();

        //            openConfiguration.SetOption(HidSharp.OpenOption.Exclusive, true);
        //            //openConfiguration.SetOption(HidSharp.OpenOption.Priority, HidSharp.OpenPriority.VeryHigh);
        //            openConfiguration.SetOption(HidSharp.OpenOption.Transient, true);
        //            openConfiguration.SetOption(HidSharp.OpenOption.Interruptible, true);

        //            var options = openConfiguration.GetOptionsList();

        //            if (device.TryOpen(openConfiguration, out stream))
        //            {
        //                IsConnected = true;

        //                SendCommand(Constants.COMMAND_WAKE);
        //                //SendCommand(Constants.COMMAND_RESET);
        //                //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);

        //                if (IsConnected)
        //                {
        //                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                    {
        //                        System.IO.File.AppendAllText(TraceLogFileName, $"Connected to device: {device.GetProductName()}" + Environment.NewLine);
        //                    }

        //                    GetFirmwareVersion();
        //                }
        //            }
        //            else
        //            {
        //                IsConnected = false;
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

        //        IsConnected = false;
        //    }
        //}

        //public void Disconnect()
        //{
        //    if (!IsConnected) return;

        //    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //    {
        //        System.IO.File.AppendAllText(TraceLogFileName, "Disconnecting from Commander CORE" + Environment.NewLine);
        //    }

        //    SendCommand(Constants.COMMAND_RESET, null, false);
        //    SendCommand(Constants.COMMAND_SLEEP, null, false);

        //    stream.Dispose();
        //    stream = null;

        //    if (device != null)
        //    {
        //        device = null;
        //    }

        //    IsConnected = false;
        //}

        //public String GetFirmwareVersion()
        //{
        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected && String.Equals(FirmwareVersion, "0.0.0", StringComparison.InvariantCultureIgnoreCase))
        //    {
        //        if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //        {
        //            System.IO.File.AppendAllText(TraceLogFileName, "Attempting to get Commander CORE firmware version" + Environment.NewLine);
        //        }

        //        try
        //        {
        //            //SendCommand(Constants.COMMAND_WAKE);
        //            //SendCommand(Constants.COMMAND_RESET);
        //            Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
        //            {
        //                FirmwareVersion = $"{response[4]}.{response[5]}.{response[6]}";

        //                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                {
        //                    System.IO.File.AppendAllText(TraceLogFileName, $"Commander CORE Firmware v{FirmwareVersion}" + Environment.NewLine);
        //                }
        //            }
        //            else
        //            {
        //                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                {
        //                    System.IO.File.AppendAllText(TraceLogFileName, $"Bad firmware version v{FirmwareVersion}" + Environment.NewLine);
        //                }

        //                Disconnect();
        //            }

        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

        //            IsConnected = false;
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }

        //    return FirmwareVersion;
        //}

        //public List<Int32> GetFanChannels()
        //{
        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected && !FanChannels.Any())
        //    {
        //        try
        //        {
        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                System.IO.File.AppendAllText(TraceLogFileName, "Getting fan channels" + Environment.NewLine);
        //            }

        //            //SendCommand(Constants.COMMAND_WAKE);
        //            //SendCommand(Constants.COMMAND_RESET);
        //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);
        //            Byte[] response = SendCommand(Constants.COMMAND_READ);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_SW_CONNECTED))
        //            {
        //                if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                {
        //                    System.IO.File.AppendAllText(TraceLogFileName, $"\tFan channel data {BitConverter.ToString(response)}" + Environment.NewLine);
        //                }

        //                Int32 totalDevices = response[6];

        //                for (Int32 i = 0; i < totalDevices; i++)
        //                {
        //                    //0 = AIO Pump, not a fan so ignore
        //                    if (response[i + 7] == 0x07)
        //                    {
        //                        FanChannels.Add(i);
        //                    }
        //                }
        //            }

        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                foreach (Int32 channel in FanChannels)
        //                {
        //                    System.IO.File.AppendAllText(TraceLogFileName, $"\tFound fan on channel {channel}" + Environment.NewLine);
        //                }
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }

        //    return FanChannels;
        //}

        //public Int32 GetFanSpeed(Int32 channel)
        //{
        //    Int32 result = 0;

        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected)
        //    {
        //        try
        //        {
        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                System.IO.File.AppendAllText(TraceLogFileName, $"Getting fan channel {channel} speed" + Environment.NewLine);
        //            }

        //            //SendCommand(Constants.COMMAND_WAKE);
        //            SendCommand(Constants.COMMAND_RESET);
        //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_SPEEDS);
        //            Byte[] response = SendCommand(Constants.COMMAND_READ);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_SPEEDS))
        //            {
        //                Int32 totalResults = response[6];

        //                for (Int32 i = 0; i < totalResults * 2; i++)
        //                {
        //                    if (i != channel) continue;

        //                    Int32 offset = 7 + i * 2;

        //                    result = BitConverter.ToUInt16(response, offset);

        //                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                    {
        //                        System.IO.File.AppendAllText(TraceLogFileName, $"\tFan channel {channel} speed: {result}" + Environment.NewLine);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }

        //    return result;
        //}

        //public Int32 GetFanPower(Int32 channel)
        //{
        //    Int32 result = 0;

        //    return result;
        //}

        //public void SetFanPower(Int32 channel, Int32 power)
        //{

        //}

        ////public void SetFanPower(Int32 channel, Int32 power)
        ////{
        ////    if (!IsConnected)
        ////    {
        ////        Connect();
        ////    }

        ////    if (IsConnected)
        ////    {
        ////        try
        ////        {
        ////            SendCommand(Constants.COMMAND_RESET);
        ////            //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_SPEED_MODE);
        ////            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
        ////            Byte[] response = SendCommand(Constants.COMMAND_READ);

        ////            if (ChecksumMatches(response, Constants.DATA_TYPE_HW_FIXED_PERCENT))
        ////            {

        ////            }

        ////            if (ChecksumMatches(response, Constants.DATA_TYPE_HW_SPEED_MODE))
        ////            {
        ////                Byte[] data = new Byte[response[6] + 1];

        ////                for (Int32 i = 0; i < data.Length; i++)
        ////                {
        ////                    data[i] = response[7 + i];
        ////                }

        ////                SendData(Constants.MODE_HW_SPEED_MODE, Constants.DATA_TYPE_HW_SPEED_MODE, data);

        ////                SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
        ////                response = SendCommand(Constants.COMMAND_READ);

        ////                if (ChecksumMatches(response, Constants.DATA_TYPE_HW_FIXED_PERCENT))
        ////                {
        ////                    data = new Byte[response[6] * 2 + 1];

        ////                    for (Int32 i = 0; i < data.Length; i++)
        ////                    {
        ////                        data[i] = response[7 + i];
        ////                    }

        ////                    SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

        ////                    Byte[] powerData = BitConverter.GetBytes(power);

        ////                    data[channel * 2 + 1] = powerData[0];
        ////                    data[channel * 2 + 2] = powerData[1];

        ////                    SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

        ////                    //duty_le = int.to_bytes(clamp(duty, 0, 100), length = 2, byteorder = "little", signed = False)
        ////                    //for chan in channels:
        ////                    //    i = chan * 2 + 1
        ////                    //    data[i: i + 2] = duty_le  # Update the device speed
        ////                    //self._write_data(_MODE_HW_FIXED_PERCENT, _DATA_TYPE_HW_FIXED_PERCENT, data)
        ////                }
        ////            }
        ////        }
        ////        catch (Exception exception)
        ////        {
        ////            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
        ////        }
        ////        finally
        ////        {
        ////            //SendCommand(Constants.COMMAND_SLEEP);
        ////        }
        ////    }
        ////}

        //public List<Int32> GetTemperatureChannels()
        //{
        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected && !TemperatureChannels.Any())
        //    {
        //        try
        //        {
        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                System.IO.File.AppendAllText(TraceLogFileName, "Getting fan channels" + Environment.NewLine);
        //            }

        //            //SendCommand(Constants.COMMAND_WAKE);
        //            SendCommand(Constants.COMMAND_RESET);
        //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_TEMPS);
        //            Byte[] response = SendCommand(Constants.COMMAND_READ);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_TEMPS))
        //            {
        //                Int32 totalResults = response[6];

        //                for (Int32 i = 0; i < totalResults; i++)
        //                {
        //                    Int32 offset = 7 + i * 3;

        //                    if (response[offset] == 0x00)
        //                    {
        //                        TemperatureChannels.Add(i);
        //                    }
        //                }
        //            }

        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                foreach (Int32 channel in TemperatureChannels)
        //                {
        //                    System.IO.File.AppendAllText(TraceLogFileName, $"\tFound temperature probe on channel {channel}" + Environment.NewLine);
        //                }
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }

        //    return TemperatureChannels;
        //}

        //public Single GetTemperature(Int32 channel)
        //{
        //    Single result = 0;

        //    if (!IsConnected)
        //    {
        //        Connect();
        //    }

        //    if (IsConnected)
        //    {
        //        try
        //        {
        //            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //            {
        //                System.IO.File.AppendAllText(TraceLogFileName, $"Getting temperature from channel {channel}" + Environment.NewLine);
        //            }

        //            //SendCommand(Constants.COMMAND_WAKE);
        //            SendCommand(Constants.COMMAND_RESET);
        //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_TEMPS);
        //            Byte[] response = SendCommand(Constants.COMMAND_READ);

        //            if (ChecksumMatches(response, Constants.DATA_TYPE_TEMPS))
        //            {
        //                Int32 totalResults = response[6];

        //                for (Int32 i = 0; i < totalResults; i++)
        //                {
        //                    if (i != channel) continue;

        //                    Int32 offset = 7 + i * 3;

        //                    if (response[offset] == 0x00)
        //                    {
        //                        result = Convert.ToSingle(BitConverter.ToUInt16(response, offset + 1) / 10.0);

        //                        if (!String.IsNullOrWhiteSpace(TraceLogFileName))
        //                        {
        //                            System.IO.File.AppendAllText(TraceLogFileName, $"\tTemperature channel {channel}: {result}" + Environment.NewLine);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);
        //        }
        //        finally
        //        {
        //            //SendCommand(Constants.COMMAND_SLEEP);
        //        }
        //    }

        //    return result;
        //}

        //#endregion

        //#region Private methods

        //private void ConnectToDevice()
        //{
        //    foreach (HidSharp.HidDevice hidDevice in HidSharp.DeviceList.Local.GetHidDevices(Constants.VENDOR_ID))
        //    {
        //        if (hidDevice.ProductID == Constants.PRODUCT_ID && hidDevice.GetMaxInputReportLength() > 0 && hidDevice.GetMaxOutputReportLength() > 0)
        //        {
        //            device = hidDevice;

        //            break;
        //        }
        //    }
        //}

        //private Byte[] SendCommand(Byte[] command, Byte[] data = null, Boolean disconnectIfError = true)
        //{
        //    Byte[] result = new Byte[Constants.RESPONSE_SIZE];

        //    try
        //    {
        //        Byte[] request = new Byte[Constants.COMMAND_SIZE];

        //        request[1] = 0x08;

        //        Int32 outputIndex = 2;

        //        foreach (Byte commandByte in command)
        //        {
        //            request[outputIndex] = commandByte;

        //            outputIndex++;
        //        }

        //        if (data != null)
        //        {
        //            foreach (Byte dataByte in data)
        //            {
        //                request[outputIndex] = dataByte;

        //                outputIndex++;
        //            }
        //        }

        //        stream.Write(request);
        //        stream.Read(result);
        //    }
        //    catch (Exception exception)
        //    {
        //        System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

        //        if (disconnectIfError)
        //        {
        //            Disconnect();
        //        }
        //    }

        //    return result;
        //}

        //private void SendData(Byte[] mode, Byte[] checksum, Byte[] data)
        //{
        //    SendCommand(Constants.COMMAND_RESET);
        //    Byte[] response = SendCommand(Constants.COMMAND_SET_MODE, mode);

        //    if (ChecksumMatches(response, checksum))
        //    {
        //        List<Byte> payload = new List<Byte>();

        //        payload.AddRange(BitConverter.GetBytes(data.Length + 2));
        //        payload.AddRange(new Byte[] { 0x00, 0x00 });
        //        payload.AddRange(checksum);
        //        payload.AddRange(data);

        //        SendCommand(Constants.COMMAND_WRITE, payload.ToArray());
        //    }
        //}

        //private Boolean ChecksumMatches(Byte[] data, Byte[] checksum, Int32 offset = 4)
        //{
        //    Boolean result = true;

        //    for (Int32 i = 0; i < checksum.Length; ++i)
        //    {
        //        if (data[offset + i] != checksum[i])
        //        {
        //            result = false;
        //        }
        //    }

        //    return result;
        //}

        //#endregion
    }
}