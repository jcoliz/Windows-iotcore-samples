# Test your SPI devices from the command line

The Windows April 2018 Update now includes the ability to write command-line apps. In this sample, we'll show you how to create a 
command-line app to test communication with SPI devices. This can be a handy debugging aid to instantly test SPI commands without
having to wait for a compile-deploy-run loop.

## Usage

```
SpiTestToolCS: Command line SPI testing utility
Usage: SpiTestToolCS [-list] [-n FriendlyName] [-c ChipSelectLine] [-m Mode]
[-d DataBitLength] [-f ClockFrequency]

  -list           List available SPI controllers and exit.
  FriendlyName    The friendly name of the SPI controller over which
                  you wish to communicate. This parameter is
                  optional and defaults to the first enumerated SPI
                  controller.
  ChipSelectLine  The chip select line to use. This parameter is
                  optional and defaults to 0.
  Mode            The SPI mode to use (0-3). This parameter is
                  optional and defaults to Mode 0.
  DataBitLength   The data bit length to use. This parameter is optional
                  and defaults to 8.
  ClockFrequency  The SPI clock frequency to use. This parameter is
                  optional and defaults to 4Mhz.

Examples:
  Connect to the first SPI controller found with default settings
  (ChipSelectLine=0, Mode=0, DataBitLength=8, Frequency=4Mhz):
    SpiTestToolCS

  List available SPI controllers and exit:
    SpiTestToolCS -list

  Connect to SPI1 in mode 2, with default speed (4Mhz) and chip
  select line (0):
    SpiTestToolCS -n SPI1 -m 2

  Connect to chip select 1 on SPI1 in mode 2 at 1Mhz:
    SpiTestToolCS -c 1 -n SPI1 -m 2 -f 1000000

Commands:
 > write { 00 11 22 .. FF }         Write bytes to device
 > read N                           Read N bytes
 > writeread { 00 11 .. FF } N      Write bytes then read N bytes
 > fullduplex { 00 11 .. FF } N     Perform full duplex transfer, reading N bytes
 > info                             Display device information
 > help                             Display this help message
 > quit                             Quit

```

## Building and running the sample

1. Clone the samples repository to your local machine.
2. Open SPITestTool\CS\SPITestToolCS.sln in Visual Studio.
3. Select the target architecture: ARM for Raspberry Pi or DragonBoard 410;  x86 for MinnowBoardMax
4. Go to Build -> Deploy Solution
5. SSH into your device using the DefaultAccount user. Note that command-line apps cannot currently be launched using PowerShell.
6. Run 'SPITestToolCS.exe' from the command line

## Example session

First, you can set up an circuit with a SPI device as shown in the [Temperature and force sensor](https://github.com/Microsoft/Windows-iotcore-samples/tree/develop/Samples/TempForceSensor/CS) sample.

Launch the tool, check the device

```
C:\Data\Users\DefaultAccount>SPITestTool.exe -f 500000
> info
         Device Id: \\?\ACPI#MSFT8000#1#{a11ee3c6-8421-4202-a3e7-b91ff90188e4}\SPI0
  Chip Select Line: 0
              Mode: Mode0
   Data Bit Length: 8
   Clock Frequency: 500000
      Sharing Mode: Exclusive
```

Configure the device, and take readings, using Full Duplex mode. This example is for an MCP3208. Check the sample code described above for other possible parameters.

```
> fullduplex { 06 00 00 } 3
 00 0c 71
```

## Additional resources
* [Windows 10 IoT Core home page](https://developer.microsoft.com/en-us/windows/iot/)
* [Documentation for all samples](https://developer.microsoft.com/en-us/windows/iot/samples)

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact <opencode@microsoft.com> with any additional questions or comments.
