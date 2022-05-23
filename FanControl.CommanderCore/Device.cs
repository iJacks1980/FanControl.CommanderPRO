using FanControl.Commander.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderCore
{
    internal class Device
    {
        #region Private objects

        private Int32 DeviceIndex;

        private HidSharp.HidDevice HidDevice;

        private HidSharp.HidStream HidStream;

        private Dictionary<Int32, Int32> FanChannelMap = new Dictionary<Int32, Int32>();

        private Dictionary<Int32, Int32> TemperatureChannelMap = new Dictionary<Int32, Int32>();

        #endregion

        #region Constructor

        internal Device(Int32 deviceIndex, HidSharp.HidDevice hidDevice)
        {
            DeviceIndex = deviceIndex;
            HidDevice = hidDevice;
        }

        #endregion

        #region Properties

        internal Boolean IsConnected { get; set; }

        internal String FirmwareVersion { get; set; } = "0.0.0";

        internal List<Int32> FanChannels { get; set; } = new List<Int32>();

        internal List<Int32> TemperatureChannels { get; set; } = new List<Int32>();

        #endregion

        #region Internal methods

        internal void Connect()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.Connect()");

            if (!IsConnected)
            {
                HidSharp.OpenConfiguration openConfiguration = new HidSharp.OpenConfiguration();

                openConfiguration.SetOption(HidSharp.OpenOption.Exclusive, true);
                openConfiguration.SetOption(HidSharp.OpenOption.Transient, true);
                openConfiguration.SetOption(HidSharp.OpenOption.Interruptible, true);

                if (HidDevice.TryOpen(openConfiguration, out HidStream))
                {
                    IsConnected = true;

                    SendCommand(Constants.COMMAND_WAKE);

                    if (IsConnected)
                    {
                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Connected to device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})");

                        GetFirmwareVersion();
                        GetFanChannels();
                        GetTemperatureChannels();
                    }
                }
                else
                {
                    IsConnected = false;
                }
            }
        }

        internal void Disconnect()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.Disconnect()");

            if (!IsConnected) return;

            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Disconnecting from Commander CORE");

            SendCommand(Constants.COMMAND_RESET, null, false);
            SendCommand(Constants.COMMAND_SLEEP, null, false);

            HidStream.Dispose();
            HidStream = null;

            if (HidDevice != null)
            {
                HidDevice = null;
            }

            IsConnected = false;
        }

        internal Int32 GetFanSpeed(Int32 channel)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.GetFanSpeed()");

            Int32 result = 0;

            if (IsConnected && FanChannelMap.ContainsKey(channel))
            {
                try
                {
                    Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Getting fan channel {channel} speed");

                    SendCommand(Constants.COMMAND_RESET);
                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_SPEEDS);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_SPEEDS))
                    {
                        Int32 totalResults = response[6];

                        for (Int32 i = 0; i < totalResults * 2; i++)
                        {
                            Int32 deviceChannel = FanChannelMap[channel];

                            if (i != deviceChannel) continue;

                            Int32 offset = 7 + i * 2;

                            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Raw fan speed data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(response)}");

                            result = BitConverter.ToUInt16(response, offset);

                            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Converted fan speed data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}");

                            break;
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

        internal Single GetTemperature(Int32 channel)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.GetTemperature()");

            Single result = 0;

            if (IsConnected && TemperatureChannelMap.ContainsKey(channel))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Getting temperature from channel {channel}");
                    }

                    //SendCommand(Constants.COMMAND_WAKE);
                    SendCommand(Constants.COMMAND_RESET);
                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_TEMPS);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_TEMPS))
                    {
                        Int32 totalResults = response[6];

                        for (Int32 i = 0; i < totalResults; i++)
                        {
                            Int32 deviceChannel = TemperatureChannelMap[channel];

                            if (i != deviceChannel) continue;

                            Int32 offset = 7 + i * 3;

                            if (response[offset] == 0x00)
                            {
                                result = Convert.ToSingle(BitConverter.ToUInt16(response, offset + 1) / 10.0);

                                Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"\tTemperature channel {channel}: {result}");
                            }

                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());

                    Disconnect();
                }
            }

            return result;
        }

        internal void SetFanPower(Int32 channel, Int32 power)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.SetFanPower()");

            //if (IsConnected && FanChannelMap.ContainsKey(channel))
            //{
            //    try
            //    {
            //        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Setting fan channel {channel} power");

            //        SendCommand(Constants.COMMAND_RESET);
            //        SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_SPEED_MODE);

            //        Byte[] response = SendCommand(Constants.COMMAND_READ);

            //        String convertedResponse = BitConverter.ToString(response);

            //        if (ChecksumMatches(response, Constants.DATA_TYPE_HW_SPEED_MODE))
            //        {
            //            Byte[] data = new Byte[response[6] + 1];

            //            for (Int32 i = 0; i < data.Length; i++)
            //            {
            //                data[i] = response[7 + i];
            //            }

            //            data[channel] = 0x00;

            //            String convertedData = BitConverter.ToString(data);

            //            SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

            //            SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
            //            response = SendCommand(Constants.COMMAND_READ);

            //            data = new Byte[(response[6] * 2) + 1];

            //            Byte[] powerData = BitConverter.GetBytes(power);
            //            data[channel * 2 + 1] = powerData[0];
            //            data[channel * 2 + 2] = powerData[1];

            //            convertedData = BitConverter.ToString(data);

            //            SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);
            //        }

            //        //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);


            //        //if (ChecksumMatches(response, Constants.DATA_TYPE_HW_SPEED_MODE))
            //        //{
            //        //    Byte[] data = new Byte[response[6] + 1];

            //        //    for (Int32 i = 0; i < data.Length; i++)
            //        //    {
            //        //        data[i] = response[7 + i];
            //        //    }

            //        //    SendData(Constants.MODE_HW_SPEED_MODE, Constants.DATA_TYPE_HW_SPEED_MODE, data);

            //        //    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_HW_FIXED_PERCENT);
            //        //    response = SendCommand(Constants.COMMAND_READ);

            //        //    if (ChecksumMatches(response, Constants.DATA_TYPE_HW_FIXED_PERCENT))
            //        //    {
            //        //        data = new Byte[response[6] * 2 + 1];

            //        //        for (Int32 i = 0; i < data.Length; i++)
            //        //        {
            //        //            data[i] = response[7 + i];
            //        //        }

            //        //        SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

            //        //        Byte[] powerData = BitConverter.GetBytes(power);

            //        //        data[channel * 2 + 1] = powerData[0];
            //        //        data[channel * 2 + 2] = powerData[1];

            //        //        SendData(Constants.MODE_HW_FIXED_PERCENT, Constants.DATA_TYPE_HW_FIXED_PERCENT, data);

            //        //        //duty_le = int.to_bytes(clamp(duty, 0, 100), length = 2, byteorder = "little", signed = False)
            //        //        //for chan in channels:
            //        //        //    i = chan * 2 + 1
            //        //        //    data[i: i + 2] = duty_le  # Update the device speed
            //        //        //self._write_data(_MODE_HW_FIXED_PERCENT, _DATA_TYPE_HW_FIXED_PERCENT, data)
            //        //    }
            //        //}
            //    }
            //    catch (Exception exception)
            //    {
            //        Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());
            //    }
            //}
        }

        #endregion

        #region Private methods

        private String GetFirmwareVersion()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.GetFirmwareVersion()");

            if (!IsConnected)
            {
                Connect();
            }

            if (IsConnected && String.Equals(FirmwareVersion, "0.0.0", StringComparison.InvariantCultureIgnoreCase))
            {
                Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Attempting to get Commander CORE firmware version");

                try
                {
                    Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
                    {
                        FirmwareVersion = $"{response[4]}.{response[5]}.{response[6]}";

                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Commander CORE Firmware v{FirmwareVersion}");
                    }
                    else
                    {
                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"Bad firmware version v{FirmwareVersion}");

                        Disconnect();
                    }

                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());

                    Disconnect();

                    IsConnected = false;
                }
            }

            return FirmwareVersion;
        }

        private void GetFanChannels()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.GetFanChannels()");

            if (IsConnected && !FanChannels.Any())
            {
                try
                {
                    Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Getting fan channels");

                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_SW_CONNECTED))
                    {
                        Int32 totalDevices = response[6];

                        for (Int32 i = 0; i < totalDevices; i++)
                        {
                            //0 = AIO Pump, not a fan so ignore
                            if (response[i + 7] == 0x07)
                            {
                                Int32 channel = ((DeviceIndex - 1) * 7) + i;

                                FanChannelMap.Add(channel, i);
                                FanChannels.Add(channel);
                            }
                        }
                    }

                    foreach (Int32 channel in FanChannels)
                    {
                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"\tFound fan on channel {channel}");
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());

                    Disconnect();
                }
            }
        }

        private void GetTemperatureChannels()
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.GetTemperatureChannels()");

            if (IsConnected && !TemperatureChannels.Any())
            {
                try
                {
                    Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Getting temperature channels");

                    //SendCommand(Constants.COMMAND_RESET);
                    SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_GET_TEMPS);
                    Byte[] response = SendCommand(Constants.COMMAND_READ);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_TEMPS))
                    {
                        Int32 totalResults = response[6];

                        for (Int32 i = 0; i < totalResults; i++)
                        {
                            Int32 offset = 7 + i * 3;

                            if (response[offset] == 0x00)
                            {
                                Int32 channel = ((DeviceIndex - 1) * 2) + i;

                                TemperatureChannelMap.Add(channel, i);
                                TemperatureChannels.Add(channel);
                            }
                        }
                    }

                    foreach (Int32 channel in TemperatureChannels)
                    {
                        Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, $"\tFound temperature probe on channel {channel}");
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());

                    Disconnect();
                }
            }
        }

        private Byte[] SendCommand(Byte[] command, Byte[] data = null, Boolean disconnectIfError = true)
        {
            Log.WriteToLog(Constants.TRACE_LOG_FILE_NAME, "Device.SendCommand()");

            Byte[] result = new Byte[Constants.RESPONSE_SIZE];

            try
            {
                Byte[] request = new Byte[Constants.COMMAND_SIZE];

                request[1] = 0x08;

                Int32 outputIndex = 2;

                foreach (Byte commandByte in command)
                {
                    request[outputIndex] = commandByte;

                    outputIndex++;
                }

                if (data != null)
                {
                    foreach (Byte dataByte in data)
                    {
                        request[outputIndex] = dataByte;

                        outputIndex++;
                    }
                }

                HidStream.Write(request);
                HidStream.Read(result);
            }
            catch (Exception exception)
            {
                Log.WriteToLog(Constants.ERROR_LOG_FILE_NAME, exception.ToString());

                if (disconnectIfError)
                {
                    Disconnect();
                }
            }

            return result;
        }

        private void SendData(Byte[] mode, Byte[] checksum, Byte[] data)
        {
            SendCommand(Constants.COMMAND_RESET);
            Byte[] response = SendCommand(Constants.COMMAND_SET_MODE, mode);

            if (ChecksumMatches(response, checksum))
            {
                List<Byte> payload = new List<Byte>();

                payload.AddRange(BitConverter.GetBytes(data.Length + 2));
                payload.AddRange(new Byte[] { 0x00, 0x00 });
                payload.AddRange(checksum);
                payload.AddRange(data);

                SendCommand(Constants.COMMAND_WRITE, payload.ToArray());
            }
        }

        private Boolean ChecksumMatches(Byte[] data, Byte[] checksum, Int32 offset = 4)
        {
            Boolean result = true;

            for (Int32 i = 0; i < checksum.Length; ++i)
            {
                if (data[offset + i] != checksum[i])
                {
                    result = false;
                }
            }

            return result;
        }

        #endregion
    }
}