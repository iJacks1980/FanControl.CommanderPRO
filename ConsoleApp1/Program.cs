using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(String[] args)
        {
            Console.WriteLine("Select mode:");
            Console.WriteLine("Press 1 for Hardware mode");
            Console.WriteLine("Press 2 for Software mode");

            var selection = Console.ReadKey(true);

            switch (selection.Key)
            {
                case ConsoleKey.D1:
                    HardwareMode();

                    break;
                case ConsoleKey.D2:
                    SoftwareMode();

                    break;
            }
        }

        private static void HardwareMode()
        {
            Console.WriteLine("Hardware mode...");

            FanControl.CommanderPro.Core.CommanderCore commander = new FanControl.CommanderPro.Core.CommanderCore();

            commander.Connect();

            Boolean exitRequested = false;

            String firmware;

            while (!exitRequested)
            {
                firmware = commander.GetFirmwareVersion();

                if (String.Equals(firmware, "0.0.0"))
                {
                    Console.WriteLine("Bad firmware data!");
                }
                else
                {
                    Console.WriteLine($"Firmware v{firmware}");

                    List<Int32> fanChannels = commander.GetFanChannels();

                    foreach (Int32 channel in fanChannels)
                    {
                        Int32 speed = commander.GetFanSpeed(channel);

                        Console.WriteLine($"\tFan on channel {channel} speed: {speed}");
                    }

                    List<Int32> temperatureChannels = commander.GetTemperatureChannels();

                    foreach (Int32 channel in temperatureChannels)
                    {
                        Single temperature = commander.GetTemperature(channel);

                        Console.WriteLine($"\tTemperature probe {channel}: {temperature}");
                    }

                    //commander.SetFanPower(3, 100);
                }

                if (Console.KeyAvailable)
                {
                    exitRequested = true;
                }

                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

                Task delay = Task.Delay(pause);
                delay.Wait();
            }

            commander.Disconnect();

            Console.ReadLine();
        }

        private static void SoftwareMode()
        {
            Console.WriteLine("Software mode...");

            FanControl.CommanderPro.Core.CommanderCoreSWMode commander = new FanControl.CommanderPro.Core.CommanderCoreSWMode();

            //commander.Connect();

            Boolean exitRequested = false;

            while (!exitRequested)
            {


                if (Console.KeyAvailable)
                {
                    exitRequested = true;
                }

                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

                Task delay = Task.Delay(pause);
                delay.Wait();
            }
        }
    }
}