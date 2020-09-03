using ConnectorClientAPI;
using FlightJobsConnectorMSFS.Data;
using FlightJobsConnectorMSFS.Extensions;
using FlightJobsConnectorMSFS.Models;
using FlightJobsConnectorMSFS.Utils;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static FlightJobsConnectorMSFS.Data.SimVarsData;

namespace FlightJobsConnectorMSFS
{

    public enum DEFINITION
    {
        Dummy = 0
    };
    public enum REQUEST
    {
        Dummy = 0
    };

    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        public FlightJobsConnectorClientAPI _flightJobsConnectorClientAPI = new FlightJobsConnectorClientAPI();

        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        /// Window handle
        private IntPtr m_hWnd = new IntPtr(0);

        /// SimConnect object
        private SimConnect m_oSimConnect = null;
        
        private DispatcherTimer _oTimer = new DispatcherTimer();

        private SIMCONNECT_SIMOBJECT_TYPE m_eSimObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER;

        private IList<SimVarModel> m_simVarModelList;

        private bool _isConnected = false;

        private DataModel _simVarsModel;
        
        private string _userId;

        private StartJobResponseModel _startJobResponseInfo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect()
        {
            Console.WriteLine("Sim Connect");

            try
            {
                if (!_isConnected)
                {
                    /// The constructor is similar to SimConnect_Open in the native API
                    m_oSimConnect = new SimConnect("Simconnect - Simvar test", m_hWnd, WM_USER_SIMCONNECT, null, 0);

                    /// Listen to connect and quit msgs
                    m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                    m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                    /// Listen to exceptions
                    m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                    /// Catch a simobject data request
                    m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
                    ReceiveSimConnectMessage();
                    _isConnected = true;
                }
            }
            catch (COMException ex)
            {
                Console.WriteLine("Connection to KH failed: " + ex.Message);
                AddLogMessage("Connection to KH failed. Please run the simulator", LogMessageTypeEnum.Error);
                _oTimer.Stop();
            }
        }

        public void Disconnect()
        {
            _oTimer.Stop();

            if (m_oSimConnect != null)
            {
                /// Dispose serves the same purpose as SimConnect_Close()
                m_oSimConnect.Dispose();
                m_oSimConnect = null;
            }
        }

        public void ReceiveSimConnectMessage()
        {
            m_oSimConnect?.ReceiveMessage();
        }

        private void RegisterSimVars()
        {
            if (m_simVarModelList != null && m_oSimConnect != null)
            {
                
                foreach (var simVar in m_simVarModelList)
                {
                    if (string.IsNullOrEmpty(simVar.UnitysName)) // Strings
                    {
                        m_oSimConnect.AddToDataDefinition(SimVarsEnum.TITLE, simVar.DataName, null, SIMCONNECT_DATATYPE.STRING256, 0, SimConnect.SIMCONNECT_UNUSED);
                    }
                    else
                    {
                        m_oSimConnect.AddToDataDefinition(SimVarsEnum.TITLE, simVar.DataName, simVar.UnitysName, SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                    }
                }

                for (int i = 1; i <= 15; i++)
                {
                    m_oSimConnect.AddToDataDefinition(SimVarsEnum.TOTAL_PAYLOAD, $"PAYLOAD STATION WEIGHT:{i}", "pounds", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                }

                // IMPORTANT: register it with the simconnect managed wrapper marshaller
                // if you skip this step, you will only receive a uint in the .dwData field.
                m_oSimConnect.RegisterDataDefineStruct<SimVarsStruct>(SimVarsEnum.TITLE);
                m_oSimConnect.RegisterDataDefineStruct<SimVarsPayloadStruct>(SimVarsEnum.TOTAL_PAYLOAD);
            }
        }

        private void RequestDataOnSim()
        {
            if (m_simVarModelList != null && m_oSimConnect != null)
            {
                m_oSimConnect?.RequestDataOnSimObjectType(SimVarsEnum.TITLE, SimVarsEnum.TITLE, 5, m_eSimObjectType);

                for (int i = 1; i <= 15; i++)
                {
                    m_oSimConnect.RequestDataOnSimObjectType(SimVarsEnum.TOTAL_PAYLOAD, SimVarsEnum.TOTAL_PAYLOAD, 1, m_eSimObjectType);
                }
            }
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect_OnRecvOpen");
            Console.WriteLine("Connected to KH");

            _oTimer.Start();
            //bOddTick = false;
        }

        /// The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("SimConnect_OnRecvQuit");
            Console.WriteLine("KH has exited");

            Disconnect();
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine("SimConnect_OnRecvException: " + eException.ToString());

            //lErrorMessages.Add("SimConnect : " + eException.ToString());
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            Console.WriteLine("SimConnect_OnRecvSimobjectDataBytype");

            if (data.dwData[0] != null)
            {
                if (data.dwData[0] is SimVarsStruct)
                {
                    SimVarsStruct simVarStruct = (SimVarsStruct)data.dwData[0];
                    _simVarsModel = new DataModel()
                    {
                        Title = simVarStruct.title,
                        Latitude = simVarStruct.latitude,
                        Longitude = simVarStruct.longitude,
                        FuelWeightPounds = Math.Round(simVarStruct.fuel_total_quantity_weight, 0),
                        PayloadPounds = 0,//Math.Round(simVarStruct.total_weight - simVarStruct.empty_weight - simVarStruct.fuel_total_quantity_weight, 0),
                        UserId = string.IsNullOrEmpty(_userId) ? _userId : ""
                    };
                }
                else if (data.dwData[0] is SimVarsPayloadStruct)
                {
                    SimVarsPayloadStruct simVarsPayloadStruct = (SimVarsPayloadStruct)data.dwData[0];
                    _simVarsModel.PayloadPounds =
                        simVarsPayloadStruct.payload_station_weight_1 +
                        simVarsPayloadStruct.payload_station_weight_2 +
                        simVarsPayloadStruct.payload_station_weight_3 +
                        simVarsPayloadStruct.payload_station_weight_4 +
                        simVarsPayloadStruct.payload_station_weight_5 +
                        simVarsPayloadStruct.payload_station_weight_6 +
                        simVarsPayloadStruct.payload_station_weight_7 +
                        simVarsPayloadStruct.payload_station_weight_8 +
                        simVarsPayloadStruct.payload_station_weight_9 +
                        simVarsPayloadStruct.payload_station_weight_10 +
                        simVarsPayloadStruct.payload_station_weight_11 +
                        simVarsPayloadStruct.payload_station_weight_12 +
                        simVarsPayloadStruct.payload_station_weight_13 +
                        simVarsPayloadStruct.payload_station_weight_14 +
                        simVarsPayloadStruct.payload_station_weight_15;
                }

                _simVarsModel.PayloadKilograms = Math.Round(_simVarsModel.PayloadPounds * 0.453592, 0);
                _simVarsModel.FuelWeightKilograms = Math.Round(_simVarsModel.FuelWeightPounds * 0.453592, 0);

                lblLatitude.Content = $"{_simVarsModel.Latitude}";
                lblLongitude.Content = $"{_simVarsModel.Longitude}";
                lblTitle.Content = _simVarsModel.Title;
                lblFuelWeight.Content = $"{_simVarsModel.FuelWeightPounds}Lb / {_simVarsModel.FuelWeightKilograms}Kg";
                lblTotalPayload.Content = $"{_simVarsModel.PayloadPounds}Lb / {_simVarsModel.PayloadKilograms}Kg";
            }
        }

        private void LoadLoginData()
        {
            try
            {
                var lines = File.ReadLines("ResourceData/LoginSavedData.ini");
                var line = lines?.FirstOrDefault();
                var info = line?.Split('|');
                if (info?.Length == 2)
                {
                    txbEmail.Text = info[0];
                    txbPassword.Password = info[1];
                }
            }
            catch (Exception)
            {
                AddLogMessage("[Error] Cannot load the login data.", LogMessageTypeEnum.Error);
            }
        }

        private void SaveLoginData()
        {
            try
            {
                string path = "ResourceData/LoginSavedData.ini";
                string createText = $"{txbEmail.Text}|{txbPassword.Password}";
                File.WriteAllText(path, createText);
            }
            catch (Exception)
            {
                AddLogMessage("[Error] Cannot save the login data.", LogMessageTypeEnum.Error);
            }
        }

        private void AddLogMessage(string msg, LogMessageTypeEnum logMessageType)
        {
            Style style = this.FindResource("LabelSuccessControl") as Style;
            switch (logMessageType)
            {
                case LogMessageTypeEnum.Error:
                    style = this.FindResource("LabelErrorControl") as Style;
                    break;
                case LogMessageTypeEnum.Warnning:
                    style = this.FindResource("LabelWarningControl") as Style;
                    break;
                case LogMessageTypeEnum.Success:
                    style = this.FindResource("LabelSuccessControl") as Style;
                    break;
                default:
                    break;
            }
            lvwMessages.Items.Add(new Label() { Style = style, Content = msg });
            lvwMessages.SelectedIndex = lvwMessages.Items.Count - 1;
            lvwMessages.ScrollIntoView(lvwMessages.SelectedItem);
        }

        private void LoadConnect()
        {
            
            m_simVarModelList = SimVarsData.CreateSimVarList("pounds");
            Connect();
            RegisterSimVars();
            

            _oTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            _oTimer.Tick += new EventHandler(OnTick);
            _oTimer.Start();
        }

        private void OnTick(object sender, EventArgs e)
        {
            RefreshSimVars();
        }

        private void RefreshSimVars()
        {
            try
            {
                if (!_isConnected)
                {
                    Connect();
                }

                m_simVarModelList = SimVarsData.CreateSimVarList("pounds");
                m_oSimConnect?.ClearDataDefinition(SimVarsEnum.TITLE);
                m_oSimConnect?.ClearDataDefinition(SimVarsEnum.TOTAL_PAYLOAD);
                RegisterSimVars();

                RequestDataOnSim();
                Thread.Sleep(500);
                ReceiveSimConnectMessage();
            }
            catch
            {
                AddLogMessage($"Simconnect erro", LogMessageTypeEnum.Error);
                Disconnect();
            }
        }

        private void ckbUseKilograms_Checked(object sender, RoutedEventArgs e)
        {
            // TODO: Save setup
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            RefreshSimVars();
            Thread.Sleep(500);
            try
            {
                if (_simVarsModel != null)
                {
                    _simVarsModel.UserId = _userId;
                    _startJobResponseInfo = await _flightJobsConnectorClientAPI.StartJob(_simVarsModel);
                    AddLogMessage(_startJobResponseInfo.ResultMessage, LogMessageTypeEnum.Success);
                    btnStart.IsEnabled = false;
                    btnFinish.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                AddLogMessage(ex.Message, LogMessageTypeEnum.Warnning);
            }
        }

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            RefreshSimVars();
            Thread.Sleep(500);
            try
            {
                if (_simVarsModel != null)
                {
                    _simVarsModel.UserId = _userId;
                    _startJobResponseInfo = await _flightJobsConnectorClientAPI.FinishJob(_simVarsModel);
                    AddLogMessage(_startJobResponseInfo.ResultMessage, LogMessageTypeEnum.Success);
                    btnStart.IsEnabled = false;
                    btnFinish.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                AddLogMessage(ex.Message, LogMessageTypeEnum.Warnning);
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txbEmail.Text.Trim()) || !Validations.IsValidEmail(txbEmail.Text))
                {
                    txbEmail.Focus();
                    pnlLoginRequired.Visibility = Visibility.Visible;
                    AddLogMessage($"[Error] invalid email", LogMessageTypeEnum.Error);
                    return;
                }
                if (string.IsNullOrEmpty(txbPassword.Password.Trim()))
                {
                    txbPassword.Focus();
                    pnlLoginRequired.Visibility = Visibility.Visible;
                    AddLogMessage($"[Error] invalid password", LogMessageTypeEnum.Error);
                    return;
                }
                txbEmail.IsEnabled = txbPassword.IsEnabled = btnLogin.IsEnabled = false;
                var responseData = await _flightJobsConnectorClientAPI.Login(txbEmail.Text, txbPassword.Password);
                if (responseData != null)
                {
                    _userId = responseData.UserId.Replace("\"", "");
                    AddLogMessage("Logged in success", LogMessageTypeEnum.Success);
                    AddLogMessage(responseData.ActiveJobInfo.Replace("\"", ""), LogMessageTypeEnum.Success);
                    btnStart.IsEnabled = true;
                    LoadConnect();
                    SaveLoginData();
                }
                else
                {
                    txbEmail.IsEnabled = txbPassword.IsEnabled = btnLogin.IsEnabled = true;
                    AddLogMessage($"[Warnning] Wrong login data. Check you email and password.", LogMessageTypeEnum.Warnning);
                }
            }
            catch (Exception ex)
            {
                txbEmail.IsEnabled = txbPassword.IsEnabled = btnLogin.IsEnabled = true;
                AddLogMessage($"[Error] {ex.Message}", LogMessageTypeEnum.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLoginData();
        }

        
    }
}
