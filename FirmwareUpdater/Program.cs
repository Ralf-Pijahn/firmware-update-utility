using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirmwareUpdater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                FrmMain frm = new FrmMain(args);
                Application.Run(frm);
                return frm.ResultCode;
            }
            catch(CommandLineArgsException ex)
            {
                return ex.ResultCode;
            }
        }
    }
}
