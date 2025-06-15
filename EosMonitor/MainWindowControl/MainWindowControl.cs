using System;
using System.IO;
using System.Management;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using EDSDKLib;
using static EDSDKLib.EDSDK;


namespace EosMonitor
{
    public partial class MainWindow : Window
    {
        // Initialize camera and user interface
        #region
        private void Initialize()
        {
            System.Threading.Thread.Sleep(500);
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                // Show a "wait" cursor
                Application.Current.Dispatcher.Invoke(() => {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.AppStarting;
                });

                // Initialite camera and user interface
                InitializeCamera(cameraIndex);
                InitializeControls();
                if (cameraModel == null) { ReportError("Initialize: cameraModel==null"); return; }

                // Update CameraLabel text
                CameraLabel.Content = "connected to " + MainWindow.cameraName.Substring(5);
                CameraLabel.Foreground = Brushes.Green;
                CameraLabel.FontSize = 13;

                // Trick 17 !!! (Timing problem with Eos 60D)
                ApertureValueComboBox.SelectedValue = "4";
                ApertureValueComboBox.SelectedValue = cameraModel.ApertureStr;

                // Show normal cursor
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    Mouse.OverrideCursor = null;
                });
            }));
        }
        private void InitializeCamera(int cameraIndex)
        {
            // Initialize the camera controller for the first camera in the list
            if (cameraCount > 0 && Error == EDSDK.EDS_ERR_OK) {
                // Get the cameraList from the SDK
                if (Error == EDSDK.EDS_ERR_OK) {
                    Error = EDSDK.EdsGetCameraList(out cameraList);
                }
                // Get the number of attached cameras from the SDK
                if (Error == EDSDK.EDS_ERR_OK) {
                    Error = EdsGetChildCount(cameraList, out cameraCount);
                }
                // Get a camera, initialize FocusShift, then open session for the camera
                Error = EDSDK.EdsGetChildAtIndex(cameraList, cameraIndex, out cameraPtr);

                if (Error == EDSDK.EDS_ERR_OK) Error = EDSDK.EdsOpenSession(cameraPtr);
                if (Error == EDSDK.EDS_ERR_OK) {
                // remember that session is opened until cameraModel is runnunnig
                _sessionIsOpen = true;

                // Initialize command processor thread
                cmdProcessor = new CmdProcessor();
                threadPoolWorker.Work(() => MainWindow.cmdProcessor.run());
                _syncCmdProcessor.WaitOne(new TimeSpan(0, 0, 2), false);

                // Create a cameraModel  object 
                if (Error == EDSDK.EDS_ERR_OK) {
                    // create camera controller 
                    cameraModel = new CameraModel(cameraPtr);
                    _cameraIsInitialized = true;

                    // Register event handlers 
                    cameraModel.IsoSpeedChanged          += HandleIsoSpeedChanged;
                    cameraModel.AEModeChanged            += HandleAEModeChanged;
                    cameraModel.EvfOutputDeviceChanged   += HandleEvfOutputDeviceChanged;
                    cameraModel.AvValueChanged           += HandleAvValueChanged;
                    cameraModel.TvValueChanged           += HandleTvValueChanged;
                    cameraModel.DriveModeChanged         += HandleDriveModeChanged;
                    cameraModel.MeterModeChanged         += HandleMeterModeChanged;
                    cameraModel.AFModeChanged            += HandleAFModeChanged;
                    cameraModel.OnProgressMade           += HandleProgressBar;
                    cameraModel.EvcChanged               += HandleEVCChanged;
                    cameraModel.BatteryLevelChanged      += HandleBatteryLevelChanged;
                    _FocusInfoTimer.Elapsed             += HandleFocusInfoTimer;
                    _EVF_AutofocusTimer.Elapsed         += HandleEVFAutofocusTimer;
                    _HistogramTimer.Elapsed             += HandleHistogramTimer;
                    SizeChanged                         += HandleMainWindowSizeChanged;
                    Closed                              += HandleWindowClosed;

                    ZoomRectCanvas.PreviewMouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(EvfFocusCanvas_PreviewMouseLeftButtonDown);
                    ZoomRectCanvas.PreviewMouseMove           += new System.Windows.Input.MouseEventHandler(EvfFocusCanvas_PreviewMouseMove);
                    ZoomRectCanvas.PreviewMouseLeftButtonUp   += new System.Windows.Input.MouseButtonEventHandler(EvfFocusCanvas_PreviewMouseLeftButtonUp);
                    ZoomRectCanvas.PreviewKeyDown             += new System.Windows.Input.KeyEventHandler(EvfFocusCanvas_PreviewKeyDown);

                    //Decrease Opacity of the Eos Monitor logo 
                    var LogoTask = new ThreadPoolWorker();
                    LogoTask.Work(() => { while (DecreaseLogoOpacity()) { Thread.Sleep(25); } });

                    // Initialize camera properties
                    cameraModel.getCameraProperties();
                }
            }
        }
    }
        private void InitializeControls()
        {
            if (cameraModel == null) { ReportError("InitializeControls:  cameraModel==null"); return; }

            base.DataContext = cameraModel;
            _lockPropChanged = true;

            // remember Window width at start time: used for scaling on SizeChanged events
            _oldWindowWidth = Width;

            // Initialize Icon resources
            cameraModel.LiveViewIcon     = "../Resources/LiveViewIcons/LiveView_black.png";
            cameraModel.ZoomIcon         = "../Resources/LiveViewIcons/ZoomPlus.png";
            cameraModel.HistogramIcon    = "../Resources/LiveViewIcons/HistogramIcon.png";
            cameraModel.CrosslinesIcon   = "../Resources/LiveViewIcons/Crosslines.png";
            cameraModel.BulbTimerIcon    = "../Resources/BulbTimerIcons/BulbTimerIcon_gray.png";
            cameraModel.LvStartStop      = "Start";

            // Initialize flags for live shooting 
            cameraModel.LiveShooting = EnableState.enabled;
            cameraModel.EvfZoom = EvfZoomFactor.fit;

            // Initialize the PicturePathGroupBox
            _saveFolderBrowser.Description = "Save Images To...";
            storePicturesOnHostRadioButton.IsEnabled = true;

            // Initialize the ImageSaveDirectory on the Host
            _imageSaveDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");
            if (!Directory.Exists(_imageSaveDirectory)) Directory.CreateDirectory(_imageSaveDirectory);
            pathOnHostTextBox.Text =_imageSaveDirectory;

            // Initialize the save destination to 'save to _camera'
            cameraModel.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Camera);
            pathOnHostTextBox.IsEnabled = false;
            SetPictureSaveDestination();
            _picturePathOK = true;

            // Display the the battery level (as text and battery icon)
            fillStatus.Text = cameraModel.BatteryLevel.ToString() + "% ";
            ShowBatteryLevel(cameraModel.BatteryLevel);

            // Display the number of available shots
            AvShots.Text = cameraModel.AvailableShots.ToString();   // Display the number of available shots

            // Initialaize BulbTimerControl
            if (cameraModel.AEMode == 4) Setup_BulbTimerControl(Visibility.Visible);
            else Setup_BulbTimerControl(Visibility.Hidden);

			// Live View is not yet active
            cameraModel.IsInLiveViewMode = false;
            LVButtonHG.Text = "";

            // Initialize the buttons on the 'ExpanderLeft'
            Setup_AEModeLabel();                                   // Display the Auto Exposure Mode (AE mode) Label
            Setup_Av_Tv_Iso_ComboBoxes(cameraModel.AEMode);         // Setup the Av-, Tv- and Iso-lists
            Setup_DriveMode_Button(0);                             // Initialize the drive mode button
            Setup_MeterMode_Button(0);                             // Initialize the meter mode button
            Setup_DSLR_AFMode_Button(0);                           // Initialize the autofocus (AF mode) button for EOS DSLR models
            Setup_DSLM_AFMButton(0);                               // Initialize the autofocus (AF mode) button for EOS Rx modles
            SetupEvcPointer();                                     // Initialize the EVC (Exposure value compensation) pointer
            ShowLvAFModeScale();                                    // Display the Exposure Value Compensation (Evc) scale
            Initialize_DSLR_DSLM_Controls();                       // Toggle between (DSLR_AFM + LvAFM) and (DSLM_AFM + DSLM_FStacking) buttons 

            _lockPropChanged = false;
        }
        #endregion

        // Handle events from the user controls
        #region 
        private void HandleCameraComboBox_SelectionChanged(object sender, EventArgs e)
        {
            if (_lockPropChanged) return;
            if (CameraComboBox.SelectedIndex == -1) return;

            // Establish a new connection to the selected camera
            if( !_sessionIsOpen ) {
                cameraName = (string)CameraComboBox.SelectedValue;
                cameraIndex = CameraComboBox.SelectedIndex;
                CameraComboBox.Visibility = Visibility.Hidden;

                // Modify CameraLabel text 
                CameraLabel.Content = "connecting to camera  ...";
                CameraLabel.Foreground = Brushes.Blue;

                Thread InitializeThread = new(new ThreadStart(Initialize));
                InitializeThread.Start();
            }
        }
        private void HandleAEModeChanged(object sender, AEModeChangedEventArgs aemode)
        {   // Handle AE mode changes by the camera 
            if (cameraModel == null) { ReportError("HandleAEModeChanged: cameraModel==null"); return; }

            // provide the "Content" for Label "AutoExposureMode"
            string aemodeStr;
            Setup_BulbTimerControl(Visibility.Hidden);

            LVButton.Opacity = 1.0;
            LiveViewLabel.Opacity = 1.0;
            LVImage.Opacity = 1.0;

            uint aemodeNr = cameraModel.GetSetting(EDSDK.PropID_AEMode);
            switch (aemodeNr) {
                case 0: aemodeStr  = " P";  break;
                case 1: aemodeStr  = "Tv";  break;
                case 2: aemodeStr  = "Av";  break;
                case 3: aemodeStr  = " M";  break;
                case 4: aemodeStr  = " B "; Setup_BulbTimerControl(Visibility.Visible);
                    // In bulb mode no Liveview!!
                    LVButton.Opacity = 0.0;
                    LiveViewLabel.Opacity = 0.0;
                    LVImage.Opacity = 0.0;
                    cameraModel.LiveViewDevice = cameraModel.LiveViewDevice = LiveViewDevice.Camera;
                    break;
                default: aemodeStr = " ";   break;
            }
            if (aemodeStr == " ") {
                ReportError("Selected AE mode is not supported, continue with Av/Tv/M/P/B mode!");
                cameraModel.SetSetting(EDSDK.PropID_AEMode, 2);
                return;
            }
            AutoExposureMode.Content = aemodeStr;

            // Initialize the comboboxes for Tv-, Av- and Iso-selection
            Setup_Av_Tv_Iso_ComboBoxes(cameraModel.AEMode);
        }
        private void HandleEvfOutputDeviceChanged(object sender, EventArgs e)
        {   // handle LiveView mode change events
            if (cameraModel == null) { ReportError("HandleEvfOutputDeviceChanged: cameraModel==null"); return; };
            if (_lockPropChanged) return;

            if (cameraModel.IsInHostLiveViewMode  && cameraModel.LiveViewDevice.HasFlag(LiveViewDevice.Host)) {
                var LiveViewTask = new ThreadPoolWorker();
                LiveViewTask.Work(() => { cameraModel.InitLiveView(); });
            }
            else {
                _lockPropChanged = true;
                cameraModel.EvfOutputDeviceChanged -= HandleEvfOutputDeviceChanged;
                cameraModel.LiveViewDevice = LiveViewDevice.Camera;
                cameraModel.IsInLiveViewMode = false;
                cameraModel.EvfOutputDeviceChanged += HandleEvfOutputDeviceChanged;
                _lockPropChanged = false;
            }
        }
        private void HandleIsoSpeedChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleIsoSpeedChanged: cameraModel==null"); return; };
            if (_lockPropChanged) return;

            _lockPropChanged = true;
            uint IsoValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_ISOSpeed);
            string SelectedValue = DataConversion.ISO(IsoValue);
            IsoComboBox.SelectedIndex = IsoComboBox.Items.IndexOf(SelectedValue);
            Thread.Sleep(cameraModel.standardDelay);
            IsoComboBox.SelectedIndex = IsoComboBox.Items.IndexOf(DataConversion.ISO((uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_ISOSpeed)));
            _lockPropChanged = false;
        }
        private void HandleAvValueChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleAvValueChanged: cameraModel==null"); return; }
            if (_lockPropChanged) return;

            _lockPropChanged = true;
            ApertureValueComboBox.SelectedIndex = ApertureValueComboBox.Items.IndexOf(DataConversion.AV((uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Av)));
            ApertureValueLabel.Content = ApertureValueComboBox.SelectedValue;
            _lockPropChanged = false;
        }
        private void HandleTvValueChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("CameraManager==null"); return; };
            if (_lockPropChanged) return;

            _lockPropChanged = true;
            uint TvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Tv);
            string SelectedValue = DataConversion.TV(TvValue);
            ExposureTimeValueComboBox.SelectedIndex = ExposureTimeValueComboBox.Items.IndexOf(SelectedValue);
            ExposureTimeValueLabel.Content = ExposureTimeValueComboBox.SelectedValue;
            Thread.Sleep(cameraModel.standardDelay);
            _lockPropChanged = false;
        }
        private void HandleDriveModeChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("CameraManager==null"); return; };
            if (_lockPropChanged) return; 

            uint data = (uint)cameraModel.DrMValue;
            ShowDriveModeIcon(data);
        }
        private void HandleMeterModeChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleMeterModeChanged: CameraManager==null"); return; };
            if (_lockPropChanged) return; 

            ShowMeterModeIcon();
        }
        private void HandleAFModeChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleAFModeChanged: CameraManager==null"); return; };
            if (_lockPropChanged) return; 

            Show_AFM_button();
        }
        private void HandleEVCChanged(object sender, EventArgs e)
        {
            SetupEvcPointer();
        }
        private void HandleBatteryLevelChanged(object? sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleBatteryLevelChanged: cameraModel==null"); return; };

            cameraModel.BatteryLevelChanged -= HandleBatteryLevelChanged;
            ShowBatteryLevel(cameraModel.BatteryLevel);
            cameraModel.BatteryLevelChanged += HandleBatteryLevelChanged;
        }
        #endregion

        // Handle LiveView, TakePicture and Stacking
        #region
        private void LiveViewButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (cameraModel == null) { return; }

            // During TakePicture LiveView may not start 
            if (cameraModel.LockLiveView) return;

            // LiveView is not allowed in bulb mode
            uint aemodeNr = cameraModel.GetSetting(EDSDK.PropID_AEMode);
            if (aemodeNr == 4) return;

            // For DSLR cameras the AF switch on the camera must be set to "AF ON"
            if ((cameraModel.AFMValue == 3) && (cameraModel.CameraType == "DSLR")) {
                ReportError("Set the AF switch on the camera to 'AF ON'!");
                return;
            }
            // Toggle the LiveView on/off behavior
            if (_nextLvState == LvState.LiveViewON) {
                // The next click will switch LiveView OFF
                _nextLvState = LvState.LiveViewOFF;

                // Face detection modes are not allowwed 
                if ((cameraModel.CameraType == "DSLR") && (cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.LiveFaceMode)) {
                    cameraModel.EVF_AutoFocusMode  = LiveViewAutoFocus.LiveAF_Mode;
                    cameraModel.EVF_AutoFocusModeStr = "Live";
                }
                else 
                if ((cameraModel.CameraType == "DSLM") && (cameraModel.LvAFValue == 2) ){
                    // No LiveView+Facedetectionmode !!
                    DSLM_AFMUp_Click(sender, e);
                    DSLM_AFMUp_Click(sender, e);
                }

                // Activate Live View Mode
                PrepareLiveView();

                // Activate Autofocus and draw the zoom+Focus rectangles
                Evf_AutofocusButton_Click(sender, e);
                cameraModel.StopEVF_Autofocus();
                _FocusInfoTimer.Interval = cameraModel.waitTime_FocusInfo;   // show Focus and zoom rectangles
                _FocusInfoTimer.Enabled = true;
            }
            // Deactivate Live View Mode
            else {
                StopLiveView();
            }
        }
        private void PrepareLiveView()
        {
            if (cameraModel == null) { ReportError("PrepareLiveView: cameraModel==null"); return; }

            // Start LiveView mode and change LiveView "Start" - Button Text to "Stop"
            SwitchToLiveView();

            // Show zoom and AF rectangles
            ZoomRectCanvas.Visibility = Visibility.Visible;
            LiveViewCanvas.Visibility = Visibility.Visible;

            // Update Start/Stop -Template, hide LogoImage
            cameraModel.LvStartStop = "Stop";
            cameraModel.LiveViewIcon = "../Resources/LiveViewIcons/LiveView_green.png";
            LogoImage.Visibility = Visibility.Hidden;

            // Show the LiveViewControlsExpander
            LiveViewControlsExpander.IsExpanded = true;

            // Initialize the LiveView control: Histogram, Zoom, Focus 
            Initialize_Controls_for_LiveView();

            // Deactivate the "Define Picture Path"-Group-Box
            PicturePathGroupBox.IsEnabled = false;
            PicturePathGroupBox.Opacity = 0.5;

            // show EvfAF button in black
            cameraModel.EVF_AFButtonIcon = "../Resources/LiveViewIcons/Crosslines.png";
        }
        private void SwitchToLiveView()
        {
            if (cameraModel == null) { ReportError("SwitchToLiveView: cameraModel==null"); return; }

            AutoResetEvent syncEvent = new(false);

            // activate the Lock for the TakePicture Button
            cameraModel.LockTakePicture = true;

            // Make the Live View Image visible           
            LiveViewImage.Visibility = Visibility.Visible;
            LiveViewImage.Opacity = 1;
            ExtensionMethods.Refresh(LiveViewImage);

            // Start Host Live View Mode
            cameraModel.IsInLiveViewMode = true;
            cameraModel.LiveViewDevice = LiveViewDevice.Host;

            // Start the _FocusInfoTimer
            syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);
            cameraModel.SafeCall(() =>
            {
                if ((cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.LiveAF_Mode)
                    || (cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.QuickAF_Mode)
                    || (cameraModel.CameraType == "DSLM") || (cameraModel.CameraType == "DSLR")) {
                    _FocusInfoTimer.Interval = cameraModel.waitTime_FocusInfo;
                    Thread.Sleep(100);
                    _FocusInfoTimer.Enabled = true;
                }
            }, ex => ReportError(ex.ToString() + "Failed to start LiveView."));
            syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);

            // deactivate the Lock for the TakePicture Button
            cameraModel.LockTakePicture = false;
        }
        private void StopLiveView()
        {
            if (cameraModel == null) { ReportError("StopLiveView: cameraModel==null"); return; }

            // The next click will switch live view off
            _nextLvState = LvState.LiveViewON;

            // Stop the _FocusInfoTimer
            AutoResetEvent syncEvent = new(false);
            cameraModel.SafeCall(() => {
                _FocusInfoTimer.Enabled = false;
            }, ex => ReportError(ex.ToString() + "Failed to start LiveView."));
            syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);

            // Update Template and Icon on the Live View button
            cameraModel.LvStartStop = "Start";
            cameraModel.LiveViewIcon = "../Resources/LiveViewIcons/LiveView_black.png";

            // Activate the "Define Picture Path" - Group - Box
            PicturePathGroupBox.IsEnabled = true;
            PicturePathGroupBox.Opacity = 1.0;

            // Reset the Visibility of the buttons for DSLR AF modes, DSLM AF modes and Stacking
            Reset_DSLR_DSLM_Controls();

            // Hide zoom and AF rectangles
            HideFocusAndZoomRectangles();
            ZoomRectCanvas.Visibility = Visibility.Hidden;
            LiveViewCanvas.Visibility = Visibility.Hidden;

            // Deactivate the Histogram Timer
            cameraModel.EvfHistogramActive = EnableState.disabled;
            _HistogramTimer.Enabled = false;
            HistogramCanvas.Visibility = Visibility.Hidden;
            cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon.png";

            // Hide the LiveViewControlsExpander
            LiveViewControlsExpander.IsExpanded = false;

            // Hide liveViewImage        
            LiveViewImage.Visibility = Visibility.Hidden;

            //  Stop LiveView mode
            cameraModel.LockLiveView = false;
            cameraModel.IsInLiveViewMode = false;
            cameraModel.LockTakePicture = false;
        }
        private void TakePictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null  ) { ReportError("cameraModel==null"); return; }
            if (cmdProcessor == null) { ReportError("cmdProcessor==null"); return; }
            if (cameraModel.LockTakePicture) return;

            // In LiveView mode no Bulb operations are allowed
            if (   (cameraModel.IsInLiveViewMode) && (cameraModel.AEMode == 4)) {
                ReportError("In LiveView mode no Bulb operations are allowed");
                StopLiveView(); 
                return;
            }

            // If this is the 2nd click in bulb mode continue direct to closing the shutter.
            if (cameraModel.WaitFor_2ndClick) {
                TakePictureBulbMode_2ndClick();
                cameraModel.WaitFor_2ndClick = false;
                return;
            }
            // Handle the first click:  block additional LiveViewButton clicks
            // Remember current LiveView state (will be reset when the picture is taken)
            if (cameraModel.IsInLiveViewMode) {
                _savedLvState = LvState.LiveViewON;
                cameraModel.LockLiveView = true;
            }
            else _savedLvState = LvState.LiveViewOFF;

            // First click:  Shutter is closed (i.e. bulb mode not active): Handle all other shooting modes
            if ((cameraModel.ShutterIsClosed) && (cameraModel.AEMode != 4)) {
               TakePicture_normalMode();
            }
            // Handle first click in Bulb mode
            else {   
              TakePictureBulbMode_1stClick();
            }
        } 
        private void TakePicture_normalMode()
        {
            if (cameraModel == null)  { ReportError("cameraModel==null");   return; }
            if (cmdProcessor == null) { ReportError("TakePicture_normalMode: cmdProcessor==null"); return; }

            lock (_takePictureLock) {
                cameraModel.IsInLiveViewMode = false;
                SetPictureSaveDestination();

                // Take the picture in all shooting modes except bulb mode
                AutoResetEvent syncEvent = new(false);
                cmdTakePicture cmd = new(syncEvent);
                cmdProcessor.enqueueCmd(cmd);
                syncEvent.WaitOne(new TimeSpan(0, 0, 2), false);

                TakePictureFinale();
            }
        }
        private void TakePictureBulbMode_1stClick()
        {
            if (cameraModel == null)   { ReportError("cameraModel==null");     return; }
            if (cmdProcessor == null) { ReportError("TakePictureBulbMode_1stClick: cmdProcessor==null"); return; }

            // in Bulb Mode pictures cannot be stored on the host
            storePicturesOnCameraRadioButton.IsChecked = true;
            _saveToCamera = true;
            cameraModel!.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Camera);
            browseOnHostButton.IsEnabled = false;
            pathOnHostTextBox.IsEnabled  = false;

            SetPictureSaveDestination();
            switch (cameraModel.IsBulbTimerSet) {
                case true:
                    // compute the bulb time in seconds
                    cameraModel.BTCounter = cameraModel.BTHours * 3600 + cameraModel.BTMinutes * 60 + cameraModel.BTSeconds;

                    // Set the bulb timer Icon to red: it will be reset to gray in "TakePictureFinale"
                    cameraModel.BulbTimerIcon = "../Resources/BulbTimerIcons/BulbTimerIcon_red.png";

                    // Start Timer controlled Bulb shooting
                    AutoResetEvent syncEvent = new(false);
                    cmdTimedBulb cmd1 = new(syncEvent);
                    cmdProcessor.enqueueCmd(cmd1);
                    syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);

                    TakePictureFinale();
                    break;

                case false:  // Bulb timer is not set: the first click will only open the shutter
                    syncEvent = new AutoResetEvent(false);
                    cmdBulbStart cmd2 = new(syncEvent);
                    cmdProcessor.enqueueCmd(cmd2);
                    syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);

                    cameraModel.ShutterIsClosed  = false;
                    ShutterOpenDot.Visibility   = Visibility.Visible;
                    cameraModel.WaitFor_2ndClick = true;
                    break;
            }
        }
        private void TakePictureBulbMode_2ndClick()
        {
            if (cameraModel == null) { ReportError("TakePictureBulbMode_2ndClick: cameraModel==null"); return; }

            // Normal BulbEnd command
            uint error = EDSDK.EdsSendCommand(cameraPtr, EDSDK.CameraCommand_BulbEnd, 0);

            // Undocumented BulbEnd command at least for:  Rebel Ti1= Eos 500D, Rebel Ti2=Eos 550D und Eos 60D
            if (error == EDSDK.EDS_ERR_INVALID_PARAMETER)
                error = EDSDK.EdsSendCommand(cameraPtr, 0x00000004, 0);

            // Unlock camera UI
            error = EDSDK.EdsSendStatusCommand(cameraPtr, EDSDK.CameraState_UIUnLock,0);

            // now the shutter is closed, restore original LiveView state
            cameraModel.ShutterIsClosed = true;
            ShutterOpenDot.Visibility = Visibility.Hidden;

            TakePictureFinale();
        }
        private void TakePictureFinale()
        {
            if (cameraModel == null) { ReportError("TakePictureFinale: cameraModel==null"); return; }

            // Hide the ProgressBarGroup in case it was visible during image transfer to the Host
            ProgressBarGroup.Visibility = Visibility.Hidden;

            // Display the number of shots
            if (storePicturesOnCameraRadioButton.IsChecked == true) {
                avShotsStackpanel.Visibility = Visibility.Visible;
                // After changing "storePicturesOnHost" to "storePicturesOnCamera" the camera gives a wrong value
                if (cameraModel.AvailableShots < 100000)
                    AvShots.Text = cameraModel.AvailableShots.ToString();
            }
            else {
                avShotsStackpanel.Visibility = Visibility.Hidden;
            }

            // Allow the next'TakePicture' and "LiveView" clicks
            cameraModel.LockTakePicture = false;

            if (_savedLvState == LvState.LiveViewON) {

                // Stop the _FocusInfoTimer
                AutoResetEvent syncEvent = new(false);
                cameraModel.SafeCall(() => {
                    _FocusInfoTimer.Enabled = false;
                }, ex => ReportError(ex.ToString() + "Failed to start LiveView."));
                syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);
                Thread.Sleep(cameraModel.standardDelay);

                SwitchToLiveView();
                cameraModel.IsInLiveViewMode = true;
                cameraModel.LiveViewDevice = LiveViewDevice.Host;
                cameraModel.LockLiveView = false;
            }   
            else {
                cameraModel.LiveViewDevice = LiveViewDevice.Camera;
                cameraModel.IsInLiveViewMode = false;
            }
        }
        static private void PressHalfway()
        {
            if (cameraModel == null) { ReportError("PressHalfway: cameraModel==null"); return; }
            if (cmdProcessor == null) { ReportError("PressHalfway: cmdProcessor==null"); return; }

            EDSDK.EdsSendCommand(cameraPtr, EDSDK.CameraCommand_PressShutterButton, 0);
            EDSDK.EdsSendCommand(cameraPtr, EDSDK.CameraCommand_PressShutterButton, 1);
            EDSDK.EdsSendCommand(cameraPtr, EDSDK.CameraCommand_PressShutterButton, 0);
        }

        private void FocusStackingButton_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (cameraModel == null) { ReportError("FocusStackingButton_Click: cameraModel==null"); return; }
            if (cameraModel.IsInLiveViewMode) { return; }

            // The AF switch on the camera must be set to "AF OFF"
            if (cameraModel.AFMValue != 3) {
                AFOFF_Hint.Visibility = Visibility.Visible;
                return;
            }
            else  
                AFOFF_Hint.Visibility = Visibility.Hidden;

            // Ensure that the StopLv-Button will stop LiveView / Stacking
            _nextLvState = LvState.LiveViewOFF;  

            // Start LiveView mode and change LiveView "Start" - Button Text to "Stop"
            SwitchToLiveView();

            // Hide zoom and AF rectangles
            ZoomRectCanvas.Visibility = Visibility.Hidden;
            LiveViewCanvas.Visibility = Visibility.Hidden;

            // Update Start/Stop -Template, hide LogoImage
            cameraModel.LvStartStop = "Stop";
            cameraModel.LiveViewIcon = "../Resources/LiveViewIcons/LiveView_green.png";
            LogoImage.Visibility = Visibility.Hidden;

            // Show the LiveViewControlsExpander
            LiveViewControlsExpander.IsExpanded = true;

            // Show the focus stacking controlsInitialize_Controls_for_Stacking
            Initialize_Controls_for_Stacking();

            // Deactivate the "Define Picture Path"-Group-Box
            PicturePathGroupBox.IsEnabled = false;
            PicturePathGroupBox.Opacity = 0.5;
        }
        #endregion

        // Handle Iso-, Tv-, Av-ComboBox selections
        #region
        private void Handle_IsoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraModel == null) { ReportError("Handle_IsoComboBox_SelectionChanged: cameraModel==null"); return; };
            if (IsoComboBox.SelectedValue == null) return;

            // Setup the IsoLabel text
            string s = IsoComboBox.SelectedValue?.ToString() ?? string.Empty;
            if (s == "Auto ISO")
                s = "Auto";
            else {
                // remove "ISO " from s
                char[] anyOf = " ".ToCharArray();
                s = IsoComboBox.SelectedValue?.ToString() ?? string.Empty;
                int at = s.LastIndexOfAny(anyOf) + 1;
                s = s.Substring(at);
            }
            cameraModel.ISOSpeedStr = s;

            // update iso value in the camera
            if (IsoComboBox.SelectedValue != null)
                cameraModel.ISOSpeed = DataConversion.ISO((string)IsoComboBox.SelectedValue);
        }
        private void Handle_ApertureValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraModel == null) { ReportError("Handle_ApertureValueComboBox_SelectionChanged: cameraModel==null"); return; }

            string? av = ApertureValueComboBox.SelectedValue as string;
            if (av != null) {
                if (_lockPropChanged == true) return;

                ApertureValueLabel.Content = av;
                cameraModel.AvValue = DataConversion.AV(av);
            }
        }
        private void Handle_ExposureTimeValueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cameraModel == null) { ReportError("Handle_ExposureTimeValueComboBox_SelectionChanged: cameraModel==null"); return; }
            string? tv = ExposureTimeValueComboBox.SelectedValue as string;
            if (tv != null) {
                ExposureTimeValueLabel.Content = tv;
                uint uint_tv = DataConversion.TV(tv);
                cameraModel.SetPropertyIntegerData(EDSDK.PropID_Tv, uint_tv);
            }
        }
        #endregion

        // Handle drive-, meter- , AF-, EVC- controls
        #region
        private void DriveModeUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            Setup_DriveMode_Button(1);
        }
        private void DriveModeDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return; 
            Setup_DriveMode_Button(2);
        }
        private void MeterModeUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;
            Setup_MeterMode_Button(1);
        }
        private void MeterModeDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return; 
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode ) return;
            
            Setup_MeterMode_Button(2);
        }
        private void DSLR_AFMUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;
            Setup_DSLR_AFMode_Button(1);
        }
        private void DSLR_AFMDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;
            Setup_DSLR_AFMode_Button(2);
        }
        private void DSLM_AFMUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLM") && cameraModel.IsInLiveViewMode) return;
            Setup_DSLM_AFMButton(1);
        }
        private void DSLM_AFMDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLM") && cameraModel.IsInLiveViewMode) return;
            Setup_DSLM_AFMButton(2);
        }
        private void LvAFUp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   // LvAFUp_Click: cycle through the Live View Autofocus Modes
            // The button is only visible for DSLR cameras
            // and blocked by 'IsInLiveViewMode' while LiveView is active

            if (cameraModel == null) return;
            if (cmdProcessor == null) { ReportError("cmdProcessor==null"); return; };
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

            switch (cameraModel.EVF_AutoFocusMode) {
                case LiveViewAutoFocus.LiveAF_Mode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.QuickAF_Mode;
                    cameraModel.EVF_AutoFocusModeStr = "Quick";
                    break;
                case LiveViewAutoFocus.QuickAF_Mode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.LiveFaceMode;
                    cameraModel.EVF_AutoFocusModeStr = "LvFace";
                    break;
                case LiveViewAutoFocus.LiveFaceMode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.LiveAF_Mode;
                    cameraModel.EVF_AutoFocusModeStr = "Live";
                    break;
            }
        }
        private void LvAFDown_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   // EvfAFModeDown_Click: cycle through the Live View Autofocus Modes
            // The button is only visible for DSLR cameras
            // and blocked by 'IsInLiveViewMode' while LiveView is active

            if (cameraModel == null) return;
            if (cmdProcessor == null) { ReportError("LvAFDown_Click: cmdProcessor==null"); return; };
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

            switch (cameraModel.EVF_AutoFocusMode) {
                case LiveViewAutoFocus.LiveAF_Mode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.LiveFaceMode;
                    cameraModel.EVF_AutoFocusModeStr = "LvFace";
                    break;
                case LiveViewAutoFocus.LiveFaceMode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.QuickAF_Mode;
                    cameraModel.EVF_AutoFocusModeStr = "Quick";
                    break;
                case LiveViewAutoFocus.QuickAF_Mode:
                    cameraModel.EVF_AutoFocusMode = LiveViewAutoFocus.LiveAF_Mode;
                    cameraModel.EVF_AutoFocusModeStr = "Live";
                    break;
            }
        }
        private void EVCRight_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   // Handle the Exposure Value Compensation (Evc) control
            if (cameraModel == null) return;
            if (cameraPtr == IntPtr.Zero) { ReportError("camera"); return; };
            if (cameraModel == null) { ReportError("EVCRight_Click: cameraModel==null"); return; };

            // Get the current Evc values from the camera
            int EvcValueListLength = cameraModel.EVCValueListLength;
            int[] EvcValueList = cameraModel.EVCValueList;
            long Evc = cameraModel.EVCValue;

            // Get the current Evc index
            int EvcIndex;
            for (EvcIndex = 0; EvcIndex < EvcValueListLength; EvcIndex++) {
                if (Evc == EvcValueList[EvcIndex]) break;
            }
            // Proceed to the next hihger Ecv value if the right end of the list is not yet reached and the next value is <= 0x18 = 24  
            int h = Math.Abs((int)Evc);
            if ((EvcIndex < EvcValueListLength - 1) & (h >= 232) | (h < 24))  EvcIndex++;
            cameraModel.EVCValue = EvcValueList[EvcIndex];
            SetupEvcPointer();
        }
        private void EVCLeft_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   // Handle the Exposure Value Compensation (Evc) control
            if (cameraModel == null) return;
            if (cameraPtr == IntPtr.Zero) { ReportError("camera"); return; };
            if (cameraModel == null) { ReportError("EVCLeft_Click: cameraModel==null"); return; };

            // Get the current Evc values from the camera
            int EvcValueListLength = cameraModel.EVCValueListLength;
            int[] EvcValueList = cameraModel.EVCValueList;
            long Evc = cameraModel.EVCValue;

            // Get the current Evc index
            int EvcIndex;
            for (EvcIndex = 0; EvcIndex < EvcValueListLength; EvcIndex++) {
                if (Evc == EvcValueList[EvcIndex]) break;
            }
            // Proceed to the next lower Ecv value if the left end of the list is not yet reached and the next value is >= 0xE8 = 232  
            int h = Math.Abs((int)Evc);
            if ((h <= 24) | (h > 232)) EvcIndex--;
            cameraModel.EVCValue = EvcValueList[EvcIndex];
            SetupEvcPointer();
        }
        #endregion

        // Handle hours-, minutes- and seconds-counters of the bulb timer control
        #region
        private void BTHoursUp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(3600);
        }
        private void BTHoursDown_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(-3600);
        }
        private void BTMinutesUp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(60);
        }
        private void BTMinutesDown_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(-60);
        }
        private void BTSecondsUp_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(1);
        }
        private void BTSecondsDown_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UpdateBulbTimer(-1);
        }
        #endregion

        // Handle controls for save picture destination 
        #region
        private void SaveTo_RdButton_Checked(object sender, RoutedEventArgs e)
        {
            try {
                if (_picturePathOK) {
                    if (storePicturesOnCameraRadioButton.IsChecked == true) {
                        // store picture on camera
                        _saveToCamera = true;
                        cameraModel!.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Camera);
                        browseOnHostButton.IsEnabled = false;
                        pathOnHostTextBox.IsEnabled = false;
                        avShotsStackpanel.Visibility = Visibility.Visible;
                    }
                    else {
                        // store picture on host 
                        _saveToCamera = false;
                        cameraModel!.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
                        //SetCapacity();
                        browseOnHostButton.IsEnabled = true;
                        pathOnHostTextBox.IsEnabled = true;
                        _saveToCamera = false;
                        avShotsStackpanel.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }
        private void BrowseOnHostButton_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) {  return; };
            if (cameraModel.SetSetting == null) { return; };

            try {
                // Set to pathOnHostTextBox.Text
                if (Directory.Exists(pathOnHostTextBox.Text)) _saveFolderBrowser.SelectedPath = pathOnHostTextBox.Text;

                if (_saveFolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    pathOnHostTextBox.Text = _saveFolderBrowser.SelectedPath;   // Set to pathOnHostTextBox.Text
                    _imageSaveDirectory = _saveFolderBrowser.SelectedPath;
                }
                cameraModel.SetSetting(EDSDK.PropID_SaveTo, (uint)EDSDK.EdsSaveTo.Host);
            }
            catch (Exception ex) { ReportError(ex.Message); }
        }
        #endregion

        // Handle events of the LiveView/Stacking   expander
        #region 
        private void Evf_Histogram_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {   // Activate/deactivate the histogram of the live image (_HistogramTimer)

            if (cameraModel == null) { ReportError("Evf_Histogram_Click: cameraModel==null"); return; };
            if ((cameraPtr == IntPtr.Zero) || !(cameraModel.IsInLiveViewMode)) return;

            if (cameraModel.EvfHistogramActive == EnableState.disabled) {
                cameraModel.EvfHistogramActive = EnableState.enabled;
                HistogramCanvas.Visibility = Visibility.Visible;
                cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon_green.png";
                _HistogramTimer.Interval = 1000;
                _HistogramTimer.Enabled = true;
            }
            else {
                cameraModel.EvfHistogramActive = EnableState.disabled;
                _HistogramTimer.Enabled = false;
                HistogramCanvas.Visibility = Visibility.Hidden;
                cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon.png";
            }
        }
        private void Evf_Zoom_Click(object sender, RoutedEventArgs e)
        { 
            if (cameraModel == null) { ReportError("Evf_Zoom_Click: cameraModel==null"); return; };
            if (!cameraModel.IsInHostLiveViewMode) return;

            if (cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.LiveFaceMode) {
                ReportError("EvF image cannot be zoomed in Face detection Live Mode!");
                return;
            }
            cameraModel.StopEVF_Autofocus();
            cameraModel.ZoomRect_Color = GetCyanBrush();
            cameraModel.EVF_AutoFocusIsOn = false;

            switch (cameraModel.EvfZoom) {
                case EvfZoomFactor.fit:
                    do {
                        cameraModel.EvfZoom = EvfZoomFactor.x5;
                    } while (cameraModel.EVF_AutoFocusIsOn);
                    HideFocusAndZoomRectangles();
                    cameraModel.ZoomIcon = "../Resources/LiveViewIcons/ZoomMinus.png";
                    break;

                case EvfZoomFactor.x5:
                    cameraModel.StopEVF_Autofocus();
                    do {
                       cameraModel.EvfZoom = EvfZoomFactor.fit;
                    } while (cameraModel.EVF_AutoFocusIsOn);
                    _FocusInfoTimer.Interval = cameraModel.waitTime_FocusInfo;   // show Focus and zoom rectangles
                    _FocusInfoTimer.Enabled  = true;
                    cameraModel.ZoomIcon   = "../Resources/LiveViewIcons/ZoomPlus.png";
                    break;
            }   
        }
        private void Evf_AutofocusButton_Click(object sender, RoutedEventArgs e)
        {  // Start Evf Autofocus, a second click (within next 3 seconds) stops Evf Autofocus
            if (cameraModel == null) { ReportError("Evf_AutofocusButton_Click: cameraModel==null"); return; }
            if (cmdProcessor == null) { ReportError("cmdProcessor==null"); return; }
            if (!cameraModel.IsInLiveViewMode) return;

            _evf_AFTimer_Elapsed = false;            // prevents, that Mouse_leave resets the Crosslines_green icon
            cameraModel.CrosslinesIcon = "../Resources/LiveViewIcons/Crosslines_green.png";

            // Stop the _FocusInfoTimer
            _FocusInfoTimer.Enabled = false;

            // First Click:  Start Evf Live View Autofocus  
            if (!cameraModel.EVF_AutoFocusIsOn)
            {
                AutoResetEvent syncEvent = new(false);
                cmdDoEvfAF cmd = new(1, syncEvent);
                cmdProcessor.enqueueCmd(cmd);
                syncEvent.WaitOne(new TimeSpan(0, 0, 1), false);

                //Start the _EVF_AutofocusTimer and stop after 3 seconds
                _EVF_AutofocusTimer.Interval = 3000;
                _EVF_AutofocusTimer.Enabled = true;

                // If in Live View QuickAF_Mode (but not in EVFZoom-Mode) update Focusinformation 
                if ((cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.QuickAF_Mode) && (cameraModel.EvfZoom == EvfZoomFactor.fit)) {
                    _FocusInfoTimer.Interval = cameraModel.waitTime_FocusInfo;
                    _FocusInfoTimer.Enabled = true;
                }

                // Set flag "EVF_AutoFocusIsOn" indicating Live View AutoFocus is on. Used in these situations:
                // 1. by StopEVF_Autofocus() to decide if a stop-command is necessry
                // 2. by Evf_AutofocusButton_Click(...) to toggle between EvfAF Mode and normal shooting mode
                // 3. delays toggling between EvfZoomFactor.x5 and EvfZoomFactor.fit until EvfAutofocus is stopped
                cameraModel.EVF_AutoFocusIsOn = true;

                // Change the Focus rectangle FP_0 to green (but not in EvfZoom-Mode)
                if ((cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.LiveAF_Mode) && (cameraModel.EvfZoom == EvfZoomFactor.fit))
                    FP_0.Stroke = GetGreenBrush();
            }
            // Second Click:  Stop Evf Live View Autofocus
            else
            {
                // Second Click: Stop Evf Live View Autofocus
                cameraModel.StopEVF_Autofocus();
                cameraModel.CrosslinesIcon = "../Resources/LiveView/Crosslines.png";
                cameraModel.ZoomRect_Color = GetCyanBrush();
                cameraModel.EVF_AutoFocusIsOn = false;
            }
        }
        private void AFON_Hint_Click(object sender, RoutedEventArgs e)
        {
            AFOFF_Hint.Visibility = Visibility.Hidden;
        }

        // Drive Lens commands
        private void DriveLensLeft (object sender, RoutedEventArgs e) {
            if (cameraModel == null) { ReportError("DriveLensLeft: cameraModel==null"); return; }

            int width = Int32.Parse(cameraModel.FocusStepWidth);
            if (width==1) DriveLens(EvfDriveLens.Near1);
            if (width==2) DriveLens(EvfDriveLens.Near2);
            if (width==3) DriveLens(EvfDriveLens.Near3);    
        }
        private void DriveLensRight(object sender, RoutedEventArgs e) {
            if (cameraModel == null) { ReportError("DriveLensRight: cameraModel==null"); return; }

            int width = Int32.Parse(cameraModel.FocusStepWidth);
            if (width==1) DriveLens(EvfDriveLens.Far1);
            if (width==2) DriveLens(EvfDriveLens.Far2);
            if (width==3) DriveLens(EvfDriveLens.Far3);
        }
        private void DriveLensNear1(object sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Near1); }
        private void DriveLensNear2(object sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Near2); }
        private void DriveLensNear3(object sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Near3); }
        private void DriveLensFar1(object  sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Far1);  }
        private void DriveLensFar2(object  sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Far2);  }
        private void DriveLensFar3(object  sender, RoutedEventArgs e) { DriveLens(EvfDriveLens.Far3);  }
        private static void DriveLens(EvfDriveLens DriveLensCode)
        {
            if (cmdProcessor == null) return;

            // The camera lens switch must be OFF   !!
            AutoResetEvent syncEvent = new(false);
            cmdEvfDriveLens cmd = new(DriveLensCode, syncEvent);
            cmdProcessor.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 3), false);
        }

        // Focus stacking controls
        private void NrFocusStepsUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;

            int steps = Convert.ToInt32(cameraModel.NrFocusSteps);
            steps++;
            if (steps == 0) steps++;
            cameraModel.NrFocusSteps = steps.ToString();
            if (steps > 0) {
                FocusStepsFw1.Visibility = Visibility.Visible;
                FocusStepsFw2.Visibility = Visibility.Visible;
                FocusStepsBw1.Visibility = Visibility.Hidden;
                FocusStepsBw2.Visibility = Visibility.Hidden;
            }
            else {
                FocusStepsFw1.Visibility = Visibility.Hidden;
                FocusStepsFw2.Visibility = Visibility.Hidden;
                FocusStepsBw1.Visibility = Visibility.Visible;
                FocusStepsBw2.Visibility = Visibility.Visible;
            }
        }
        private void NrFocusStepsDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;

            int steps = Convert.ToInt32(cameraModel.NrFocusSteps);
            steps--;
            if (steps == 0) steps--;
            if (steps > 0) {
                FocusStepsFw1.Visibility = Visibility.Visible;
                FocusStepsFw2.Visibility = Visibility.Visible;
                FocusStepsBw1.Visibility = Visibility.Hidden;
                FocusStepsBw2.Visibility = Visibility.Hidden;
            }
            else {
                FocusStepsFw1.Visibility = Visibility.Hidden;
                FocusStepsFw2.Visibility = Visibility.Hidden;
                FocusStepsBw1.Visibility = Visibility.Visible;
                FocusStepsBw2.Visibility = Visibility.Visible;
            }

            cameraModel.NrFocusSteps = steps.ToString();
        }
        private void WidthOfFocusStepsUp_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;

            int steps = Int32.Parse(cameraModel.FocusStepWidth);
            if (steps < 3) steps++;
            else steps = 1;
            cameraModel.FocusStepWidth = steps.ToString();
        }
        private void WidthOfFocusStepsDown_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;
            int steps = Int32.Parse(cameraModel.FocusStepWidth);
            if (steps > 1) steps--;
            else steps =3;
            cameraModel.FocusStepWidth = steps.ToString();
        }
        private void StepSizeChangedEventHandler(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) { ReportError("StepSizeChangedEventHandler: cameraModel==null"); return; }

            bool correctInt = int.TryParse(cameraModel.FocusStepWidth, out int i);
            if (!correctInt) {
                cameraModel.FocusStepWidth = "1";
                return;
            }
            int stepWidth = Int32.Parse(cameraModel.FocusStepWidth);
            if (stepWidth > 3) cameraModel.FocusStepWidth = "3";
            if (stepWidth < 1) cameraModel.FocusStepWidth = "1";
        }
        private void NrFocusStepsChangedEventHandler(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) { ReportError("NrFocusStepsChangedEventHandler: cameraModel==null"); return; }

            bool correctInt = int.TryParse(cameraModel.NrFocusSteps, out int i);
            if (!correctInt) {
                cameraModel.NrFocusSteps = "1";
                return;
            }
            if (cameraModel.NrFocusSteps == "0") cameraModel.NrFocusSteps = "1";

        }

        private void StartStacking_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel == null) return;

            cameraModel.IsInLiveViewMode = true;
            StartStackingEllipseUp.Dispatcher.InvokeAsync(TakeStack, DispatcherPriority.SystemIdle);          
        }
        private void TakeStack()
        {
            if (cameraModel   == null) return;
            if (cmdProcessor == null) return;

            lock (_stackingLock)
            { 
                Thread.Sleep(200);

                AutoResetEvent syncEvent     = new(false);
                EvfDriveLens   driveLensCode = new();

                // Get FocusShift settings 
                int width = Convert.ToInt32(cameraModel.FocusStepWidth);
                int steps = Convert.ToInt32(cameraModel.NrFocusSteps);

                if ((width==1) && (steps>0))   driveLensCode = EvfDriveLens.Far1;
                if ((width==2) && (steps>0))   driveLensCode = EvfDriveLens.Far2;
                if ((width==3) && (steps>0))   driveLensCode = EvfDriveLens.Far3;

                if ((width == 1) && (steps < 0)) driveLensCode = EvfDriveLens.Near1;
                if ((width == 2) && (steps < 0)) driveLensCode = EvfDriveLens.Near2; ;
                if ((width == 3) && (steps < 0)) driveLensCode = EvfDriveLens.Near3;

                // Drive the lens
                syncEvent = new(false);
                cmdEvfDriveLens cmd_DL = new(driveLensCode, syncEvent);
                cmdProcessor.enqueueCmd(cmd_DL);
                syncEvent.WaitOne();

                Thread.Sleep(_delayDriveLens);

                // Take the picture 
                syncEvent = new(false);
                cmdTakePicture cmd_TP = new(syncEvent);
                cmdProcessor.enqueueCmd(cmd_TP);
                syncEvent.WaitOne();

                Thread.Sleep(_delayTakeStack);

                // Update the number of remaining step on the FocusStepsButton
                if (steps > 0) steps--;
                else steps++;
                cameraModel.NrFocusSteps = steps.ToString();

                if (steps != 0) { StartStackingEllipseUp.Dispatcher.InvokeAsync(TakeStack, DispatcherPriority.SystemIdle); }
                if (steps == 0) { TakeStackingFinale(); }
            }
        }
        private void TakeStackingFinale()
        {
            if (cameraModel == null) { ReportError("TakeStackingFinale: cameraModel==null"); return; }

            // Hide the ProgressBarGroup in case it was visible during image transfer to the Host
            ProgressBarGroup.Visibility = Visibility.Hidden;

            // Display the number of shots
            AvShots.Text = cameraModel.AvailableShots.ToString();

            //Return to LiveView
            cameraModel.IsInLiveViewMode = true;
            cameraModel.LiveViewDevice = LiveViewDevice.Host;
            cameraModel.LockLiveView = false;
         }
        #endregion

        // Drag&Drop zoom rectangle
        #region 
        private Point           _startPoint;
        private bool            _isDown;
        private bool            _isDragging;
        private EDSDK.EdsPoint  _point;

        private void EvfFocusCanvas_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape && _isDragging) { DragFinished(true); }
        }
        private void EvfFocusCanvas_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDown) { DragFinished(false); e.Handled = true; }
        }
        private void EvfFocusCanvas_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isDown) {
                if ((_isDragging == false) && ((Math.Abs(e.GetPosition(ZoomRectCanvas).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(ZoomRectCanvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance))) {
                    DragStarted();
                }
                if (_isDragging) { DragMoved(); }
            }
        }
        private void EvfFocusCanvas_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e == null) return;

            if (!(e.Source == ZoomRectCanvas)) {
                _isDown = true;
                _startPoint = e.GetPosition(ZoomRectCanvas);
                _originalElement = (UIElement)e.Source;
                ZoomRectCanvas.CaptureMouse();
                e.Handled = true;

                // Double click on "ZoomRectCanvas" is handled as Evf_AutofocusButton_Click
                if (e.ClickCount == 2) Evf_AutofocusButton_Click(sender, e);
            }
        }
        private void DragStarted()
        {
            if (cameraModel == null) return;

            _isDragging = true;
            _overlayElement = new FocusRectangleAdorner(_originalElement);
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_originalElement);
            layer.Add(_overlayElement);
        }
        private void DragMoved()
        {
            Point CurrentPosition = System.Windows.Input.Mouse.GetPosition(ZoomRectCanvas);
            _overlayElement.LeftOffset = CurrentPosition.X - _startPoint.X;
            _overlayElement.TopOffset = CurrentPosition.Y - _startPoint.Y;
        }
        private void DragFinished(bool cancelled)
        {
            if (cameraModel == null) { ReportError("DragFinished: cameraModel==null"); return; };

            System.Windows.Input.Mouse.Capture(null);
            if (_isDragging)
            {
                AdornerLayer.GetAdornerLayer(_overlayElement.AdornedElement).Remove(_overlayElement);
                if (cancelled == false)
                {
                    // Update _camera.ZoomPosition: // enforce zoom rectangle to stay within image area
                    double scaleFactor = (Width - 30 - 8) / cameraModel.LocalFocusInformation.Bounds.Width;
                    _point.x = (int)(cameraModel.ZoomPosition.x + (_overlayElement.LeftOffset / scaleFactor));
                    _point.y = (int)(cameraModel.ZoomPosition.y + (_overlayElement.TopOffset  / scaleFactor));

                    if (_point.x > (double)(cameraModel.LocalFocusInformation.Bounds.Width) / 5 * 4) {
                        _point.x = (int)(double)(cameraModel.LocalFocusInformation.Bounds.Width) / 5 * 4;
                        cameraModel.ZoomRect_X = _point.x * (Width - 30 - 8) / cameraModel.LocalFocusInformation.Bounds.Width;
                    }
                    if (_point.y > (double)(cameraModel.LocalFocusInformation.Bounds.Height) / 5 * 4) {
                        _point.y = (int)(double)(cameraModel.LocalFocusInformation.Bounds.Height) / 5 * 4;
                        cameraModel.ZoomRect_Y = _point.y * (Width - 30 - 8) / cameraModel.LocalFocusInformation.Bounds.Width;
                    }
                    if (_point.x < 0) { _point.x = 0; cameraModel.ZoomRect_X = 0; }  
                    if (_point.y < 0) { _point.y = 0; cameraModel.ZoomRect_Y = 0; }

                    // Set the camera zoom position 
                    cameraModel.ZoomPosition = _point;

                    // Set flag for the _FocusInfoTimer
                    _zoomPositionValid = true;
                }
                _FocusInfoTimer.Enabled = true;
            }
            _isDragging = false;
            _isDown = false;
        }
        #endregion

        // Watcher-, WindowClosed, WindowSizeChanged-, Restart-Handler
        #region
        // OnWatcher_EventArrived handler: restart the application when a camera is removed or inserted 
        public void OnWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            // if a camera is inserted / removed multiple events may occur
            // WatcherCounter makes shure that only one of them is used
            if (watcherCounter == 0) {
                watcherCounter++;
                System.Windows.Forms.Application.Restart();
                watcher.Dispose();
                Environment.Exit(0);
            }
        }

        // HandleWindowClosed + Exit-Button handler: Release the SDK and Shutdown the cameraModel
        private void HandleWindowClosed(object sender, EventArgs e)
        {
            if (cameraModel == null) { Environment.Exit(0); }

            // Stop the _FocusInfoTimer and LiveView
            _FocusInfoTimer.Enabled = false;
            cameraModel.LiveViewDevice = LiveViewDevice.Camera;
            cameraModel!.IsInLiveViewMode = true;
            _efvImage = IntPtr.Zero;

            //Stop cmdProcessor()
            cmdProcessor!.stopCmd();
            _syncCmdProcessor.Reset();
            threadPoolWorker.Work(work: () => { cmdProcessor.clear(); });
            _syncCmdProcessor.WaitOne(new TimeSpan(0, 0, 2), false);

            // Close Session and EDSDK
            cameraModel!.CloseSession();
            Error = EDSDK.EdsTerminateSDK();

            Environment.Exit(0);
        }

        // Resize the Main Window and focus markers preserving the picture box proportion = 3:2 
        private void HandleMainWindowSizeChanged(object sender, EventArgs e)
        {
            if (cameraModel == null) { ReportError("HandleMainWindowSizeChanged: cameraModel==null"); return; };

            // Enforce Widt/Height ratio = 3:2
            Height = (2 * (Width - 30 - 8) / 3) + 30 + 30;

            // Scale factor "actual window size" / "original window size"
            double scale = (Width - 30 - 8) / (_oldWindowWidth - 30 - 8);

            // save this width/height as old width/height
            _oldWindowWidth = Width;

            // resize Zoom Rectangle
            cameraModel.ZoomRect_Height *= scale;
            cameraModel.ZoomRect_Width *= scale;
            cameraModel.ZoomRect_X *= scale;
            cameraModel.ZoomRect_Y *= scale;
        }

        // Restart button handler
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (cameraModel != null)
            {
                // Stop the _FocusInfoTimer, LiveView,cmdProcessor
                _FocusInfoTimer.Enabled = false;
                cameraModel!.IsInLiveViewMode = false;
                _efvImage = IntPtr.Zero;
                cmdProcessor!.stopCmd();

                // Close Session and EDSDK
                cameraModel!.CloseSession();
                Error = EDSDK.EdsTerminateSDK();
            }
            // Restart application
            System.Windows.Forms.Application.Restart();
            uint error = EDSDK.EdsTerminateSDK();
            watcher.Dispose();
            Environment.Exit(0);
        }
        #endregion

        // Timer handling
        #region 
        // HandleProgressBar: Updates the progress bar 
        public void HandleProgressBar(object sender, ProgressEventArgs e)
        {
            ShowProgress(e.PercentComplete);
            if (e.PercentComplete == 100) ProgressBarGroup.Visibility = Visibility.Hidden;
        }

        // Timer for resetting Evf AF Mode button after 5 seconds
        public System.Timers.Timer _EVF_AutofocusTimer = new();
        public void HandleEVFAutofocusTimer(object source, ElapsedEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; };

            _EVF_AutofocusTimer.Enabled = false;
            _evf_AFTimer_Elapsed = true;

            cameraModel.StopEVF_Autofocus();   
            ResetAutoFocusActiveColors();
            cameraModel.CrosslinesIcon = "../Resources/LiveViewIcons/Crosslines.png";
        }

        // FocusInfoTimer: get FocusInformation and reset Zoom/Focus rectangles, started by SwitchToLiveView()
        public System.Timers.Timer _FocusInfoTimer = new();
        private delegate void HandleFocusInfoTimerDelegate();
        public void HandleFocusInfoTimer(object source, ElapsedEventArgs e)
        {
            if (ThisWindow.Dispatcher.CheckAccess()) {
                UpdateFocusInfo();
            }
            else {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new HandleFocusInfoTimerDelegate(UpdateFocusInfo));
            }
        }

        // HistogramTimer: Displays the Evf Histogram continuously
        public System.Timers.Timer _HistogramTimer = new();
        private delegate void HandleHistogramTimerDelegate();
        public void HandleHistogramTimer(object source, ElapsedEventArgs e)
        {
            if (ThisWindow.Dispatcher.CheckAccess()) {
               ShowHistogram();
            }
            else {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                             new HandleHistogramTimerDelegate(ShowHistogram));
            }
        }
        #endregion

    }

}
