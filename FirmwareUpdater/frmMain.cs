using Reader3Managed;
using Reader3Managed.Common;
using Reader3Managed.Mk3;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using GLib;
using Thread = System.Threading.Thread;

namespace FirmwareUpdater
{
    public partial class FrmMain : Form
    {
        enum UpdateState {Idle, Discovering, Connected, Disconnecting, Updating }
        SerialPort port;
        XMODEM_FullDotNET xmodem;
        BootloaderController controller;
        IEpdStack stack;
        IEpdMk3Interface epd = null;
        string m_portName;
        UpdateState currentState = UpdateState.Idle;
        bool m_autorun = false;
        string m_autorun_port;
        string m_autorun_file;
        AccessLevel cmdLineAccesslevel;
        AccessLevel m_Accesslevel = AccessLevel.Manufacturer;
        string cmdLinePassphrase = null;

        // To update Mk3.1 Firmware
        bool newMk3Firmware = false;
        byte EraseApplication = 2;
        byte[] TxD = new byte[1100];
        byte[] BinData = new byte[256 * 1024];
        uint AddressOffset = 0xFFFC0000;
        uint ApplicationEnd = 0xFFFFF800;
        uint ApplicationStart = 0xFFFC2800;
        byte DoReset = 3;


        readonly byte[][] DefaultMfgrKeys = new byte[][]{
            new byte[]{0x44,0x80,0xa9,0x17,0x4f,0x50,0xba,0x38,0x97,0x54,0x99,0x2b,0x77,0xf1,0xdc,0xd6,0x4a,0xb9,0x60,0x2b,0x30,0xe0,0x5a,0x1e,0x8d,0x1a,0xe8,0xc0,0x14,0xd8,0xe8,0xb7}, //correct (V1.2 & later)
            new byte[]{0x04,0x89,0x5e,0xab,0x72,0x3a,0xe4,0xee,0xc0,0x4a,0x50,0x99,0x26,0x46,0xcf,0x34,0x23,0x64,0x72,0x02,0xaf,0x73,0x28,0x0b,0xbb,0x50,0xe0,0x07,0xcb,0x9e,0xaf,0xfa} //buggy (V1.2 & earlier)
        };


        public FrmMain(string[] args)
        {
            InitializeComponent();
            ResultCode = 0;
            if (!ProcessArgs(args)) throw new CommandLineArgsException(this.ResultCode);

            epdImg.Image = imageListEpds.Images[0];

            stack = StackFactory.CreateInterface();
            stack.EpdsDiscovered += HandleDiscoveredEpds;

            port = new SerialPort();
            port.BaudRate = 115200;
            port.ReadTimeout = 100;

            ddlPort.Items.AddRange(SerialPort.GetPortNames());

            
            controller = new BootloaderController(port);

            xmodem = new XMODEM_FullDotNET(port, XMODEM_FullDotNET.Variants.XModemCRC);
            xmodem.SendInactivityTimeoutMillisec = 10000;
            xmodem.MaxSenderRetries = 3;
            xmodem.SenderPacketRetryTimeoutMillisec = 2000;
            xmodem.TransmitProgress += xmodem_TransmitProgress;

            ManufacturerKeyProvider.Instance.Start();
        }


        public int ResultCode { get; private set; }

        private bool ProcessArgs(string[] args)
        {
            ResultCode = ResultCodes.OK;
            if (args.Length > 0)
            {
                bool gotCmdLineOptions;

                CommandLineOptions options = new CommandLineOptions();
                gotCmdLineOptions = CommandLine.Parser.Default.ParseArguments(args, options);
                if (!gotCmdLineOptions || options.NeedsHelp)
                {
                    Console.WriteLine(options.GetUsage());
                    ResultCode = ResultCodes.BadOptions;
                }
                else
                {
                    switch (options.AccessLevel)
                    {
                        case 'M':
                            cmdLineAccesslevel = AccessLevel.Manufacturer;
                            break;
                        case 'A':
                            cmdLineAccesslevel = AccessLevel.Admin;
                            break;
                        case 'R':
                            cmdLineAccesslevel = AccessLevel.Regulator;
                            break;
                        case 'O':
                            cmdLineAccesslevel = AccessLevel.Open;
                            break;
                        default:
                            cmdLineAccesslevel = AccessLevel.Manufacturer;
                            break;
                    }
                            
                    cbAccessLevel.Text = cmdLineAccesslevel.ToString();

                    tbPassphrase.Text = options.Passphrase;

                    m_autorun = options.Run;

                    if (options.ComPortName == null)
                        m_autorun = false;
                    else
                    {
                        m_autorun_port = SerialPort.GetPortNames().FirstOrDefault(x => x.ToUpper() == options.ComPortName.ToUpper());
                        ddlPort.Text = m_autorun_port;

                        if (m_autorun_port == null)
                        {
                            if (m_autorun) ResultCode = ResultCodes.BadComPort;
                            m_autorun = false;
                        }
                    }


                    if (options.FirmwareFileName == null)
                        m_autorun = false;
                    else
                    {
                        if (File.Exists(options.FirmwareFileName))
                        {
                            m_autorun_file = options.FirmwareFileName;
                            edtFileName.Text = m_autorun_file;
                        }
                        else
                        {
                            if (m_autorun) ResultCode = ResultCodes.BadFirmwareFile;
                            m_autorun = false;
                        }
                    }
                }
            }
            return ResultCodes.OK==ResultCode;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            stack.EpdsDiscovered -= HandleDiscoveredEpds;
            ShutdownStack(false);
            ManufacturerKeyProvider.Instance.Stop();
        }

        private void HandleDiscoveredEpds(object sender, DiscoveryEventArgs e)
        {
            epdImg.Image = imageListEpds.Images[0];
            if (e.DiscoveredEpds.Count > 0)
            {
                epd = stack.CreateEpd(e.DiscoveredEpds[0]) as IEpdMk3Interface;
                epd.Connected +=epd_Connected;
                epd.Disconnected += epd_Disconnected;
                epd.Connect(false);
                currentState = UpdateState.Connected;
            }
        }

        void epd_Disconnected(object sender, ConnectEventArgs e)
        {
            epdImg.Image = imageListEpds.Images[0];
            stack.ReleaseEpd(epd);
            if (currentState != UpdateState.Updating)
            {
                stack.StartDiscovery();
                currentState = UpdateState.Discovering;
            }
        }

        void epd_Connected(object sender, ConnectEventArgs e)
        {
            epdImg.Image = imageListEpds.Images[1];
            currentState = UpdateState.Connected;
            var token = epd.BeginReadEpdIdentities();
            epd.Commit();

            uint epdId;
            EpdType epdType;
            uint capabilities;
            Version markNumber;
            Version firmwareVersion;
            string vcsRev;

            epd.EndReadEpdIdentities(token, out epdId, out epdType, out capabilities, out markNumber, out firmwareVersion, out vcsRev);

            if (markNumber.Major == 3)
            {
                newMk3Firmware = false;

                if (markNumber.Minor >= 1)
                {
                    newMk3Firmware = true;
                }
            }

            UpdateEpdIdentities(firmwareVersion);
            if(m_autorun)
            {
                this.Invoke((MethodInvoker)(() => this.btnUpdate_Click(null, new EventArgs())));
                tmrCmdLineFallback.Enabled = false;

            }
        }

        private void UpdateEpdIdentities(Version firmwareVersion)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateEpdIdentities(firmwareVersion)));
            }
            else
            {
                lblFwVersion.Text = firmwareVersion.ToThermoVersion();
            }
        }

        private void ddlPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_portName = ddlPort.SelectedItem.ToString();
            CreateStackStartDiscovery();
        }

        private void CreateStackStartDiscovery()
        {
            if (null != stack)
            {
                if (null != epd) stack.ReleaseEpd(epd);
                stack.Close();
                stack.Open(m_portName);
                stack.StartDiscovery();
                currentState = UpdateState.Discovering;
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(edtFileName.Text)) openFileDialog.FileName = edtFileName.Text;
            if(DialogResult.OK == openFileDialog.ShowDialog())
            {
                edtFileName.Text = openFileDialog.FileName;
                edtFileName.ScrollBars = ScrollBars.Horizontal;
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!File.Exists(edtFileName.Text))
            {
                MessageBox.Show(local.NoFirmwareFileSelected, local.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (ShutdownStack(true))
                {
                    Thread.Sleep(200);
                    if (port.IsOpen) port.Close();
                    port.PortName = m_portName;
                    port.Open();
                    lblStatusValue.Text = local.DetectingBootloader;
                    controller.BootloaderDetected += BootloaderDetected;
                    controller.DetectBootloader(TimeSpan.FromSeconds(10), newMk3Firmware);
                }
            }
        }

        private bool CheckAuthorizedToUpdateFirmware(AccessLevel theAccessLevel)
        {
            bool authorizedToUpdateFirmware = false;

            try
            {
                // Check if autorized at access level logged in

                var token = epd.BeginReadAccessPermissionsForLevel(theAccessLevel, null);
                List<CommandIdentifier> commandIds = null;
                epd.Commit();
                epd.EndReadAccessPermissionsForLevel(token, out commandIds);

                foreach (var item in commandIds)
                {
                    if (item == CommandIdentifier.InitiateFirmwareUpdate)
                    {
                        authorizedToUpdateFirmware = true;
                        break;
                    }
                }

                // If not authorized in logged in access level then check lower levels.
                if (!authorizedToUpdateFirmware)
                {
                    if (theAccessLevel == AccessLevel.Open)
                        return authorizedToUpdateFirmware;

                    // The Access Level is higher than Open so check if Authorized at Open level
                    token = epd.BeginReadAccessPermissionsForLevel(AccessLevel.Open, null);
                    List<CommandIdentifier> openCommandIds = null;
                    epd.Commit();
                    epd.EndReadAccessPermissionsForLevel(token, out openCommandIds);

                    foreach (var item in openCommandIds)
                    {
                        if (item == CommandIdentifier.InitiateFirmwareUpdate)
                        {
                            authorizedToUpdateFirmware = true;
                            break;
                        }
                    }

                    if (!authorizedToUpdateFirmware)
                    {
                        if (theAccessLevel == AccessLevel.Admin)
                            return authorizedToUpdateFirmware;

                        // The Access Level is higher than Admin so check if Authorized at Admin level
                        token = epd.BeginReadAccessPermissionsForLevel(AccessLevel.Admin, null);
                        List<CommandIdentifier> adminCommandIds = null;
                        epd.Commit();
                        epd.EndReadAccessPermissionsForLevel(token, out adminCommandIds);

                        foreach (var item in adminCommandIds)
                        {
                            if (item == CommandIdentifier.InitiateFirmwareUpdate)
                            {
                                authorizedToUpdateFirmware = true;
                                break;
                            }
                        }

                        if (!authorizedToUpdateFirmware)
                        {
                            if (theAccessLevel == AccessLevel.Regulator)
                                return authorizedToUpdateFirmware;

                            // The Access Level is higher than Regulator so check if Authorized at Regulator level
                            token = epd.BeginReadAccessPermissionsForLevel(AccessLevel.Regulator, null);
                            List<CommandIdentifier> regulatorCommandIds = null;
                            epd.Commit();
                            epd.EndReadAccessPermissionsForLevel(token, out regulatorCommandIds);

                            foreach (var item in regulatorCommandIds)
                            {
                                if (item == CommandIdentifier.InitiateFirmwareUpdate)
                                {
                                    authorizedToUpdateFirmware = true;
                                    break;
                                }
                            }

                            if (!authorizedToUpdateFirmware)
                            {
                                if (theAccessLevel == AccessLevel.Manufacturer)
                                    return authorizedToUpdateFirmware;

                            } // Not authorized for Maufacturer

                        } // Not authorized for Regulator

                    } // Not authorized for Admin

                } // Not authorized for Open


            }
            catch (EpdException ex)
            {
                if (ex.ErrorCode != DllReturnCode.InsufficientPriv) return false;
            }
            
            return authorizedToUpdateFirmware;

        }

        private bool ShutdownStack(bool startUpdate)
        {
            bool useDebugRegisterMethod = false;

            if (currentState == UpdateState.Connected)
            {
                if (startUpdate)
                {
                    lblStatusValue.Text = "";

                    // if a command line or on screen passphrase was provided login using 
                    // the set access level and passphrase.
                    // No longer use the Manufacturer USB dongle login.
                    if (tbPassphrase.Text != null)
                    {
                        try
                        {
                            var token = epd.BeginWriteAccessLevel(m_Accesslevel, tbPassphrase.Text, null);
                            epd.Commit();
                            epd.EndWriteAccessLevel(token);
                        }
                        catch (EpdException ex)
                        {
                            lblStatusValue.Text = local.InsufficientPrivilege;
                            return false;
                        }

                        bool authorizedToUpdateFirmware = false;

                        authorizedToUpdateFirmware = CheckAuthorizedToUpdateFirmware(m_Accesslevel);

                        if (!authorizedToUpdateFirmware)
                        {
                            lblStatusValue.Text = local.InsufficientPrivilege;
                            return false;
                        }


                    }
                    //else
                    //{
                    //    cbAccessLevel.Text = AccessLevel.Manufacturer.ToString();
                    //    cbAccessLevel.SelectedIndex = (int)AccessLevel.Manufacturer;
                    //    if (!ElevatePrivilege())
                    //        return false;
                    //}

                    try
                    {
                        //for (int i = 0; i < 20; i++)
                        do
                        {
                            try
                            {
                                if (!useDebugRegisterMethod)
                                {//try update using dedicated command
                                    var token = epd.BeginInitiateFirmwareUpdate();
                                    epd.Commit();
                                    epd.EndInitiateFirmwareUpdate(token);
                                }
                                else
                                {//try old way using debug register (only in early code, not in final production)
                                    var token = epd.BeginWriteDebugRegister(1, 0);
                                    epd.Commit();
                                    epd.EndWriteDebugRegister(token);
                                    useDebugRegisterMethod = false;
                                }
                            }
                            catch (EpdException ex)
                            {
                                if (ex.ErrorCode == DllReturnCode.UnknownCmd)
                                {
                                    //we have a firmware version that doesn't include initiateFirmwareUpdate command
                                    useDebugRegisterMethod = true;
                                }
                                else if ((DllReturnCode.InvalidParam == ex.ErrorCode) && useDebugRegisterMethod)
                                {
                                    lblStatusValue.Text = local.UnableToStartBootloader;
                                    return false;
                                }
                                else
                                    throw;
                            }
                        } while (useDebugRegisterMethod == true);

                        //lblStatusValue.Text = local.UnableToStartBootloader;
                        //return false;
                    }
                    catch (EpdException ex)
                    {
                        lblStatusValue.Text = local.UnableToStartBootloader;
                        return false;
                    }
                }
                else
                {
                    epd.Disconnect();
                }
            }

            //Sucessfully put the EPD in bootloader mode

            if (null != stack)
            {
                if (currentState == UpdateState.Discovering) stack.StopDiscovery();
                if (null != epd)
                {
                    stack.ReleaseEpd(epd);
                }
                currentState = UpdateState.Updating;
                stack.Close();
            }
            return true;
        }


        bool ElevatePrivilege()
        {
            foreach (byte[] key in DefaultMfgrKeys.Union(ManufacturerKeyProvider.Instance.ManufacturerKeys))
            {
                try
                {
                    var token = epd.BeginWriteAccessLevel(AccessLevel.Manufacturer, key, null);
                    epd.Commit();
                    epd.EndWriteAccessLevel(token);
                    return true;
                }
                catch (EpdException ex)
                {
                    if (ex.ErrorCode != DllReturnCode.InvalidParam) return false;
                }
            }
            lblStatusValue.Text = local.InsufficientPrivilege;
            return false;
        }



        void BootloaderDetected(object sender, EventArgs<bool> foundArgs)
        {
            controller.BootloaderDetected -= BootloaderDetected;
            if (foundArgs.Value)
            {
                xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Starting));
                if (newMk3Firmware)
                {
                    ReadBin(edtFileName.Text);
                    //tbFileChecksum.Text = GetChecksum().ToString("X04"); //Checksumme anzeigen
                    //Fortschrittanzeige mit Maximum füttern
                    //timer1.Enabled = true; //Überwachung der Datei
                    //timer1.Start();

                    uint blocksize = 512;
                    bool success = false;
                    // Clear application
                    success = ClearApplication();
                    // Send firmware data 
                    if (success)
                        success = SendFirmware(blocksize);
                    //All data sent, reset Trudose
                    if (success) 
                        success = ResetTrudose();

                    if (!success)
                        xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Failed));
                    else xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Complete));
                }
                else
                {
                    controller.StartUpgrade();
                    byte[] bytes = File.ReadAllBytes(edtFileName.Text);
                    System.Threading.Tasks.Task sendTask = new System.Threading.Tasks.Task(() => xmodem.Send(bytes));
                    sendTask.Start();
                }
            }
            else
            {
                xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Failed));
            }
        }


        /// <summary>
        /// Callback called when communication has been re-established with bootloader menu
        /// after flash updating
        /// Invokes EPD reset if communication established
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="foundArgs"></param>
        void BootloaderDetected2(object sender, EventArgs<bool> foundArgs)
        {
            controller.BootloaderDetected -= BootloaderDetected2;
            if (foundArgs.Value)
            {
                if (newMk3Firmware)
                {
                    ResetTrudose();
                }
                else
                {
                    controller.Reset();
                }
                Thread.Sleep(100);
                port.Close();
                if (m_autorun)
                {
                    ResultCode = ResultCodes.OK;
                    System.Windows.Forms.Application.Exit();
                }
                else
                {
                    //Start looking for the next EPD
                    CreateStackStartDiscovery();
                }
            }
            else
            {
                xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Failed));
            }
        }




        void xmodem_TransmitProgress(object sender, ProgressEventArgs e)
        {
            if(InvokeRequired)
            {
                Invoke(new Action(() => xmodem_TransmitProgress(sender, e)));
            }
            else 
            {
                switch(e.TransferStatus)
                {
                    case ProgressEventArgs.Status.Starting:
                        lblStatusValue.Text = local.StartingUpdate;
                        break;
                    case ProgressEventArgs.Status.InProgress:
                        transmitProgress.Maximum = e.TotalBytesToTransfer;
                        transmitProgress.Value = e.BytesTransferred;
                        lblStatusValue.Text = local.Updating;
                        break;
                    case ProgressEventArgs.Status.Complete:
                        lblStatusValue.Text = local.Success;
                        controller.BootloaderDetected += BootloaderDetected2;
                        controller.DetectBootloader(TimeSpan.FromSeconds(5), newMk3Firmware);
                        break;
                    case ProgressEventArgs.Status.Failed:
                        if (m_autorun)
                        {
                            ResultCode = ResultCodes.ProgramingFailed;
                            System.Windows.Forms.Application.Exit();
                        }
                        else
                        {
                            lblStatusValue.Text = local.Failed;
                        }
                        break;
                }
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (m_autorun)
            {
                //ddlPort.SelectedIndex = ddlPort.Items.IndexOf(m_autorun_port);
                //edtFileName.Text = m_autorun_file;
                tmrCmdLineFallback.Enabled = true;
                btnUpdate.Enabled = false;
                ddlPort.Enabled = false;
                edtFileName.Enabled = false;
                cbAccessLevel.Enabled = false;
                tbPassphrase.Enabled = false;
            }

            if (ddlPort.Items.Count > 0)
            {
                if (m_autorun_port == null)
                    ddlPort.SelectedIndex = 0;
                else
                    ddlPort.SelectedIndex = ddlPort.Items.IndexOf(m_autorun_port);
            }

            if (cbAccessLevel.Items.Count > 0)
            {
                if (cbAccessLevel.Text == null)
                    cbAccessLevel.SelectedIndex = (int)AccessLevel.Manufacturer;
                else
                    cbAccessLevel.SelectedIndex = (int)m_Accesslevel;
            }
        }


        /// <summary>
        /// Fallback timer handler
        /// </summary>
        /// <remarks>
        /// Called in commandline mode if EPD not discovered after 15 seconds
        /// This only occurs if an EPD is already in bootloader mode when placed
        /// in front of the reader - because the normal EPD stack is not running
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrCmdLineFallback_Tick(object sender, EventArgs e)
        {
            btnUpdate_Click(null, new EventArgs());
            tmrCmdLineFallback.Enabled = false;
        }

        private void cbAccessLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAccessLevel = cbAccessLevel.SelectedItem.ToString();

            if (selectedAccessLevel == AccessLevel.Open.ToString())
                m_Accesslevel = AccessLevel.Open;
            else if (selectedAccessLevel == AccessLevel.Admin.ToString())
                m_Accesslevel = AccessLevel.Admin;
            else if (selectedAccessLevel == AccessLevel.Regulator.ToString())
                m_Accesslevel = AccessLevel.Regulator;
            else if (selectedAccessLevel == AccessLevel.Manufacturer.ToString())
                m_Accesslevel = AccessLevel.Manufacturer;
            else
                m_Accesslevel = AccessLevel.Manufacturer;

        }

        //For Mk3.1 Firmware Update

        private bool ClearApplication()
        {
            int pos = 0;
            byte[] bytes = new byte[20];
            pos = AddData(true, 0x21, 0, Convert.ToByte(EraseApplication));
            SendData(pos);
            System.Threading.Thread.Sleep(1000); // Wait 1000ms for for answer
            if (CmdA1() == false)
            {
                MessageBox.Show("Error erasing", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        //======================================================================
        //!	\fn     static int AddData(bool init, byte Cmd, int offset, byte[] data)
        //! \brief  Copy a byte array to transmit buffer
        //! \param  init    add preamble (true / false)
        //! \param  Cmd     Command-ID
        //! \param  offset  Offset
        //! \param  data    byte array
        //! \return position after copy
        //======================================================================
        private int AddData(bool init, byte Cmd, int offset, byte[] data)
        {
            int pos;
            if (init == true)
            {
                Array.Clear(TxD, 0, TxD.Length);
                TxD[0] = 0xC0;
                TxD[1] = Convert.ToByte(data.Length + 7); // Low Byte
                TxD[2] = Convert.ToByte((data.Length + 7) / 256);// High Byte
                TxD[3] = Cmd;
                pos = 4;
            }
            else pos = offset;
            Array.Copy(data, 0, TxD, pos, data.Length);
            pos += data.Length;
            return pos;
        }

        private int AddData(bool init, byte Cmd, int offset, byte data)
        {
            int pos;
            if (init == true)
            {
                Array.Clear(TxD, 0, TxD.Length);
                TxD[0] = 0xC0;
                TxD[1] = Convert.ToByte(1 + 7); // Low Byte
                TxD[2] = Convert.ToByte((1 + 7) / 256);// High Byte
                TxD[3] = Cmd;
                pos = 4;
            }
            else pos = offset;
            TxD[pos] = data;
            pos += 1;
            return pos;
        }

        //======================================================================
        //!	\fn     private bool CmdA1()
        //! \brief  Get answer from Trudose of cmd21
        //! \return received bytes
        //======================================================================
        private bool CmdA1()
        {
            int bytes = 0;
            byte[] buffer = new byte[100];
            byte[] EmptyBuffer = new byte[0];
            System.Threading.Thread.Sleep(10); // Wait 10ms
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    bytes += port.Read(buffer, bytes, port.BytesToRead);
                    if (checkChecksum(buffer, bytes) == true) return true;
                }
                catch { }
                System.Threading.Thread.Sleep(10); // Wait 10ms
            }
            return false;
        }

        //======================================================================
        //!	\fn     private void SendData(int len)
        //! \brief  Low level routine to send data to HG
        //! \param  len Payload length
        //======================================================================
        private void SendData(int len)
        {
            ClearInputBuffer();
            len = AddChecksum(len);
            try
            {
                port.Write(TxD, 0, len);
            }
            catch { }
        }


        private void ClearInputBuffer()
        {
            System.Threading.Thread.Sleep(10);
            int bytes = port.BytesToRead;
            byte[] buffer = new byte[bytes];
            port.Read(buffer, 0, bytes);
        }

        //======================================================================
        //!	\fn     private byte[] AlignBuffer(byte[] buffer)
        //! \brief  Align buffer to preamble 0xAA
        //! \return aligned buffer
        //======================================================================        
        private byte[] AlignBuffer(byte[] buffer)
        {
            int i;
            for (i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0xC0) break;
            }
            byte[] result = new byte[buffer.Length - i];
            Array.Copy(buffer, i, result, 0, buffer.Length - i);

            return result;
        }

        //======================================================================
        //!	\fn     static bool checkChecksum(byte[] buffer, int bytes)
        //! \brief  calculation and compare checksum
        //! \param  buffer  buffer
        //! \param  bytes   length
        //! \return true / false
        //======================================================================
        private bool checkChecksum(byte[] buffer, int bytes)
        {
            UInt16 BufferChecksum, CalcChecksum;

            try
            {
                // Find Start of frame 0xC0
                int start = FindStartofFrame(buffer);
                if (start < 0) return false;
                int len = GetLength(buffer, start);
                BufferChecksum = BitConverter.ToUInt16(buffer, start + len - 3);
                CalcChecksum = CalculateCRC(buffer, (uint)start, (uint)(start + len - 3));
                if (CalcChecksum == BufferChecksum) 
                    return true;
                else
                    return false;
            }
            catch 
            { 
                return false; 
            } 
        }
        int FindStartofFrame(byte[] buffer)
        {
            for (int start=0; start < buffer.Length; start++)
            {
                if ((buffer[start] == 0xC0)) return start;
            }
            return -1;
        }
        int GetLength(byte[] buffer, int start)
        {
            return (int)buffer[start + 1];
        }
        //======================================================================
        //!	\fn     static int AddChecksum(int length)
        //! \brief  Claculation of checksum and add it to transmit buffer
        //! \param  length  Länge
        //! \return Position nach dem Kopieren
        //======================================================================
        private int AddChecksum(int length)
        {

            ushort checksum = CalculateCRC(TxD, 0, (uint)length);
            length = AddData(false, 0, length, BitConverter.GetBytes(checksum));
            TxD[length++] = 0xC1;//EOF
            return length;
        }

        //======================================================================
        //!	\fn     public ushort Gen_CRC(byte[] buf, int len)
        //! \brief  CRC16 (Modbus) Checksummenberechnung
        //======================================================================
        public static ushort CalculateCRC(byte[] buf, UInt32 start, UInt32 end)
        {
            ushort CRC = 0xFFFF;
            UInt32 len = (end - start);
            //uint len = st + 507744;
            for (UInt32 pos = start; pos < (start + len); pos++)
            {
                UInt16 temp = Convert.ToUInt16(buf[pos]);
                CRC ^= Convert.ToUInt16(buf[pos]);          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((CRC & 0x0001) != 0)
                    {      // If the LSB is set
                        CRC >>= 1;                    // Shift right and XOR 0xA001
                        CRC ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        CRC >>= 1;                    // Just shift right
                }
            }
            return CRC;
        }

        private bool SendFirmware(uint blocksize)
        {
            for (uint i = ApplicationStart - AddressOffset; i < ApplicationEnd - AddressOffset; i += blocksize)
            {
                // Check if we need to send anything
                if (CheckIForTransfer(blocksize, i) == false) // Block not empty
                {
                    if (Cmd20(i, (int)blocksize) == false)// SendData
                    {// Error
                        MessageBox.Show("Error flashing the TruDose", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        xmodem_TransmitProgress(this, new ProgressEventArgs(ProgressEventArgs.Status.Failed));
                        return false;
                    }
                    System.Threading.Thread.Sleep(20);
                }
                // xmodem_TransmitProgress(this, new ProgressEventArgs((int)((ApplicationEnd - ApplicationStart) / blocksize + 1), (int)(i*blocksize)));
                xmodem_TransmitProgress(this, new ProgressEventArgs((int)(ApplicationEnd - ApplicationStart), (int)(i - 0x2800)));
            }
            return true;
        }

        private bool CheckIForTransfer(uint blocksize, uint i)
        {
            bool Skip = true;
            for (uint x = i; x < (i + blocksize); x++)
            {
                if (BinData[x] != 0xFF)
                {
                    Skip = false;
                    break;
                }
            }

            return Skip;
        }

        //======================================================================
        //!	\fn     private void Cmd20(int adr, int length)
        //! \brief  Send program data to Trudose
        //! \param  adr  location in flash
        //! \param  length  number of bytes to send
        //======================================================================
        private bool Cmd20(uint adr, int length)
        {
            byte[] data = new byte[1100];
            int pos;

            data = BitConverter.GetBytes(Convert.ToUInt32(adr) + AddressOffset);
            pos = AddData(true, 0x20, 0, data);
            Array.Copy(BinData, adr, TxD, pos, length);

            // Length + 4 Byte Address + 1 Byte Commd + 1 Byte BOF + 2 Byte Länge + 2 Byte CRC + 1 Byte EOF = length + 11 Byte
            TxD[2] = Convert.ToByte((length + 11) >> 8);
            TxD[1] = Convert.ToByte((length + 11) & 0xFF);
            SendData(length + 11 - 3); // Checksum und länge werden angefügt
            System.Threading.Thread.Sleep(10);
            return CmdA0();
        }

        //======================================================================
        //!	\fn     private bool CmdA0()
        //! \brief  Flash result of cmd20
        //! \return see documentation
        //======================================================================
        private bool CmdA0()
        {
            int bytes = 0;
            /*
             * How many bytes arrived?
            */
            byte[] buffer = new byte[100];
            System.Threading.Thread.Sleep(10); // Wait 10ms
            for (int i = 0; i < 100; i++)
            {
                try
                {
                    bytes += port.Read(buffer, bytes, port.BytesToRead);
                    if (checkChecksum(buffer, bytes) == true)
                    {
                        if (buffer[4] == 6) return true; //ACK->true
                        else return false; //NAK->false
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(10); // Wait 10ms
            }
            return false;
        }

        private bool ResetTrudose()
        {
            int pos = 0;
            byte[] bytes = new byte[20];
            pos = AddData(true, 0x21, 0, Convert.ToByte(DoReset));
            SendData(pos);
            System.Threading.Thread.Sleep(1000); // Wait 1s for for answer
            return CmdA1();

        }

        //======================================================================
        //!	\fn     public void FillArray(string Dateiname)
        //! \brief  Fill Array from mot-File
        //======================================================================
        public void ReadBin(string Dateiname)
        {
            //Array mit 0xFF füllen
            for (int i = 0; i < BinData.Length; i++) BinData[i] = 0xFF;
            byte[] App = encryptDecryptBinary(Dateiname);
            Array.Copy(App, 0, BinData, 0x2000, App.Length);
        }
        // Secret Key
        static string Key = "4&7MM=WA82XjB?q";

        private static byte[] encryptDecryptBinary(string fileName)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] fileBytes = File.ReadAllBytes(fileName);
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(Key));
            for (Int32 i = 0; i < fileBytes.Length - 1; i++)
            {
                fileBytes[i] ^= hash[i % hash.Length];
            }
            return fileBytes;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                ReadBin(edtFileName.Text);
                //tbFileChecksum.Text = GetChecksum().ToString("X04"); //Show Checksum
            }
            catch
            {
                //tbFileChecksum.Text = "--";
            }
        }
        //======================================================================
        //!	\fn     public UInt16 GetChecksum()
        //! \brief  returns application checksum from application code
        //======================================================================
        public UInt16 GetChecksum()
        {
            int checksum;
            checksum = BinData[0x2001] * 256 + BinData[0x2000];
            return Convert.ToUInt16(checksum);
        }
    }


    public class CommandLineArgsException:ArgumentException
    {
        public CommandLineArgsException(int result):base()
        {
            ResultCode = result;
        }

        public int ResultCode { get; protected set; }
    }

}
