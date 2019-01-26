namespace SampleModule
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    using EdgeModuleSamples.Common;

    class Program
    {
        static AppOptions Options;
        static ModuleClient ioTHubModuleClient;
        static readonly Random Rnd = new Random();

        static async Task Main(string[] args)
        {
            try
            {
                //
                // Parse options
                //

                Options = new AppOptions();

                Options.Parse(args);

                //
                // Enumerate devices
                //

                var devicepaths = SerialPort.GetPortNames();
                if (Options.ShowList || string.IsNullOrEmpty(Options.DeviceId))
                {
                    if (devicepaths.Length > 0)
                    {
                        Log.WriteLine("Available devices:");

                        foreach (var devicepath in devicepaths)
                        {
                            Console.WriteLine($"{devicepath}");
                        }
                    }
                    else
                    {
                        Log.WriteLine("No available devices");
                    }
                    return;
                }

                //
                // Open Device
                //

                var deviceid = devicepaths.Where(x => x.Contains(Options.DeviceId)).SingleOrDefault();
                if (null == deviceid)
                    throw new ApplicationException($"Unable to find device containing {Options.DeviceId}");

                Log.WriteLine($"Connecting to device {deviceid}...");

                using (var device = new SerialPort())
                {
                    //
                    // Configure Device
                    //

                    device.PortName = deviceid;
                    device.BaudRate = 9600;
                    device.Open();

                    //
                    // Dump device info
                    //

                    if (Options.ShowConfig)
                    {
                        Console.WriteLine("=====================================");

                        Console.WriteLine($"Parity: {device.Parity}");
                        Console.WriteLine($"Encoding: {device.Encoding}");
                        Console.WriteLine($"BaudRate: {device.BaudRate}");
                        Console.WriteLine($"DataBits: {device.DataBits}");
                        Console.WriteLine($"StopBits: {device.StopBits}");
                        Console.WriteLine($"Handshake: {device.Handshake}");

                        Console.WriteLine("=====================================");
                    }

                    //
                    // Init module client
                    //

                    if (Options.UseEdge)
                    {
                        Init().Wait();
                    }

                    if (Options.Test)
                    {
                        // Set to 16x2
                        device.Write(new byte[] { 0xFE, 0xD1, 16, 2 },0,4);
                        await Task.Delay(TimeSpan.FromMilliseconds(20));

                        // Autoscroll off
                        device.Write(new byte[] { 0xFE, 0x52 },0,2);
                        await Task.Delay(TimeSpan.FromMilliseconds(20));

                        // Turn on backlight
                        device.Write(new byte[] { 0xFE, 0x42 },0,2);
                        await Task.Delay(TimeSpan.FromMilliseconds(20));

                        // Clear screen
                        device.Write(new byte[] { 0xFE, 0x58 },0,2);
                        await Task.Delay(TimeSpan.FromMilliseconds(20));

                        // Go home
                        device.Write(new byte[] { 0xFE, 0x48 },0,2);
                        await Task.Delay(TimeSpan.FromMilliseconds(20));

                        device.WriteLine("This is a test  ");
                        device.WriteLine("1234567890123456");
                    }

                    //
                    // Set up a background thread to read from the device
                    //

                    if (Options.Receive)
                    {
                        var background = Task.Run(() => ReaderTask(device));
                    }

                    // Wait until the app unloads or is cancelled
                    if (Options.Receive)
                    {
                        var cts = new CancellationTokenSource();
                        AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
                        Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
                        WhenCancelled(cts.Token).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLineError($"{ex.GetType().Name} {ex.Message}");
            }        
        }


        private static async void ReaderTask(SerialPort device)
        {
            try
            {
                //
                // Continuously read from serial device
                // and send those values to edge hub.
                //

                int i = 1;
                while (true)
                {
                    try
                    {
                        string message = device.ReadLine();
                        Log.WriteLine($"Read {i} Completed. Received {message.Length} bytes: \"{message}\"");

                        // Translate it into a messagebody
#if EDGE
                        if (Options.UseEdge)
                        {
                            var tempData = new MessageBody();
                            tempData.SerialEncode = message;
                            tempData.TimeCreated = DateTime.Now;
                            if (tempData.isValid)
                            {
                                string dataBuffer = JsonConvert.SerializeObject(tempData); 
                                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                                Log.WriteLine($"SendEvent: [{dataBuffer}]");
                                await ioTHubModuleClient.SendEventAsync("temperatureOutput", eventMessage);                        
                            }
                        }
#else
                        await Task.Delay(1);
#endif
                    }
                    catch (TimeoutException ex) 
                    { 
                        Log.WriteLineError($"{ex.GetType().Name} {ex.Message}");
                    }

                    i++;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLineError($"{ex.GetType().Name} {ex.Message}");
            }
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Log.WriteLine($"IoT Hub module client initialized.");
        }
    }
}
