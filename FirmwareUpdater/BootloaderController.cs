using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FirmwareUpdater
{
    class BootloaderController
    {
        SerialPort m_port;
        public event EventHandler<EventArgs<bool>> BootloaderDetected;

        public BootloaderController(SerialPort port)
        {
            m_port = port;
        }


        public void DetectBootloader(TimeSpan timeout, bool bIsNewMk3Firmware)
        {
            if (bIsNewMk3Firmware)
            {
                OnBootloaderDetected(true);
            }
            else
            {
                Task detect = new Task(() =>
                {
                    var endtime = DateTime.Now + timeout;
                    bool result = false;
                    while (DateTime.Now < endtime)
                    {
                        try
                        {
                            string s = m_port.ReadLine();
                            System.Diagnostics.Debug.WriteLine(s);
                            if (s.Contains("r - reset"))
                            {
                                result = true;
                                break;
                            }
                        }
                        catch (TimeoutException)
                        {
                            m_port.Write("h");
                        }
                    }
                    OnBootloaderDetected(result);
                });
                detect.Start();
            }
        }

        public void StartUpgrade()
        {
            m_port.Write("u");
        }

        public void Reset()
        {
            for (int i = 0; i < 5; i++)
            {
                m_port.Write("r");
                System.Threading.Thread.Sleep(50);
            }
        }


        private void OnBootloaderDetected(bool result)
        {
            var safeHandler = BootloaderDetected;
            if(null != safeHandler)
            {
                safeHandler(this, new EventArgs<bool>(result));
            }
        }

    }


}
