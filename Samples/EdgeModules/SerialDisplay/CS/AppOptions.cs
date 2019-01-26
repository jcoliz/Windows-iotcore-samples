using Mono.Options;
using System;
using System.Collections.Generic;

namespace SampleModule
{
    public class AppOptions: OptionSet
    {
        public bool Help { get; private set; }
        public bool ShowList { get; private set; }
        public bool ShowConfig { get; private set; }
        public bool Receive { get; private set; }
        public string DeviceId { get; private set; }
        public bool UseEdge { get; private set; }
        public bool Exit { get; private set; } = false;
        public string[] Lines { get; } = new string[] { string.Empty, string.Empty };
        
        public AppOptions()
        {
            Add( "h|help", "show this message and exit", v => Help = v != null );
            Add( "l|list", "list available devices and exit", v => ShowList = v != null);
            Add( "d|device=", "the {ID} of device to connect", v => DeviceId = v);
            Add( "r|receive", "receive and display messages", v => Receive = v != null);
            Add( "c|config", "display device configuration", v => ShowConfig = v != null);
            Add( "e|edge", "transmit through azure edge", v => UseEdge = v != null);
            Add( "1|line1=", "the {TEXT} to display on line 1", v => Lines[0] = v);
            Add( "2|line2=", "the {TEXT} to display on line 2", v => Lines[1] = v);
        }

        public new List<string> Parse(IEnumerable<string> args)
        {
            var result = base.Parse(args);

            if (Help || !(Receive || ShowList || ShowConfig || !String.IsNullOrEmpty(Lines[0])))
            {
                Console.WriteLine($"{AppName} {AppVersion}");
                WriteOptionDescriptions(Console.Out);
                Exit = true;
            }

            return result;
        }

        static private string AppName => typeof(AppOptions).Assembly.GetName().Name;
        static private string AppVersion => typeof(AppOptions).Assembly.GetName().Version.ToString();
    }
}
