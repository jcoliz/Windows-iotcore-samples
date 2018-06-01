# Test your I2C devices from the command line

The Windows Spring 2018 Update now includes the ability to write command-line apps. In this sample, we'll show you how to create a 
command-line app to test communication with I2C devices. This can be a handy debugging aid to instantly test I2C commands without
having to wait for a compile-deploy-run loop.

## Usage

```
I2cTestToolCS: Command line I2C testing utility
Usage: I2cTestToolCS.exe [-list] SlaveAddress [FriendlyName]

  -list          List available I2C controllers and exit.
  SlaveAddress   The slave address of the device with which you
                 wish to communicate. This is a required parameter.
  FriendlyName   The friendly name of the I2C controller over
                 which you wish to communicate. This parameter is
                 optional and defaults to the first enumerated
                 I2C controller.

Examples:
  List available I2C controllers and exit:
    I2cTestToolCS.exe -list

  Open connection on the first enumerated controller to slave address 0x57:
    I2cTestToolCS.exe 0x57

  Open connection on I2C1 to slave address 0x57:
    I2cTestToolCS.exe 0x57 I2C1

Commands:
 > write { 00 11 22 .. FF }         Write bytes to the device
 > read N                           Read N bytes
 > writeread { 00 11 .. FF } N      Write bytes, restart, read N bytes
 > info                             Display device information
 > help                             Display this help message
 > quit                             Quit
```

## Building and running the sample

1. Clone the samples repository to your local machine.
2. Open I2CTestTool\CS\I2CTestToolCS.sln in Visual Studio.
3. Select the target architecture: ARM for Raspberry Pi or DragonBoard 410;  x86 for MinnowBoardMax
4. Go to Build -> Deploy Solution
5. SSH into your device using the DefaultAccount user. Note that command-line apps cannot currently be launched using PowerShell.
6. Run 'I2CTestToolCS.exe' from the command line


## Example session

First, you can set up an circuit with an I2C device as shown in the [I2C Port Expander](https://github.com/Microsoft/Windows-iotcore-samples/tree/develop/Samples/I2cPortExpander) sample.

Launch the tool, check the device

```
C:\Data\Users\DefaultAccount>I2CTestTool.exe 0x20
    Type 'help' for a list of commands
  > info
         DeviceId: \\?\ACPI#MSFT8000#1#{a11ee3c6-8421-4202-a3e7-b91ff90188e4}\I2C5
    Slave address: 0x20
        Bus Speed: StandardMode (100Khz)        
```

Get the initial values of IODIR, GPIO, and OLAT registers

```
  > writeread { 0 } 1
   ff
  > writeread { 9 } 1
   0
  > writeread { a } 1
   0
```

Configure the LED pin output to be logic high, leave the other pins as they are. We are using GPIO pin 0 on the port expander for the LED.

```
  > write { a 1 }
```

Configure only the LED pin to be an output and leave the other pins as they are. Input is logic low, output is logic high
Set the LED GPIO pin mask bit to '0', all other bits to '1'

```
  > write { 0 fe }
```

Turn on the LED

```
  > write { a 0 }
```

Turn off the LED

```
  > write { a 1 }
```

Now check the status of the button. You can try this with the button pressed or released to see the difference.

```
  > writeread { 9 } 1
   1
  > writeread { 9 } 1
   0
```

## Additional resources
* [Windows 10 IoT Core home page](https://developer.microsoft.com/en-us/windows/iot/)
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
