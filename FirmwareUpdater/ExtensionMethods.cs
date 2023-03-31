using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareUpdater
{
    public static class VersionExtensionMethods
    {
        public static string ToThermoVersion(this Version ver)
        {
            return string.Format("{0}.{1}.{2}.{3}", ver.Major, ver.Minor, ver.Revision, ver.Build);
        }

        public static string ToShortVersion(this Version ver)
        {
            return string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Revision);
        }
    }

}
