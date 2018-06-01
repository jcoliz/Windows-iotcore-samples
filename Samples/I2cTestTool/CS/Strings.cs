using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GpioTestTool
{
    static class Strings
    {
        public static string Usage =
$@"{Program.Name}: Command line I2C testing utility
Usage: {Program.Name} [-list] SlaveAddress [FriendlyName]

  -list          List available I2C controllers and exit.
  SlaveAddress   The slave address of the device with which you
                 wish to communicate. This is a required parameter.
  FriendlyName   The friendly name of the I2C controller over
                 which you wish to communicate. This parameter is
                 optional and defaults to the first enumerated
                 I2C controller.

Examples:
  List available I2C controllers and exit:
    {Program.Name} -list

  Open connection on the first enumerated controller to slave address 0x57:
    {Program.Name} 0x57

  Open connection on I2C1 to slave address 0x57:
    {Program.Name} 0x57 I2C1";

        public const string Help =
@"Commands:
 > write { 00 11 22 .. FF }         Write bytes to device
 > read N                           Read N bytes
 > writeread { 00 11 .. FF } N      Write bytes, restart, read N bytes
 > info                             Display device information
 > help                             Display this help message
 > quit                             Quit
";
    }
}
