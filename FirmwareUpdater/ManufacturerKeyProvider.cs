using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UsbEncryptionUtils;
using AccessLevelKeyGenerator;
using System.IO;

namespace FirmwareUpdater
{
    /// <summary>
    /// Class to provide a list of manufacturer passwords from USB key
    /// </summary>
    class ManufacturerKeyProvider
    {
        const string c_keyFileName = "permissions.key";
        List<byte[]> m_keys;
        bool m_started = false;
        static volatile ManufacturerKeyProvider s_instance;
        static object sync = new object();
        
        protected ManufacturerKeyProvider()
        {
            m_keys = new List<byte[]>();
        }

        ~ManufacturerKeyProvider()
        {
            if (m_started) Stop();
        }

        public static ManufacturerKeyProvider Instance
        {
            get
            {
                if (null == s_instance)
                    lock (sync)
                    {
                        if (null == s_instance)
                        {
                            s_instance = new ManufacturerKeyProvider();
                            s_instance.Start();
                        }
                    }
                return s_instance;
            }
        }


        public void Start()
        {
            if (m_started) return;
            m_keys.Clear();
            m_started = true;
            UsbDeviceUtils.DeviceDetectedEvent += DeviceDetectedHandler;
            UsbDeviceUtils.StartDetecting();

            // Run one read of the USB sticks now in case we have one already plugged in on startup.
            DeviceDetectedHandler(this, EventArgs.Empty);
        }

        public void Stop()
        {
            UsbDeviceUtils.StopDetecting();
            UsbDeviceUtils.DeviceDetectedEvent -= DeviceDetectedHandler;
            m_started = false;
        }

        public IEnumerable<byte[]> ManufacturerKeys
        {
            get
            {
                return new List<byte[]>(m_keys);
            }
        }



        private void DeviceDetectedHandler(object sender, EventArgs e)
        {
            // A USB device was detected = as we don't get informed which one, get the details of all of them
            var devices = UsbDeviceUtils.GetUSBDevices();

            byte[] fileBytes = null;
            string deviceId = string.Empty;
            bool foundSomething = false;
            string foundFilePath = string.Empty;


            // check each stick for a file of the correct name / format
            foreach (var item in devices)
            {
                string filePath = Path.Combine(item.DriveLetter, c_keyFileName);
                // find file list on driveletter root
                if (File.Exists(filePath))
                {
                    fileBytes = File.ReadAllBytes(filePath);
                    deviceId = item.PnpDeviceID;
                    foundFilePath = filePath;
                    foundSomething = true;
                    break;
                }

            }

            // We found something in the place we were hoping to file it - now decrypt & use it
            if (foundSomething)
            {
                byte[] passPhrases; //array of bytes
                string userName;
                DateTime createdDateTime;

                try
                {
                    // Attempt to decrypt the file, and log / store the passPhrases if all goes well
                    KeyDecrypter.DecryptKeyData(fileBytes, deviceId, out passPhrases, out userName, out createdDateTime);
                    //OK now split and parse the passphrases content
                    m_keys = ExtractKeysInDateOrder(passPhrases);
                    //CalLogger.Instance().Info(new LogData() { Component = LoggingComponent.CalibrationDll, EventCode = CalApiEventCodes.ManufacturerKeysDecodedOK, Message = InterfaceStrings.ManufacturerKeysDecoded });
                }
                catch (System.Security.Cryptography.CryptographicException)
                {
                    // Log cryptographic error - usually indicating the deviceId is incorrect (ie: a copied key)
                    //CalLogger.Instance().Error(new LogData() { Component = LoggingComponent.CalibrationDll, EventCode = CalApiEventCodes.ManufacturerKeysDecodeError, Message = InterfaceStrings.ManufacturerKeysDecodeError });
                }
                catch (Exception )
                {
                    // Log error - not expecting any other exception type here.
                   // CalLogger.Instance().Error(new LogData() { Component = LoggingComponent.CalibrationDll, EventCode = CalApiEventCodes.ManufacturerKeysUnexpectedError, Message = InterfaceStrings.ManufacturerKeysUnexpectedError });
                }
            }

        }


        private List<byte[]> ExtractKeysInDateOrder(byte[] p)
        {
            string encodedString = System.Text.UTF8Encoding.UTF8.GetString(p);
            var splitted = encodedString.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            SortedDictionary<DateTime, byte[]> keys = new SortedDictionary<DateTime, byte[]>();
            foreach (var item in splitted)
            {
                try
                {
                    var parts = item.Split('~');
                    var dateTime = DateTime.Parse(parts[1]);

                    byte[] data = Array.ConvertAll<string, byte>(parts[0].Split('-'), s => Convert.ToByte(s, 16));

                    keys[dateTime] = data;
                }
                catch
                {
                    // Catch all here on purpose - if it fails we can simply log and move on with the next one
                    //CalLogger.Instance().Warning(new LogData() { Component = LoggingComponent.CalibrationDll, EventCode = CalApiEventCodes.ManufacturerKeyParseError, Message = InterfaceStrings.ManufacturerKeyParseError });

                }
            }
            // The SortedDictionary will put them in chronological order - we want the reverse of that here.
            return keys.Values.Reverse().ToList();
        }


    }
}
