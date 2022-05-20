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
                        if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Connected to device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                        }

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
            if (!IsConnected) return;

            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Disconnecting from Commander CORE" + Environment.NewLine);
            }

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
            Int32 result = 0;

            if (IsConnected && FanChannelMap.ContainsKey(channel))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting fan channel {channel} speed" + Environment.NewLine);
                    }

                    //SendCommand(Constants.COMMAND_WAKE);
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

                            result = BitConverter.ToUInt16(response, offset);

                            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                            {
                                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tFan channel {channel} speed: {result}" + Environment.NewLine);
                            }

                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        internal Single GetTemperature(Int32 channel)
        {
            Single result = 0;

            if (IsConnected && TemperatureChannelMap.ContainsKey(channel))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting temperature from channel {channel}" + Environment.NewLine);
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

                                if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                                {
                                    System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tTemperature channel {channel}: {result}" + Environment.NewLine);
                                }
                            }

                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return result;
        }

        #endregion

        #region Private methods

        private String GetFirmwareVersion()
        {
            if (!IsConnected)
            {
                Connect();
            }

            if (IsConnected && String.Equals(FirmwareVersion, "0.0.0", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                {
                    System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Attempting to get Commander CORE firmware version" + Environment.NewLine);
                }

                try
                {
                    //SendCommand(Constants.COMMAND_WAKE);
                    //SendCommand(Constants.COMMAND_RESET);
                    Byte[] response = SendCommand(Constants.READ_FIRMWARE_VERSION);

                    if (ChecksumMatches(response, Constants.DATA_TYPE_FIRMWARE, 2))
                    {
                        FirmwareVersion = $"{response[4]}.{response[5]}.{response[6]}";

                        if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Commander CORE Firmware v{FirmwareVersion}" + Environment.NewLine);
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Bad firmware version v{FirmwareVersion}" + Environment.NewLine);
                        }

                        Disconnect();
                    }

                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }

            return FirmwareVersion;
        }

        private void GetFanChannels()
        {
            if (IsConnected && !FanChannels.Any())
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Getting fan channels" + Environment.NewLine);
                    }

                    //SendCommand(Constants.COMMAND_WAKE);
                    //SendCommand(Constants.COMMAND_RESET);
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
                                //FanChannels.Add(i);
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        foreach (Int32 channel in FanChannels)
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tFound fan on channel {channel}" + Environment.NewLine);
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }
        }

        private void GetTemperatureChannels()
        {
            if (IsConnected && !TemperatureChannels.Any())
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, "Getting temperature channels" + Environment.NewLine);
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
                            Int32 offset = 7 + i * 3;

                            if (response[offset] == 0x00)
                            {
                                Int32 channel = ((DeviceIndex - 1) * 2) + i;

                                TemperatureChannelMap.Add(channel, i);
                                TemperatureChannels.Add(channel);
                            }
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        foreach (Int32 channel in TemperatureChannels)
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tFound temperature probe on channel {channel}" + Environment.NewLine);
                        }
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);
                }
                finally
                {
                    //SendCommand(Constants.COMMAND_SLEEP);
                }
            }
        }

        private Byte[] SendCommand(Byte[] command, Byte[] data = null, Boolean disconnectIfError = true)
        {
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
                System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

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