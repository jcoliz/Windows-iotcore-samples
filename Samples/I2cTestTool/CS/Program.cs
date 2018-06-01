using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;

namespace GpioTestTool
{
    class Program
    {
        public const string Name = "I2cTestToolCS";

        static async Task ListI2cControllers()
        {
            var deviceInformation = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector(), new[] { "System.DeviceInterface.Spb.ControllerFriendlyName" });

            if (deviceInformation?.FirstOrDefault() == null)
                throw new Exception("There are no I2C controllers on this system");

            foreach (var device in deviceInformation)
            {
                Console.WriteLine($"Name: {device.Name} Id: {device.Id} ");
            }
        }

        static async Task ShowPrompt(int slaveAddress, string friendlyName)
        {
            //
            // Create the device
            //

            string aqs;
            if (string.IsNullOrEmpty(friendlyName))
                aqs = I2cDevice.GetDeviceSelector();
            else
                aqs = I2cDevice.GetDeviceSelector(friendlyName);

            var deviceInfos = await DeviceInformation.FindAllAsync(aqs);

            if (deviceInfos?.FirstOrDefault() == null)
                throw new Exception("I2C controller not found");

            var id = deviceInfos.First().Id;
            var device = await I2cDevice.FromIdAsync(id, new I2cConnectionSettings(slaveAddress));

            if (null == device)
                throw new Exception($"Slave address 0x{slaveAddress:X} on bus {id} is in use. Please ensure that no other applications are using this I2C.");

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

                var arg = new Queue<string>( input.Split(' ') );
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

                        case "i":
                        case "info":
                            Console.WriteLine($"         Device Id: {device.DeviceId}");
                            Console.WriteLine($"     Slave Address: {device.ConnectionSettings.SlaveAddress:X}");
                            Console.WriteLine($"         Bus Speed: {device.ConnectionSettings.BusSpeed}");
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

        private static void PrintTransferResult(I2cTransferResult result)
        {
            switch (result.Status)
            {
                case I2cTransferStatus.FullTransfer:
                    Console.WriteLine("OK");
                    break;
                case I2cTransferStatus.PartialTransfer:
                    Console.WriteLine($"Partial transfer: {result.BytesTransferred} bytes");
                    break;
                case I2cTransferStatus.SlaveAddressNotAcknowledged:
                    Console.WriteLine("Slave address was not acknowledged");
                    break;
                case I2cTransferStatus.ClockStretchTimeout:
                    Console.WriteLine("Clock stretch timeout occurred");
                    break;
                case I2cTransferStatus.UnknownError:
                    Console.WriteLine("An unknown error was reported");
                    break;
                default:
                    throw new Exception("Invalid transfer status value");
            }
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

        private static I2cTransferResult Debug_WriteReadPartial(byte[] writeBuffer, byte[] readBuffer)
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

            return new I2cTransferResult() { Status = I2cTransferStatus.FullTransfer };
        }

        private static void WriteReadDevice(I2cDevice device, Queue<string> arg)
        {
            byte[] writeBuffer = ExtractBufferFromArgs(arg);
            byte[] readBuffer = CreateReadBufferFromSizeInArgs(arg);
            var result = device.WriteReadPartial(writeBuffer,readBuffer);
            PrintTransferResult(result);
            PrintBufferContents(readBuffer);
        }

        private static void ReadDevice(I2cDevice device, Queue<string> arg)
        {
            byte[] readBuffer = CreateReadBufferFromSizeInArgs(arg);
            var result = device.ReadPartial(readBuffer);
            PrintTransferResult(result);
            PrintBufferContents(readBuffer);
        }

        private static void WriteDevice(I2cDevice device, Queue<string> arg)
        {
            byte[] writeBuffer = ExtractBufferFromArgs(arg);
            var result = device.WritePartial(writeBuffer);
            PrintTransferResult(result);
        }

        // Async main now in C# 7.1, huzzah!
        static async Task<int> Main(string[] args)
        {
            bool success = false;

            try
            {
                string[] helpArgs = { "-h", "/h", "-help", "/help", "-?", "/?" };
                string[] listArgs = { "-l", "/l", "-list", "/list" };

                if (null == args || args.Length < 1)
                {
                    throw new Exception($"Missing required command line parameter: SlaveAddress\nType {Program.Name} /? for usage");
                }
                else if (helpArgs.Contains(args[0]))
                {
                    Console.WriteLine(Strings.Usage);
                }
                else if (listArgs.Contains(args[0]))
                {
                    await ListI2cControllers();
                }
                else
                {
                    Int32 slaveAddress = Convert.ToInt32(args[0], 16);

                    string friendlyName = null;
                    if (args.Length > 1)
                        friendlyName = args[1];

                    await ShowPrompt(slaveAddress,friendlyName);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }

            return success ? 0 : 1;
        }
    }
}