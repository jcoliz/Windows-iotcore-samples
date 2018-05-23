using System;
using System.Linq;
using Windows.Devices.Gpio;

namespace GpioTestTool
{
    class Program
    {
        public const string Name = "GpioTestToolCS";

        static void ListPins()
        {
            var controller = GpioController.GetDefault();

            if (null == controller)
                throw new Exception("GPIO is not available on this system");

            var pinCount = controller.PinCount;

            for (int pinNumber = 0; pinNumber < pinCount; ++pinNumber)
            {
                GpioPin pin;
                GpioOpenStatus status;
                var openOk = controller.TryOpenPin(pinNumber, GpioSharingMode.SharedReadOnly, out pin, out status);

                Console.Write($"Pin {pinNumber}: ");
                if (!openOk)
                {
                    Console.WriteLine("Failed to open");
                }
                else
                {
                    Console.WriteLine($"Status: {status} Drive Mode: {pin.GetDriveMode()} Value: {pin.Read()}");
                }
            }
        }

        static void ShowPrompt(int pinNumber)
        {
            var controller = GpioController.GetDefault();

            GpioPin pin = null;
            bool done = false;
            bool listenerRegistered = false;

            if (null == controller)
                throw new Exception("GPIO is not available on this system");

            GpioOpenStatus status;

            // First, try to open it exclusive
            var exclusiveOpenOk = controller.TryOpenPin(pinNumber, GpioSharingMode.Exclusive, out pin, out status);
            if (exclusiveOpenOk)
            {
                Console.WriteLine($"Pin opened for exclusive use.");
            }
            else
            {
                // But it's OK if we get it readonly
                var sharedOpenOk = controller.TryOpenPin(pinNumber, GpioSharingMode.SharedReadOnly, out pin, out status);
                if (sharedOpenOk)
                {
                    Console.WriteLine($"Pin opened for shared read-only use.");
                }
                else
                {
                    Console.WriteLine("Failed to open");
                    done = true;
                }
            }

            if (!done)
                Console.WriteLine($"Status: {status} Drive Mode: {pin.GetDriveMode()} Value: {pin.Read()}");

            Console.WriteLine();
            Console.WriteLine(Strings.Help);
            Console.WriteLine();

            while (!done)
            {
                string input = ReadLine.Read("> ");

                if (input.Length == 0)
                    continue;

                var arg = input.Split(' ');
                var command = arg[0];

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
                            uint value = uint.Parse(arg[1]);
                            pin.Write(value == 0 ? GpioPinValue.Low : GpioPinValue.High);
                            break;

                        case "high":
                            pin.Write(GpioPinValue.High);
                            break;

                        case "low":
                            pin.Write(GpioPinValue.Low);
                            break;

                        case "t":
                        case "toggle":
                            GpioPinValue oldValue = pin.Read();
                            pin.Write(oldValue == GpioPinValue.High ? GpioPinValue.Low : GpioPinValue.High);
                            break;

                        case "r":
                        case "read":
                            Console.WriteLine($"Value: {pin.Read()}");
                            break;

                        case "d":
                        case "drive":
                        case "mode":
                        case "setdrivemode":
                            GpioPinDriveMode newMode = Enum.Parse<GpioPinDriveMode>(arg[1]);
                            pin.SetDriveMode(newMode);
                            break;

                        case "i":
                        case "info":
                            Console.WriteLine($"        Pin Number: {pin.PinNumber}");
                            Console.WriteLine($"      Sharing Mode: {pin.SharingMode}");
                            Console.WriteLine($"  Debounce Timeout: {pin.DebounceTimeout.TotalMilliseconds} ms");
                            Console.WriteLine($"        Drive Mode: {pin.GetDriveMode()}");
                            Console.WriteLine($"             Value: {pin.Read()}");
                            break;

                        case "int":
                        case "interrupt":
                            if ("on" == arg[1])
                            {
                                if (listenerRegistered)
                                    throw new Exception("Interrupt listener already registered.");

                                pin.ValueChanged += Pin_ValueChanged;
                                listenerRegistered = true;
                            }
                            else if ("off" == arg[1])
                            {
                                if (!listenerRegistered)
                                    throw new Exception("Interrupt listener not registered.");

                                pin.ValueChanged -= Pin_ValueChanged;
                                listenerRegistered = false;
                            }
                            else
                                throw new Exception("Expected 'on' or 'off'");

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

        private static void Pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Console.WriteLine($"Pin {sender.PinNumber} Interrupt! Edge: {args.Edge}");
            Console.Write("> ");
        }

        static int Main(string[] args)
        {
            bool success = false;

            try
            {
                string[] helpArgs = { "-h", "/h", "-help", "/help", "-?", "/?" };
                string[] listArgs = { "-l", "/l", "-list", "/list" };

                if (null == args || args.Length < 1)
                {
                    throw new Exception($"Missing required command line parameter: PinNumber\nType {Program.Name} /? for usage");
                }
                else if (helpArgs.Contains(args[0]))
                {
                    Console.WriteLine(Strings.Usage);
                }
                else if (listArgs.Contains(args[0]))
                {
                    ListPins();
                }
                else
                {
                    Int32 pinNumber;
                    bool parsedOk = Int32.TryParse(args[0], out pinNumber);
                    if (!parsedOk)
                    {
                        throw new Exception($"Expecting integer: {args[0]}\nType {Program.Name} /? for usage");
                    }

                    ShowPrompt(pinNumber);
                }

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