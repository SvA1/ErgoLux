﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FTD2XX_NET;

namespace ErgoLux
{
    public partial class FrmSettings : Form
    {
        private int[] _data = new int[10];
        private string _deviceType;
        private string _deviceID;
        private ClassSettings _settings;

        public string GetDeviceType { get => _deviceType; }
        public string GetDeviceID { get => _deviceID; }

        public FrmSettings()
        {
            InitializeComponent();

            // Initializa ListView
            viewDevices.View = View.Details;
            viewDevices.GridLines = true;
            viewDevices.FullRowSelect = true;

            //Add column header
            viewDevices.Columns.Add("Device index", 50);
            viewDevices.Columns.Add("Flags", 100);
            viewDevices.Columns.Add("Type", 100);
            viewDevices.Columns.Add("ID", 70);
            viewDevices.Columns.Add("Location ID", 70);
            viewDevices.Columns.Add("Serial number", 100);
            viewDevices.Columns.Add("Description", 100);
            
            PopulateDevices();

            // Populate cboDataBits
            Dictionary<string, int> cboList = new Dictionary<string, int>();
            cboList.Add("7 bits", FTDI.FT_DATA_BITS.FT_BITS_7);
            cboList.Add("8 bits", FTDI.FT_DATA_BITS.FT_BITS_8);

            cboDataBits.DataSource = new BindingSource(cboList, null);
            cboDataBits.DisplayMember = "Key";
            cboDataBits.ValueMember = "Value";
            cboDataBits.SelectedIndex = 0;

            // Populate cboStop
            cboList.Clear();
            cboList.Add("1 bit", FTDI.FT_STOP_BITS.FT_STOP_BITS_1);
            cboList.Add("2 bits", FTDI.FT_STOP_BITS.FT_STOP_BITS_2);

            cboStopBits.DataSource = new BindingSource(cboList, null);
            cboStopBits.DisplayMember = "Key";
            cboStopBits.ValueMember = "Value";
            cboStopBits.SelectedIndex = 0;

            // Populate cboParity
            cboList.Clear();
            cboList.Add("None", FTDI.FT_PARITY.FT_PARITY_NONE);
            cboList.Add("Odd", FTDI.FT_PARITY.FT_PARITY_ODD);
            cboList.Add("Even", FTDI.FT_PARITY.FT_PARITY_EVEN);
            cboList.Add("Mark", FTDI.FT_PARITY.FT_PARITY_MARK);
            cboList.Add("Space", FTDI.FT_PARITY.FT_PARITY_SPACE);

            cboParity.DataSource = new BindingSource(cboList, null);
            cboParity.DisplayMember = "Key";
            cboParity.ValueMember = "Value";
            cboParity.SelectedIndex = 2;

            // Populate cboFlowControl
            cboList.Clear();
            cboList.Add("None", FTDI.FT_FLOW_CONTROL.FT_FLOW_NONE);
            cboList.Add("RTS / CTS", FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS);
            cboList.Add("DTR / DSR", FTDI.FT_FLOW_CONTROL.FT_FLOW_DTR_DSR);
            cboList.Add("Xon / Xoff", FTDI.FT_FLOW_CONTROL.FT_FLOW_XON_XOFF);

            cboFlowControl.DataSource = new BindingSource(cboList, null);
            cboFlowControl.DisplayMember = "Key";
            cboFlowControl.ValueMember = "Value";
            cboFlowControl.SelectedIndex = 0;

            txtBaudRate.Text = "9600";
            txtOn.Text = "11";
            txtOff.Text = "13";
            txtHz.Text = "2";
        }

        public FrmSettings(int[] _settings)
            :this()
        {
            updSensors.Value = _settings[1];
            txtBaudRate.Text = _settings[2].ToString();
            cboDataBits.SelectedValue = _settings[3];
            cboStopBits.SelectedValue = _settings[4];
            cboParity.SelectedValue = _settings[5];
            cboFlowControl.SelectedValue = _settings[6];
            txtOn.Text = _settings[7].ToString();
            txtOff.Text = _settings[8].ToString();
            txtHz.Text = _settings[9].ToString();
        }

        public FrmSettings(ClassSettings settings)
            :this()
        {
            _settings = settings;

            // Update tabDevice
            updSensors.Value = settings.T10_NumberOfSensors;
            txtBaudRate.Text = settings.T10_BaudRate.ToString();
            cboDataBits.SelectedValue = settings.T10_DataBits;
            cboStopBits.SelectedValue = settings.T10_StopBits;
            cboParity.SelectedValue = settings.T10_Parity;
            cboFlowControl.SelectedValue = settings.T10_FlowControl;
            txtOn.Text = settings.T10_CharOn.ToString();
            txtOff.Text = settings.T10_CharOff.ToString();
            txtHz.Text = settings.T10_Frequency.ToString();

            // Update tabPlots
            txtArrayPoints.Text = settings.Plot_ArrayPoints.ToString();
            txtPlotWindow.Text = settings.Plot_WindowPoints.ToString();
            chkShowRaw.Checked = settings.Plot_ShowRawData;
            chkShowRadar.Checked = settings.Plot_ShowRadar;
            chkShowAverage.Checked = settings.Plot_ShowAverage;
            chkShowRatio.Checked = settings.Plot_ShowRatios;
        }

        /// <summary>
        /// Populates ListView control with data from the FTDI devices connected
        /// </summary<
        private void PopulateDevices()
        {
            // FTDI connection code
            UInt32 ftdiDeviceCount = 0;
            FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

            // Create new instance of the FTDI device class
            FTDISample myFtdiDevice = new FTDISample();

            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);

            // Allocate storage for device info list
            FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);
            if (ftStatus != FTDI.FT_STATUS.FT_OK) return;

            // Populate the control
            ListViewItem item;
            for (UInt32 i = 0; i < ftdiDeviceCount; i++)
            {
                item = new ListViewItem(
                    new string[]
                    {
                            i.ToString(),
                            String.Format("{0:X}", ftdiDeviceList[i].Flags),
                            ftdiDeviceList[i].Type.ToString(),
                            String.Format("{0:X}", ftdiDeviceList[i].ID),
                            String.Format("{0:X}", ftdiDeviceList[i].LocId),
                            ftdiDeviceList[i].SerialNumber.ToString(),
                            ftdiDeviceList[i].Description.ToString()
                    });
                if (i++ % 2 == 1)
                {
                    item.BackColor = Color.FromArgb(240, 240, 240);
                    item.UseItemStyleForSubItems = true;
                }
                viewDevices.Items.Add(item);
            }
        }

        private void cboFlowControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboFlowControl.SelectedIndex == 3)
            {
                txtOn.Enabled = true;
                txtOff.Enabled = true;
            }
            else
            {
                txtOn.Enabled = false;
                txtOff.Enabled = false;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;

            if (viewDevices.SelectedIndices.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("Please select one of the devices from the list.",
                    "Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            //_data[0] = Convert.ToInt32(viewDevices.SelectedItems[0].SubItems[4].Text, 16);  //Location ID
            //_data[1] = (int)updSensors.Value;
            //_data[2] = Convert.ToInt32(txtBaudRate.Text);
            //_data[3] = ((KeyValuePair<string, int>)cboDataBits.SelectedItem).Value;
            //_data[4] = ((KeyValuePair<string, int>)cboStopBits.SelectedItem).Value;
            //_data[5] = ((KeyValuePair<string, int>)cboParity.SelectedItem).Value;
            //_data[6] = ((KeyValuePair<string, int>)cboFlowControl.SelectedItem).Value;
            //_data[7] = Convert.ToInt32(txtOn.Text);
            //_data[8] = Convert.ToInt32(txtOff.Text);
            //_data[9] = Convert.ToInt32(txtHz.Text);   // Sample rate

            _deviceType = viewDevices.SelectedItems[0].SubItems[2].Text;
            _deviceID = viewDevices.SelectedItems[0].SubItems[3].Text;

            // Save to class settings
            _settings.T10_LocationID = Convert.ToInt32(viewDevices.SelectedItems[0].SubItems[4].Text, 16);
            _settings.T10_NumberOfSensors = (int)updSensors.Value;
            _settings.T10_BaudRate = Convert.ToInt32(txtBaudRate.Text);
            _settings.T10_DataBits = ((KeyValuePair<string, int>)cboDataBits.SelectedItem).Value;
            _settings.T10_StopBits = ((KeyValuePair<string, int>)cboStopBits.SelectedItem).Value;
            _settings.T10_Parity = ((KeyValuePair<string, int>)cboParity.SelectedItem).Value;
            _settings.T10_FlowControl = ((KeyValuePair<string, int>)cboFlowControl.SelectedItem).Value;
            _settings.T10_CharOn = Convert.ToInt32(txtOn.Text);
            _settings.T10_CharOff = Convert.ToInt32(txtOff.Text);
            _settings.T10_Frequency = Convert.ToInt32(txtHz.Text);

            _settings.Plot_ArrayPoints = Convert.ToInt32(txtArrayPoints.Text);
            _settings.Plot_WindowPoints = Convert.ToInt32(txtPlotWindow.Text);
            _settings.Plot_ShowRawData = chkShowRaw.Checked;
            _settings.Plot_ShowRadar = chkShowRadar.Checked;
            _settings.Plot_ShowAverage = chkShowAverage.Checked;
            _settings.Plot_ShowRatios = chkShowRatio.Checked;

            this.DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
