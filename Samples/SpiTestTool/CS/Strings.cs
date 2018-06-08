using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiTestToolCS
{
    static class Strings
    {
        public static string Usage =
$@"{Program.Name}: Command line SPI testing utility
Usage: {Program.Name} [-list] [-n FriendlyName] [-c ChipSelectLine] [-m Mode]
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
    {Program.Name}

  List available SPI controllers and exit:
    {Program.Name} -list

  Connect to SPI1 in mode 2, with default speed (4Mhz) and chip
  select line (0):
    {Program.Name} -n SPI1 -m 2

  Connect to chip select 1 on SPI1 in mode 2 at 1Mhz:
    {Program.Name} -c 1 -n SPI1 -m 2 -f 1000000,
";
        public const string Help =

@"Commands:
 > write { 00 11 22 .. FF }         Write bytes to device
 > read N                           Read N bytes
 > writeread { 00 11 .. FF } N      Write bytes then read N bytes
 > fullduplex { 00 11 .. FF } N     Perform full duplex transfer, reading N bytes
 > info                             Display device information
 > help                             Display this help message
 > quit                             Quit
";
    }
}
