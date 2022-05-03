using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FanControl.CommanderPro.Core
{
    public class CommanderCoreSWMode : ICommander
    {
        #region Private objects

        private const String ErrorLogFileName = "CommanderCORE.err.log";

#if DEBUG
        private const String TraceLogFileName = "CommanderCORE.trc.log";
#else
        private const String TraceLogFileName = "";
#endif

        private HidSharp.HidDevice device;

        private HidSharp.HidStream stream;

        private Boolean IsConnected = false;

        private String FirmwareVersion = "0.0.0";

        private List<Int32> FanChannels = new List<Int32>();

        private List<Int32> TemperatureChannels = new List<Int32>();

        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundTask;

        #endregion

        #region Properties

        public DeviceType Type => DeviceType.Core;

        #endregion

        #region Constructor

        public CommanderCoreSWMode()
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(TraceLogFileName) && System.IO.File.Exists(TraceLogFileName))
                {
                    System.IO.File.Delete(TraceLogFileName);
                }

                if (!String.IsNullOrWhiteSpace(ErrorLogFileName) && System.IO.File.Exists(ErrorLogFileName))
                {
                    System.IO.File.Delete(ErrorLogFileName);
                }

                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellation = _cancellationTokenSource.Token;

                _backgroundTask = Task.Factory.StartNew(() => StreamReaderThread(cancellation), cancellation, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (Exception exception)
            {

            }
        }

        #endregion

        #region Public methods

        public void Connect()
        {
            if (IsConnected) return;

            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Looking for a Commander CORE family device" + Environment.NewLine);
            }

            IsConnected = false;

            try
            {
                FirmwareVersion = "0.0.0";
                FanChannels.Clear();
                TemperatureChannels.Clear();

                if (!ConnectToCommanderCore())
                {
                    ConnectToCommanderCoreXt();
                }

                if (device != null)
                {
                    Console.WriteLine($"Selected device: {device.GetFriendlyName()}");
                    Console.WriteLine($"\tDevice Path: {device.DevicePath}");
                    Console.WriteLine($"\tHash code: {device.GetHashCode()}");
                    Console.WriteLine($"\tMax feature length: {device.GetMaxFeatureReportLength()}");
                    Console.WriteLine($"\tMax input length: {device.GetMaxInputReportLength()}");
                    Console.WriteLine($"\tMax output length: {device.GetMaxOutputReportLength()}");

                    if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                    {
                        System.IO.File.AppendAllText(TraceLogFileName, $"Found device: {device.GetProductName()}" + Environment.NewLine);
                    }

                    if (device.TryOpen(out stream))
                    {
                        stream.InterruptRequested += Stream_InterruptRequested;

                        IsConnected = true;

                        //SendCommand(Constants.COMMAND_WAKE);
                        //SendCommand(Constants.COMMAND_RESET);
                        //SendCommand(Constants.COMMAND_SET_MODE, Constants.MODE_CONNECTED);

                        if (IsConnected)
                        {
                            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
                            {
                                System.IO.File.AppendAllText(TraceLogFileName, $"Connected to device: {device.GetProductName()}" + Environment.NewLine);
                            }
                        }
                    }
                    else
                    {
                        IsConnected = false;
                    }
                }
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                IsConnected = false;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;

            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Disconnecting from Commander CORE" + Environment.NewLine);
            }

            //SendCommand(Constants.COMMAND_RESET, null, false);
            //SendCommand(Constants.COMMAND_SLEEP, null, false);

            stream.Dispose();
            stream = null;

            if (device != null)
            {
                device = null;
            }

            IsConnected = false;
        }

        public String GetFirmwareVersion()
        {
            throw new NotImplementedException();
        }

        public List<Int32> GetFanChannels()
        {
            throw new NotImplementedException();
        }

        public Int32 GetFanSpeed(Int32 channel)
        {
            throw new NotImplementedException();
        }

        public Int32 GetFanPower(Int32 channel)
        {
            throw new NotImplementedException();
        }

        public List<Int32> GetTemperatureChannels()
        {
            throw new NotImplementedException();
        }

        public Single GetTemperature(Int32 channel)
        {
            throw new NotImplementedException();
        }

        public void SetFanPower(Int32 channel, Int32 power)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private void StreamReaderThread(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!IsConnected)
                {
                    Connect();
                }

                if (IsConnected)
                {
                    Byte[] data = ReadStreamData();

                    if (data != null)
                    {
                        if (ChecksumMatches(data, Constants.DATA_TYPE_SPEEDS))
                        {

                        }
                    }
                }

                try
                {
                    if (!token.IsCancellationRequested)
                    {
                        TimeSpan pause = new TimeSpan(0, 0, 0, 0, 250);

                        Task delay = Task.Delay(pause, token);
                        delay.Wait(token);
                    }
                }
                catch (Exception exception)
                {

                }
            }
        }

        private Boolean ConnectToCommanderCore()
        {
            Boolean result = false;

            foreach (HidSharp.HidDevice hidDevice in HidSharp.DeviceList.Local.GetHidDevices(0x1b1c))
            {
                if (hidDevice.ProductID == 0x0c1c && hidDevice.GetMaxInputReportLength() > 0 && hidDevice.GetMaxOutputReportLength() > 0)
                {
                    device = hidDevice;

                    result = true;

                    break;
                }
            }

            //return HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c1c);

            return result;
        }

        private Boolean ConnectToCommanderCoreXt()
        {
            Boolean result = false;

            foreach (HidSharp.HidDevice hidDevice in HidSharp.DeviceList.Local.GetHidDevices(0x1b1c))
            {
                if (hidDevice.ProductID == 0x0c2a && hidDevice.GetMaxInputReportLength() > 0 && hidDevice.GetMaxOutputReportLength() > 0)
                {
                    device = hidDevice;

                    result = true;

                    break;
                }
            }

            //return HidSharp.DeviceList.Local.TryGetHidDevice(out device, 0x1b1c, 0x0c2a);

            return result;
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

                stream.Write(request);
                stream.Read(result);
            }
            catch (Exception exception)
            {
                System.IO.File.AppendAllText(ErrorLogFileName, exception.ToString() + Environment.NewLine);

                if (disconnectIfError)
                {
                    Disconnect();
                }
            }

            return result;
        }

        private Byte[] ReadStreamData()
        {
            Byte[] result = null;

            try
            {
                result = stream.Read();
            }
            catch (Exception exception)
            {
                Disconnect();
            }

            return result;
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

        private void Stream_InterruptRequested(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(TraceLogFileName))
            {
                System.IO.File.AppendAllText(TraceLogFileName, "Other process wants to access device" + Environment.NewLine);
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}