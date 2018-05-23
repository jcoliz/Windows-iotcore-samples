using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace SpiTestToolCS
{
    class Program
    {
        // TODO: Remove this workaround when final tooling is released
        public const string Name = "SpiTestToolCS";

        // Configuration parameters
        static string FriendlyName = null;
        static SpiConnectionSettings ConnectionSettings = new SpiConnectionSettings(0);

        static async Task ListSpiControllers()
        {
            var deviceInformation = await DeviceInformation.FindAllAsync(SpiDevice.GetDeviceSelector(), new[] { "System.DeviceInterface.Spb.ControllerFriendlyName" });

            if (deviceInformation?.FirstOrDefault() == null)
                throw new Exception("There are no SPI controllers on this system");

            foreach (var device in deviceInformation)
            {
                Console.WriteLine($"Name: {device.Name} Id: {device.Id} ");
            }
        }

        static async Task ShowPrompt(string friendlyName)
        {
            //
            // Create the device
            //

            string aqs;
            if (string.IsNullOrEmpty(friendlyName))
                aqs = SpiDevice.GetDeviceSelector();
            else
                aqs = SpiDevice.GetDeviceSelector(friendlyName);

            var deviceInfos = await DeviceInformation.FindAllAsync(aqs);

            if (deviceInfos?.FirstOrDefault() == null)
                throw new Exception("SPI controller not found");

            var id = deviceInfos.First().Id;

            var device = await SpiDevice.FromIdAsync(id, ConnectionSettings);

            if (null == device)
                throw new Exception($"Device on bus {id} is in use. Please ensure that no other applications are using this SPI.");

            bool done = false;

            Console.WriteLine();
            Console.WriteLine(Strings.Help);
            Console.WriteLine();

            //
            // Command prompt loop
            //

            while (!done)
            {
                string input = ReadLine.Read("> ");

                if (input.Length == 0)
                    continue;

                var arg = new Queue<string>(input.Split(' '));
                var command = arg.Dequeue();

                try
                {
                    switch (command)
                    {
                        case "q":
                        case "quit":
                            done = true;
                            break;

                        case "h":
                        case "help":
                            Console.WriteLine(Strings.Help);
                            break;

                        case "w":
                        case "write":
                            WriteDevice(device, arg);
                            break;

                        case "r":
                        case "read":
                            ReadDevice(device, arg);
                            break;

                        case "wr":
                        case "writeread":
                            WriteReadDevice(device, arg);
                            break;

                        case "f":
                        case "fd":
                        case "fullduplex":
                            TransferFullDuplex(device, arg);
                            break;

                        case "i":
                        case "info":
                            Console.WriteLine($"         Device Id: {device.DeviceId}");
                            Console.WriteLine($"  Chip Select Line: {device.ConnectionSettings.ChipSelectLine}");
                            Console.WriteLine($"              Mode: {device.ConnectionSettings.Mode}");
                            Console.WriteLine($"   Data Bit Length: {device.ConnectionSettings.DataBitLength}");
                            Console.WriteLine($"   Clock Frequency: {device.ConnectionSettings.ClockFrequency}");
                            Console.WriteLine($"      Sharing Mode: {device.ConnectionSettings.SharingMode}");
                            break;

                        default:
                            throw new Exception($"Unrecognized command: {input}");

                    } // end switch input
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine("Type 'help' for command usage.");
                }

            } // end while not done

        }

        private static byte[] CreateReadBufferFromSizeInArgs(Queue<string> arg)
        {
            int bytesToRead;
            bool parsedOk = int.TryParse(arg.Dequeue(), out bytesToRead);
            if (!parsedOk)
                throw new Exception("Expecting integer. e.g.: read 4");

            return new byte[bytesToRead];
        }

        private static byte[] ExtractBufferFromArgs(Queue<string> arg)
        {
            List<byte> bytes = new List<byte>();

            var openBracket = arg.Dequeue();
            if (openBracket != "{")
                throw new Exception("Expecting '{'");

            do
            {
                var item = arg.Dequeue();

                if ("}" == item)
                    break;

                bytes.Add(Convert.ToByte(item, 16));
            }
            while (true); // continue until broken internally

            return bytes.ToArray();
        }

        private static void PrintBufferContents(byte[] buffer)
        {
            if (buffer != null)
            {
                foreach (var b in buffer)
                {
                    Console.Write($"{b:X} ");
                }
                Console.WriteLine();
            }
            else
                Console.WriteLine("(null)");
        }

        private static void Debug_WriteRead(byte[] writeBuffer, byte[] readBuffer)
        {
            Console.WriteLine("writeBuffer:");
            Console.Write("\t");
            PrintBufferContents(writeBuffer);

            Console.WriteLine("readBuffer:");
            Console.Write("\t");
            if (readBuffer != null)
                Console.WriteLine($"Length: {readBuffer.Length}");
            else
                Console.WriteLine("(null)");
        }

        private static void TransferFullDuplex(SpiDevice device, Queue<string> arg)
        {
            byte[] writeBuffer = ExtractBufferFromArgs(arg);
            byte[] readBuffer = CreateReadBufferFromSizeInArgs(arg);
            device.TransferFullDuplex(writeBuffer, readBuffer);
            PrintBufferContents(readBuffer);
        }

        private static void WriteReadDevice(SpiDevice device, Queue<string> arg)
        {
            byte[] writeBuffer = ExtractBufferFromArgs(arg);
            byte[] readBuffer = CreateReadBufferFromSizeInArgs(arg);
            device.TransferSequential(writeBuffer, readBuffer);
            PrintBufferContents(readBuffer);
        }

        private static void ReadDevice(SpiDevice device, Queue<string> arg)
        {
            byte[] readBuffer = CreateReadBufferFromSizeInArgs(arg);
            device.Read(readBuffer);
            PrintBufferContents(readBuffer);
        }

        private static void WriteDevice(SpiDevice device, Queue<string> arg)
        {
            byte[] writeBuffer = ExtractBufferFromArgs(arg);
            device.Write(writeBuffer);
        }

        static async Task<int> Main(string[] args)
        {
            bool success = false;
            bool done = false;

            try
            {
                string[] helpArgs = { "-h", "/h", "-help", "/help", "-?", "/?" };
                string[] listArgs = { "-l", "/l", "-list", "/list" };

                Queue<string> argQ = new Queue<string>(args);

                while (argQ.Count > 0)
                {
                    var command = argQ.Dequeue();

                    if (helpArgs.Contains(command))
                    {
                        Console.WriteLine(Strings.Usage);
                        done = true;
                        break;
                    }
                    if (listArgs.Contains(command))
                    {
                        await ListSpiControllers();
                        done = true;
                        break;
                    }

                    if (command[0] == '-' || command[0] == '/')
                    {
                        switch (command[1])
                        {
                            case 'n':
                                FriendlyName = argQ.Dequeue();
                                break;
                            case 'c':
                                ConnectionSettings.ChipSelectLine = int.Parse(argQ.Dequeue());
                                break;
                            case 'm':
                                int spiMode = int.Parse(argQ.Dequeue());
                                if (spiMode < 0 || spiMode > 3)
                                    throw new Exception($"Invalid spi mode {spiMode}");
                                ConnectionSettings.Mode = (SpiMode)spiMode;
                                break;
                            case 'd':
                                ConnectionSettings.DataBitLength = int.Parse(argQ.Dequeue());
                                break;
                            case 'f':
                                int clockFrequency = int.Parse(argQ.Dequeue());
                                if (clockFrequency <= 0)
                                    throw new Exception("Invalid clock frequency");
                                ConnectionSettings.ClockFrequency = clockFrequency;
                                break;
                            default:
                                throw new Exception($"Unexpected option {command}");
                        };
                    }
                    else
                        throw new Exception($"Unexpected input {command}");

                } // while argQ count > 0

                if (!done)
                    await ShowPrompt(FriendlyName);

                success = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
            return success ? 0 : 1;
        }
    }
}
