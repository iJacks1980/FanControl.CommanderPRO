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
            //Console.WriteLine("Press 2 for Software mode");

            ConsoleKeyInfo selection = Console.ReadKey(true);

            switch (selection.Key)
            {
                case ConsoleKey.D1:
                    HardwareMode();

                    break;
                //case ConsoleKey.D2:
                //    SoftwareMode();

                //    break;
            }
        }

        private static void HardwareMode()
        {
            Console.WriteLine("Hardware mode...");

            FanControl.CommanderCore.DeviceManager commander = new FanControl.CommanderCore.DeviceManager();

            commander.Connect();

            Boolean exitRequested = false;

            while (!exitRequested)
            {
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

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyPress = Console.ReadKey(true);

                    switch (keyPress.Key)
                    {
                        case ConsoleKey.Enter:
                            exitRequested = true;

                            break;
                        case ConsoleKey.Add:
                            commander.SetFanPower(5, 75);

                            break;
                        case ConsoleKey.Subtract:
                            commander.SetFanPower(5, 25);

                            break;
                    }
                }

                TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

                Task delay = Task.Delay(pause);
                delay.Wait();
            }

            commander.Disconnect();

            Console.ReadLine();
        }

        //private static void SoftwareMode()
        //{
        //    Console.WriteLine("Software mode...");

        //    FanControl.CommanderPro.Core.CommanderCoreSWMode commander = new FanControl.CommanderPro.Core.CommanderCoreSWMode();

        //    //commander.Connect();

        //    Boolean exitRequested = false;

        //    while (!exitRequested)
        //    {


        //        if (Console.KeyAvailable)
        //        {
        //            exitRequested = true;
        //        }

        //        TimeSpan pause = new TimeSpan(0, 0, 0, 0, 1000);

        //        Task delay = Task.Delay(pause);
        //        delay.Wait();
        //    }
        //}
    }
}