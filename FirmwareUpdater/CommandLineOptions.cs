using CommandLine;
using CommandLine.Text;

namespace FirmwareUpdater
{
    class CommandLineOptions
    {
        [Option('f',"filename",Required=false, HelpText="Encrypted Firmware Filename and Path")]
        public string FirmwareFileName { get; set; }

        [Option('p',"port",Required=false, HelpText="Com Port Name e.g. Com1")]
        public string ComPortName { get; set; }

        [Option('r', "run", Required = false, HelpText = "Run Programming On Discovery (Single Device)")]
        public bool Run { get; set; }

        [Option('a', "access level", Required = false, HelpText = "Access Level (M=Manufacturer, A=Administrator, R=Regulator, O=Open) - must provide passphrase")]
        public char AccessLevel { get; set; }

        [Option('s', "passphrase", Required = false, HelpText = "Passphrase - must have provided an Access Level")]
        public string Passphrase { get; set; }

        [Option('h', "help", Required = false, HelpText = "Display Command Line Parameters")]
        public bool NeedsHelp { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current)=>HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
