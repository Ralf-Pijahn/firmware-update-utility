using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUpdater
{
    public static class ResultCodes
    {
        public const int OK = 0;
        public const int BadOptions = 1;
        public const int BadFirmwareFile = 2;
        public const int BadComPort = 3;
        public const int ProgramingFailed = 4;

    }
}
