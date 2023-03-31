using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;

namespace AccessLevelKeyGenerator
{
    public static class UsbDeviceUtils
    {
        public static event EventHandler DeviceDetectedEvent;

        private static ManagementEventWatcher watcher;

        static UsbDeviceUtils()
        {
            watcher = new ManagementEventWatcher();
            // Add a check for EventType = 3 too if you want to detect device loss as well as found
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");
            watcher.EventArrived += new EventArrivedEventHandler(Detected);
            watcher.Query = query;
        }

        public static void StartDetecting()
        {
            watcher.Start();
        }

        public static void StopDetecting()
        {
            watcher.Stop();
        }

        private static void Detected(object sender, EventArgs e)
        {
            var safeHandler = DeviceDetectedEvent;
            if (null != safeHandler)
            {
                safeHandler(null, EventArgs.Empty);
            }
        }



        public static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            //Find usb disks
            String qry = String.Format("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");
            ManagementObjectSearcher disks = new ManagementObjectSearcher(qry);
            var res = disks.Get();
            foreach (ManagementObject disk in res)
            {
                disk.Get();
                //Get partitions for disk physical disk
                string id = disk.ToString();
                string partitionQueryStr = "ASSOCIATORS of {" + id + "} where ResultClass=Win32_DiskPartition";
                ManagementObjectSearcher partitionQuery = new ManagementObjectSearcher(partitionQueryStr);
                string pnp1 = disk["PnPDeviceId"].ToString();
                var partitions = partitionQuery.Get();
                foreach (var partition in partitions)
                {
                    //Now get logical disks for partitition
                    string logDiskQryStr = "ASSOCIATORS of {" + partition.ToString() + "} where ResultClass= Win32_LogicalDisk";
                    ManagementObjectSearcher logDiskQry = new ManagementObjectSearcher(logDiskQryStr);
                    var logicalDisks = logDiskQry.Get();
                    foreach (var logicalDisk in logicalDisks)
                    {
                        string driveLetter = logicalDisk["DeviceId"].ToString();
                        string driveSerial = parseSerialFromDeviceID(disk["PNPDeviceID"].ToString());
                        string volumeSerial = logicalDisk["VolumeSerialNumber"].ToString();

                        DriveInfo c = new DriveInfo(driveLetter);
                        double availableSpaceInMb = (double)c.AvailableFreeSpace / 1024 / 1024;
                        double totalSpaceInMb = (double)c.TotalSize / 1024 / 1024;

                        devices.Add(new USBDeviceInfo(driveLetter, driveSerial, volumeSerial, availableSpaceInMb, totalSpaceInMb));
                    }
                }
            }
            return devices;
        }


        public static string GetUsbSerial(string driveLetter)
        {
            string result = string.Empty;
            var usbDevices = UsbDeviceUtils.GetUSBDevices();
            var matchingUsb = usbDevices.Where(a => a.DriveLetter.ToUpper().Equals(driveLetter)).FirstOrDefault();
            if (null != matchingUsb)
            {
                result = matchingUsb.VolumeSerialNumber;
            }
            return result;
        }

        public static string GetUsbPnPDeviceId(string driveLetter)
        {
            string result = string.Empty;
            var usbDevices = UsbDeviceUtils.GetUSBDevices();
            var matchingUsb = usbDevices.Where(a => a.DriveLetter.ToUpper().Equals(driveLetter)).FirstOrDefault();
            if (null != matchingUsb)
            {
                result = matchingUsb.PnpDeviceID;
            }
            return result;
        }


        private static string getValueInQuotes(string inValue)
        {
            string parsedValue = "";

            int posFoundStart = 0;
            int posFoundEnd = 0;

            posFoundStart = inValue.IndexOf("\"");
            posFoundEnd = inValue.IndexOf("\"", posFoundStart + 1);

            parsedValue = inValue.Substring(posFoundStart + 1, (posFoundEnd - posFoundStart) - 1);

            return parsedValue;
        }

        private static string parseSerialFromDeviceID(string deviceId)
        {
            string[] splitDeviceId = deviceId.Split('\\');
            string[] serialArray;
            string serial;
            int arrayLen = splitDeviceId.Length - 1;

            serialArray = splitDeviceId[arrayLen].Split('&');
            serial = serialArray[0];

            return serial;
        }
    }

    public class USBDeviceInfo
    {
        public USBDeviceInfo(string driveLetter, string hardSerialNumber, string volumeSerialNumber, double freeSpaceMb, double totalSpaceMb)
        {
            this.DriveLetter = driveLetter;
            this.PnpDeviceID = hardSerialNumber;
            this.VolumeSerialNumber = volumeSerialNumber;
            this.FreeSpaceMb = freeSpaceMb;
            this.TotalSizeMb = totalSpaceMb;
        }
        public string DriveLetter { get; private set; }
        public string PnpDeviceID { get; private set; }
        public string VolumeSerialNumber { get; private set; }
        public double FreeSpaceMb { get; private set; }
        public double TotalSizeMb { get; private set; }
    }
}
