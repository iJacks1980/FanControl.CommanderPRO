using System;
using System.Collections.Generic;
using System.Linq;

namespace FanControl.CommanderPro
{
    internal class Device
    {
        #region Private objects

        private Int32 DeviceIndex;

        private HidSharp.HidDevice HidDevice;

        private HidSharp.HidStream HidStream;

        private Byte[] outbuf = new Byte[Constants.COMMAND_SIZE];

        private Byte[] inbuf = new Byte[Constants.RESPONSE_SIZE];

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

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Failed to connect to device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }
                }
            }
        }

        internal void Disconnect()
        {
            if (!IsConnected) return;

            if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
            {
                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Disconnecting from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
            }

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
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting fan speed for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }

                    ClearOutputBuffer();

                    outbuf[1] = Constants.READ_FAN_SPEED;
                    outbuf[2] = (Byte)FanChannelMap[channel];

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Raw fan speed data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(inbuf)}" + Environment.NewLine);
                    }

                    result = 256 * inbuf[2] + inbuf[3];

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Converted fan speed data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        internal Int32 GetFanPower(Int32 channel)
        {
            Int32 result = 0;

            if (IsConnected && FanChannelMap.ContainsKey(channel))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting fan power for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }

                    ClearOutputBuffer();

                    outbuf[1] = Constants.READ_FAN_POWER;
                    outbuf[2] = (Byte)FanChannelMap[channel];

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Raw fan power data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(inbuf)}" + Environment.NewLine);
                    }

                    result = 256 * inbuf[2] + inbuf[3];

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Converted fan power data for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            return result;
        }

        internal void SetFanSpeed(Int32 channel, Int32 speed)
        {
            if (IsConnected && FanChannelMap.ContainsKey(channel))
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Setting fan speed for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) to: {speed}" + Environment.NewLine);
                    }

                    ClearOutputBuffer();

                    outbuf[1] = Constants.WRITE_FAN_SPEED;
                    outbuf[2] = (Byte)FanChannelMap[channel];
                    outbuf[3] = (Byte)(speed >> 8);
                    outbuf[4] = (Byte)(speed & 0xff);

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
        }

        internal void SetFanPower(Int32 channel, Int32 power)
        {
            if (IsConnected && FanChannelMap.ContainsKey(channel) && power >= 0 && power <= 100)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Setting fan power for channel {FanChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) to: {power}" + Environment.NewLine);
                    }

                    ClearOutputBuffer();

                    outbuf[1] = Constants.WRITE_FAN_POWER;
                    outbuf[2] = (Byte)FanChannelMap[channel];
                    outbuf[3] = (Byte)(power);

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }
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
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting temperature for channel {TemperatureChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }

                    ClearOutputBuffer();

                    outbuf[1] = Constants.READ_TEMPERATURE_VALUE;
                    outbuf[2] = (Byte)TemperatureChannelMap[channel];

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Raw temperature data for channel {TemperatureChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(inbuf)}" + Environment.NewLine);
                    }

                    result = BitConverter.ToUInt16(inbuf, 1) / 100;

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Converted temperature data for channel {TemperatureChannelMap[channel]} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
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
                    System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Attempting to get Commander PRO firmware version from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                }

                ClearOutputBuffer();

                outbuf[1] = Constants.READ_FIRMWARE_VERSION;

                try
                {
                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    FirmwareVersion = String.Empty;

                    for (Int32 i = 2; i < 5; ++i)
                    {
                        if (i > 2) { FirmwareVersion = FirmwareVersion + "." + inbuf[i]; }
                        else { FirmwareVersion = FirmwareVersion + inbuf[i]; }
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Commander PRO Firmware from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = v{FirmwareVersion}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
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
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting fan channels from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }

                    String fanMask = GetFanMask();

                    try
                    {
                        for (Int32 j = 0; j < fanMask.Length; j++)
                        {
                            Char y = fanMask[j];

                            switch (y)
                            {
                                case '1':
                                case '2':
                                    Int32 channel = ((DeviceIndex - 1) * 6) + j;

                                    FanChannelMap.Add(channel, j);
                                    FanChannels.Add(channel);

                                    break;
                            }
                        }

                        if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                        {
                            foreach (Int32 channel in FanChannels)
                            {
                                System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tFound fan on channel {channel} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                        IsConnected = false;
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
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Getting temperature channels from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
                    }

                    String temperatureMask = GetTemperatureMask();

                    try
                    {
                        for (Int32 j = 0; j < temperatureMask.Length; j++)
                        {
                            Char y = temperatureMask[j];

                            switch (y)
                            {
                                case '1':
                                    Int32 channel = ((DeviceIndex - 1) * 4) + j;

                                    TemperatureChannelMap.Add(channel, j);
                                    TemperatureChannels.Add(channel);

                                    break;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                        IsConnected = false;
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        foreach (Int32 channel in TemperatureChannels)
                        {
                            System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"\tFound temperature probe on channel {channel} from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()})" + Environment.NewLine);
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

        private void ClearOutputBuffer()
        {
            for (Int32 i = 0; i < 64; ++i)
            {
                outbuf[i] = 0x00;
            }
        }

        private String GetFanMask()
        {
            String result = "";

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_FAN_MASK;

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Raw fan mask data from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(inbuf)}" + Environment.NewLine);
                    }

                    for (Int32 i = 2; i < 8; ++i)
                    {
                        result = result + inbuf[i].ToString();
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Converted fan mask data from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            if (result.Length != 6)
            {
                result = "000000";
            }

            return result;
        }

        private String GetTemperatureMask()
        {
            String result = "";

            if (IsConnected)
            {
                ClearOutputBuffer();

                try
                {
                    outbuf[1] = Constants.READ_TEMPERATURE_MASK;

                    HidStream.Write(outbuf);
                    HidStream.Read(inbuf);

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Raw temperature mask data from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {BitConverter.ToString(inbuf)}" + Environment.NewLine);
                    }

                    for (Int32 i = 2; i < 6; ++i)
                    {
                        result = result + inbuf[i].ToString();
                    }

                    if (!String.IsNullOrWhiteSpace(Constants.TRACE_LOG_FILE_NAME))
                    {
                        System.IO.File.AppendAllText(Constants.TRACE_LOG_FILE_NAME, $"Converted temperature mask data from device: {HidDevice.GetProductName()} ({HidDevice.GetSerialNumber()}) = {result}" + Environment.NewLine);
                    }
                }
                catch (Exception exception)
                {
                    System.IO.File.AppendAllText(Constants.ERROR_LOG_FILE_NAME, exception.ToString() + Environment.NewLine);

                    IsConnected = false;
                }
            }

            if (result.Length != 4)
            {
                result = "0000";
            }

            return result;
        }

        #endregion
    }
}