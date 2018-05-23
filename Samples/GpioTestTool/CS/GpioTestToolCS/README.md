# Control your GPIO lines from the command line

The Windows April 2018 Update now includes the ability to write command-line apps. In this sample, we'll show you how to create a 
command-line app to control the GPIO pins on your Windows IoT Core device.

## Usage

```
GpioTestToolCS: Command line GPIO testing utility
Usage: GpioTestToolCS.exe [-list] PinNumber

  -list         List the available pins on the default GPIO controller.
  PinNumber     The pin number with which you wish to interact. This
                parameter is required.

Commands:
 > write 0|1                        Write pin low (0) or high (1)
 > high                             Alias for 'write 1'
 > low                              Alias for 'write 0'
 > toggle                           Toggle the pin from its current state
 > read                             Read pin
 > setdrivemode drive_mode          Set the pins's drive mode
     where drive_mode = input|output|
                        inputPullUp|inputPullDown
 > interrupt on|off                 Register or unregister for pin value
                                    change events.
 > info                             Dump information about the pin
 > help                             Display this help message
 > quit                             Quit
```

## Building and running the sample

1. Clone the samples repository to your local machine.
2. Open GpioTestTool\CS\GpioTestToolCS.sln in Visual Studio.
3. Select the target architecture: ARM for Raspberry Pi or DragonBoard 410;  x86 for MinnowBoardMax
4. Go to Build -> Deploy Solution
5. SSH into your device using the DefaultAccount user. Note that command-line apps cannot currently be launched using PowerShell.
6. Run 'GpioTestToolCS.exe' from the command line

## Example session

First, you can set up an circuit with an LED and a push-button as shown in the [Push Button](https://github.com/Microsoft/Windows-iotcore-samples/blob/develop/Samples/PushButton/CS/README.md) sample.

```
C:\Data\Users\DefaultAccount>GpioTestToolCS 6
  Type 'help' for a list of commands
> read
Value: Low
> info
        Pin Number: 6
      Sharing Mode: Exclusive
  Debounce Timeout: 0 ms
        Drive Mode: InputPullDown
             Value: Low
> mode Output
> write 1
> write 0
> quit
```

Notice that when you write 1 or 0, the LED turns on or off. 

Now, you can set an interrupt that will fire when you press and release the button:

```
C:\Data\Users\DefaultAccount>GpioTestToolCS 5
> info
        Pin Number: 5
      Sharing Mode: Exclusive
  Debounce Timeout: 0 ms
        Drive Mode: InputPullUp
             Value: High
> int on
> Pin 5 Interrupt! Edge: FallingEdge
> Pin 5 Interrupt! Edge: RisingEdge
> quit
```

Notice that when you press and release the button, the interrupt fires, showing the rising/falling edge.

## Additional resources
* [Windows 10 IoT Core home page](https://developer.microsoft.com/en-us/windows/iot/)
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
