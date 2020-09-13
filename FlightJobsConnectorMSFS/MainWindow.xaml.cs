using ConnectorClientAPI;
using FlightJobsConnectorMSFS.Data;
using FlightJobsConnectorMSFS.Extensions;
using FlightJobsConnectorMSFS.Models;
using FlightJobsConnectorMSFS.Utils;
using Microsoft.FlightSimulator.SimConnect;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interação lógica para MainWindow.xam        Squirrel --releasify FlightJobsPackage.0.1.0.nupkg
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

        private IList<JobModel> _jobs;

        private bool _finishPopUpShown;
        private bool _startPopUpShown;

        public MainWindow()
        {
            InitializeComponent();
            AddVersionNumber();
        }

        private void AddVersionNumber()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.Title += $" v.{fileVersionInfo.FileVersion}";
        }

        private async Task CheckForUpdates()
        {
            try
            {
                using (var manager = await UpdateManager.GitHubUpdateManager(@"https://github.com/rhpa23/FlightJobsConnectorMSFS"))
                {
                    var updateInfo = await manager.CheckForUpdate();

                    if (updateInfo.ReleasesToApply.Any())
                    {
                        await manager.UpdateApp();
                        AddLogMessage($"This app was updated. The new version will take effect when App is restarted.", LogMessageTypeEnum.Success);
                    }
                }
            }
            catch (Exception e)
            {
                AddLogMessage($"Update app failed.", LogMessageTypeEnum.Error);
            }
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
                    AddLogMessage("Connected to KH", LogMessageTypeEnum.Success);
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
            _isConnected = false;
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

                //m_oSimConnect.WeatherRequestObservationAtStation(SimVarsEnum.WEATHER_INFO, "LOWW");
                //m_oSimConnect.WeatherRequestObservationAtNearestStation(SimVarsEnum.WEATHER_INFO, 48.117680599757485f, 16.566300032290759f);
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

            if (_startJobResponseInfo != null)
            {
                Reset();
            }
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
                        UserId = string.IsNullOrEmpty(_userId) ? _userId : "",
                        Pressure = Math.Round(simVarStruct.sea_level_pressure, 0),
                        WindDirection = Math.Round(simVarStruct.ambient_wind_direction,0),
                        WindVelocity = Math.Round(simVarStruct.ambient_wind_velocity,0),
                        Temperature = Math.Round(simVarStruct.ambient_temperature,0),
                        Visibility = Math.Round(simVarStruct.ambient_visibility, 0),
                        ParkingBrakeOn = Convert.ToBoolean(simVarStruct.brake_parking_position),
                        EngOneRunning = Convert.ToBoolean(simVarStruct.eng_combustion)
                    };
                }
                else if (data.dwData[0] is SimVarsPayloadStruct)
                {
                    SimVarsPayloadStruct simVarsPayloadStruct = (SimVarsPayloadStruct)data.dwData[0];
                    _simVarsModel.PayloadPounds = Math.Round(
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
                        simVarsPayloadStruct.payload_station_weight_15, 0);
                }

                _simVarsModel.PayloadKilograms = Math.Round(_simVarsModel.PayloadPounds * 0.453592, 0);
                _simVarsModel.FuelWeightKilograms = Math.Round(_simVarsModel.FuelWeightPounds * 0.453592, 0);

                //lblLatitude.Content = $"{_simVarsModel.Latitude}";
                //lblLongitude.Content = $"{_simVarsModel.Longitude}";
                lblTitle.Content = _simVarsModel.Title;
                lblFuelWeight.Content = $"{_simVarsModel.FuelWeightPounds} Lb / {_simVarsModel.FuelWeightKilograms} Kg";
                lblTotalPayload.Content = $"{_simVarsModel.PayloadPounds} Lb / {_simVarsModel.PayloadKilograms} Kg";
                lblPressure.Content = $"{(_simVarsModel.Pressure * 0.02953).ToString("0.00")} inHg / {_simVarsModel.Pressure} mbar";
                lblWind.Content = $"{_simVarsModel.WindDirection}º with {_simVarsModel.WindVelocity} Kts";
                lblTemperature.Content = $"{_simVarsModel.Temperature}º";
                lblVisibility.Content = $"{_simVarsModel.Visibility} meters";
            }
        }

        private void Reset()
        {
            txbEmail.IsEnabled = txbPassword.IsEnabled = btnLogin.IsEnabled = true;
            btnStart.IsEnabled = false;
            btnFinish.IsEnabled = false;
            lvwMessages.Items.Clear();

            //lblLatitude.Content = "0";
            //lblLongitude.Content = "0";
            lblTotalPayload.Content = "0";
            lblTitle.Content = "0";
            lblFuelWeight.Content = "0";
            lblPressure.Content = "0";
            lblWind.Content = "0";
            lblTemperature.Content = "0";
            lblVisibility.Content = "0";
            jobListDataGrid.IsEnabled = true;
            jobListDataGrid.ItemsSource = null;
            _startJobResponseInfo = null;
            _finishPopUpShown = false;
            _startPopUpShown = false;
            imgStart.Visibility = Visibility.Hidden;
            imgFinish.Visibility = Visibility.Hidden;
            Disconnect();
        }

        private void LoadSettingsData()
        {
            try
            {
                var path = System.AppDomain.CurrentDomain.BaseDirectory;
                var lines = File.ReadLines(System.IO.Path.Combine(path, "ResourceData\\Settings.ini"));
                var line = lines?.FirstOrDefault();
                var info = line?.Split('|');
                if (info?.Length > 2)
                {
                    ckbPopupParkingBrake.IsChecked = info[0] == "1";
                    ckbAutoUpdate.IsChecked = info[1] == "1";
                    ckbOnTop.IsChecked = info[2] == "1";
                }
            }
            catch (Exception ex)
            {
                AddLogMessage("[Error] Cannot load the login data.", LogMessageTypeEnum.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                var path = System.AppDomain.CurrentDomain.BaseDirectory;
                path = System.IO.Path.Combine(path, "ResourceData\\Settings.ini");
                string ckbPopupParkingBrakeValue = ckbPopupParkingBrake.IsChecked.Value ? "1" : "0";
                string ckbAutoUpdateValue = ckbAutoUpdate.IsChecked.Value ? "1" : "0";
                string ckbOnTopValue = ckbOnTop.IsChecked.Value ? "1" : "0";
                string createText = $"{ckbPopupParkingBrakeValue}|{ckbAutoUpdateValue}|{ckbOnTopValue}";
                File.WriteAllText(path, createText);
                AddLogMessage($"Settings saved", LogMessageTypeEnum.Success);
                MessageBox.Show("Settings saved", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Topmost = ckbOnTop.IsChecked.Value;
            }
            catch (Exception ex)
            {
                AddLogMessage($"[Error] {ex.Message}", LogMessageTypeEnum.Error);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void LoadLoginData()
        {
            try
            {
                var path = System.AppDomain.CurrentDomain.BaseDirectory;
                var lines = File.ReadLines(System.IO.Path.Combine(path, "ResourceData\\LoginSavedData.ini"));
                var line = lines?.FirstOrDefault();
                var info = line?.Split('|');
                if (info?.Length == 2)
                {
                    txbEmail.Text = info[0];
                    txbPassword.Password = info[1];
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"[Error] Cann1ot load the login data.", LogMessageTypeEnum.Error);
            }
        }

        private void SaveLoginData()
        {
            try
            {
                var path = System.AppDomain.CurrentDomain.BaseDirectory;
                path = System.IO.Path.Combine(path, "ResourceData/LoginSavedData.ini");
                string createText = $"{txbEmail.Text}|{txbPassword.Password}";
                File.WriteAllText(path, createText);
            }
            catch (Exception)
            {
                AddLogMessage("[Error] Cannot save the login data.", LogMessageTypeEnum.Error);
            }
        }

        private async Task LoadJobListDataGrid()
        {
            try
            {
                if (!string.IsNullOrEmpty(_userId))
                {
                    _jobs = await _flightJobsConnectorClientAPI.GetUserJobs(_userId);
                    jobListDataGrid.ItemsSource = _jobs;
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"[Error] {ex.Message}", LogMessageTypeEnum.Error);
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
            CheckWindowPopup();
        }

        private void CheckWindowPopup()
        {
            if (_simVarsModel != null && ckbPopupParkingBrake.IsChecked.Value)
            {
                if (_startJobResponseInfo == null)
                {
                    if (!_startPopUpShown && !_simVarsModel.ParkingBrakeOn)
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        _startPopUpShown = true;
                    }
                }
                else
                {
                    if (!_finishPopUpShown)
                    {
                        bool isCloseToArrivel = AirportDatabaseFile.CheckClosestLocation(_simVarsModel.Latitude, _simVarsModel.Longitude,
                                                                                     _startJobResponseInfo.ArrivalLAT, _startJobResponseInfo.ArrivalLON);
                        if (isCloseToArrivel && _simVarsModel.ParkingBrakeOn)
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                            _finishPopUpShown = true;
                            imgStart.Visibility = Visibility.Hidden;
                            imgFinish.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        private void RefreshSimVars()
        {
            try
            {
                if (!_isConnected)
                {
                    Connect();
                }

                m_oSimConnect?.ClearDataDefinition(SimVarsEnum.TITLE);
                m_oSimConnect?.ClearDataDefinition(SimVarsEnum.TOTAL_PAYLOAD);
                RegisterSimVars();

                RequestDataOnSim();
                ReceiveSimConnectMessage();
            }
            catch
            {
                AddLogMessage($"Simconnect fail", LogMessageTypeEnum.Error);
                Disconnect();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            btnStart.IsEnabled = false;
            btnFinish.IsEnabled = false;
            LoadConnect();
            Thread.Sleep(500);
            try
            {
                if (_simVarsModel != null)
                {
                    
                    _simVarsModel.UserId = _userId;
                    _startJobResponseInfo = await _flightJobsConnectorClientAPI.StartJob(_simVarsModel);
                    var arrivalInfo = AirportDatabaseFile.FindAirportInfo(_startJobResponseInfo.ArrivalICAO);
                    _startJobResponseInfo.ArrivalLAT = arrivalInfo.Latitude;
                    _startJobResponseInfo.ArrivalLON = arrivalInfo.Longitude;
                    AddLogMessage(_startJobResponseInfo.ResultMessage, LogMessageTypeEnum.Success);
                    btnFinish.IsEnabled = true;
                    jobListDataGrid.IsEnabled = false;
                    imgStart.Visibility = Visibility.Visible;
                    imgFinish.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnStart.IsEnabled = true;
                    btnFinish.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                btnStart.IsEnabled = true;
                btnFinish.IsEnabled = false;
                AddLogMessage(ex.Message, LogMessageTypeEnum.Warnning);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            btnStart.IsEnabled = false;
            btnFinish.IsEnabled = false;

            RefreshSimVars();
            Thread.Sleep(500);
            try
            {
                if (_simVarsModel != null)
                {
                    _simVarsModel.UserId = _userId;
                    _startJobResponseInfo = await _flightJobsConnectorClientAPI.FinishJob(_simVarsModel);
                    AddLogMessage(_startJobResponseInfo.ResultMessage, LogMessageTypeEnum.Success);
                    await LoadJobListDataGrid();
                    imgStart.Visibility = Visibility.Hidden;
                    imgFinish.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnStart.IsEnabled = false;
                    btnFinish.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                btnFinish.IsEnabled = true;
                AddLogMessage(ex.Message, LogMessageTypeEnum.Warnning);
            }
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
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
                    await LoadJobListDataGrid();
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
            finally
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string msg = "Do you really want to reset the connector?";
                if (MessageBox.Show(msg, "Reset?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Reset();
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"[Error] {ex.Message}", LogMessageTypeEnum.Error);
            }

        }

        private async void Row_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                jobListDataGrid.IsEnabled = false;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                var selected = (JobModel)((System.Windows.FrameworkElement)sender).DataContext;
                if (!selected.IsActivated)
                {
                    var result = await _flightJobsConnectorClientAPI.ActivateUserJob(_userId, selected.Id);
                    if (result)
                    {
                        AddLogMessage($"The current activeted job is from {selected.DepartureICAO} to {selected.ArrivalICAO}", LogMessageTypeEnum.Success);
                        await LoadJobListDataGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"[Error] {ex.Message}", LogMessageTypeEnum.Error);
            }
            finally
            {
                jobListDataGrid.IsEnabled = true;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            m_simVarModelList = SimVarsData.CreateSimVarList("pounds");
            LoadLoginData();
            LoadSettingsData();
            if (ckbAutoUpdate.IsChecked.Value)
            {
                CheckForUpdates();
            }

            this.Topmost = ckbOnTop.IsChecked.Value;
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private async void btnUpdadeApp_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdates();
        }
    }
}
