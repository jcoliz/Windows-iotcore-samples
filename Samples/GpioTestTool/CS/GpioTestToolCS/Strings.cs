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
$@"{Program.Name}: Command line GPIO testing utility in C#
Usage: {Program.Name} [-list] PinNumber

    -list         List the available pins on the default GPIO controller.
    PinNumber     The pin number with which you wish to interact. This
                    parameter is required.

Example:
    {Program.Name} -list
    {Program.Name} 47";

        public const string Help =
@"Commands:
> write 0|1                        Write pin low (0) or high (1)
> high                             Alias for 'write 1'
> low                              Alias for 'write 0'
> toggle                           Toggle the pin from its current state
> read                             Read pin
> setdrivemode drive_mode          Set the pins's drive mode
     where drive_mode = Input|Output|InputPullUp|InputPullDown
> interrupt on|off                 Register or unregister for pin value
                                    change events.
> info                             Dump information about the pin
> help                             Display this help message
> quit                             Quit";
    }
}
