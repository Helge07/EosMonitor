using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using EDSDKLib;

namespace EosMonitor
{
    public partial class CameraModel : Object, INotifyPropertyChanged
   {
        // Constructor, open/close session, initialize camera features
        #region 
        // Constructor
        internal CameraModel(IntPtr camera) : base(camera)
        {
            // Initialize the camera properties to default values
            #region
            _Artist = string.Empty;
            _Copyright = string.Empty;
            _BatteryIcon = string.Empty;
            _CameraDate = string.Empty;
            _CameraTime = string.Empty;
            _FirmwareVersion = string.Empty;
            _OwnerName = string.Empty;
            _ProductName = string.Empty;
            _SerialNumber = string.Empty;
            _LvStartStop = string.Empty;
            _LiveViewIcon = string.Empty;
            _AEModeStr = string.Empty;
            _AEModeIcon = string.Empty;
            _AFModeIcon = string.Empty;
            _LvAFStr = string.Empty;
            _BTHoursStr = string.Empty;
            _BTMinutesStr = string.Empty;
            _BTSecondsStr = string.Empty;
            _BulbTimerIcon = string.Empty;
            _ApertureStr = "Av-VALUE";
            _AFModeStr = string.Empty;
            _DrModeIcon = string.Empty;
            _ISOSpeedStr = string.Empty;
            _MModeIcon = string.Empty;
            _AESpeedStr = string.Empty;
            _LiveViewImageSource = null!;
            _LiveViewHistogram = null!;
            _PreviewImageSource = null!;
            _ZoomRect_Color = null!;
            _ZoomIcon = string.Empty;
            _EVF_AFButtonIcon = string.Empty;
            _HistogramIcon = string.Empty;
            _CrosslinesIcon = string.Empty;
            _edsObjectEventHandler = null!;
            _edsPropertyEventHandler = null!;
            _edsStateEventHandler = null!;
            _edsProgressCallbackHandler = null!;
            _IsInLiveViewMode = false;
            Shutdown = null!;
            BulbExposureTime = null!;
            ConfPropChanged = null!;
            AvValueChanged = null!;
            TvValueChanged = null!;
            IsoSpeedChanged = null!;
            DriveModeChanged = null!;
            MeterModeChanged = null!;
            EvcChanged = null!;
            AFModeChanged = null!;
            BatteryLevelChanged = null!;
            EvfOutputDeviceChanged = null!;
            AEModeChanged = null!;
            PictureTakenTransfer = null!;
            PictureTaken = null!;
            VolumeInfoChanged = null!;
            OnProgressMade = null!;
            PropertyChanged = null!;
            _picturePath = string.Empty;
            _CameraType = string.Empty;
            LockLiveView = false;
            WaitFor_2ndClick = false;
            NrFocusSteps ="1";
            FocusStepWidth= "1";
            #endregion

            // Get the 'DeviveInfo' of the _camera
            MainWindow.Error = EDSDK.EdsGetDeviceInfo(MainWindow.cameraPtr, out _deviceInfo);
            uint error = EDSDK.EdsGetDeviceInfo(camera, out _deviceInfo);
            if (error != (uint)ErrorCode.Ok) MainWindow.ReportError("SDK Error:  Device Info could not be retrieved for " + DeviceName);

            // register callback handlers for EDSDK events
            RegisterCallbackHandlers();

            // Overtake the an open camera session, otherwise dispose unused memory
            IsSessionOpen = MainWindow._sessionIsOpen;
            if (!IsSessionOpen) Dispose();
        }

        // getCameraProperties:  general features of the camera model are fetched from the camera
        public void getCameraProperties()
        {
            _Artist = GetPropertyStringData(EDSDK.PropID_Artist);
            _Copyright = GetPropertyStringData(EDSDK.PropID_Copyright);
            _BatteryLevel = GetPropertyIntegerData(EDSDK.PropID_BatteryLevel);
            _FirmwareVersion = GetPropertyStringData(EDSDK.PropID_FirmwareVersion);
            _OwnerName = GetPropertyStringData(EDSDK.PropID_OwnerName);
            _ProductName = GetPropertyStringData(EDSDK.PropID_ProductName);
            _SerialNumber = GetPropertyStringData(EDSDK.PropID_BodyIDEx);
            _Time = GetPropertyStruct<EDSDK.EdsTime>(EDSDK.PropID_DateTime, EDSDK.EdsDataType.Time);
            _EVF_AutoFocusMode = LiveViewAutoFocus.LiveAF_Mode;
            EvfZoom = EvfZoomFactor.fit;
            picturePath = "";
            ShutterIsClosed = true;
            ShutterButtonState = ShutterButtonState.Off;
            LiveShooting = EnableState.enabled;
            IsInLiveViewMode = false;
        }

        // OpenSession establishes a logical connection to the camera object 
        public void OpenSession()
        {
            CheckDisposed();
            try {
                if (!IsSessionOpen) {
                    uint error = EDSDK.EdsOpenSession(MainWindow.cameraPtr);
                    if (error != (uint)ErrorCode.Ok) {
                        IsSessionOpen = false;
                        error = EDSDK.EdsCloseSession(MainWindow.cameraPtr);
                        throw new EosException(error, "Cannot open camera session to : " + DeviceName);
                    }
                    IsSessionOpen = true;
                }
            }
            catch (EosException ex) {
                System.Windows.MessageBox.Show(ex.EosErrorMessage + " : " + ex.EosErrorCode.ToString(), "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Close the session with the current camera
        public void CloseSession()
        {
            if (!IsSessionOpen) {
                //if live view is still on, stop it and wait until the thread has stopped
                if (IsInLiveViewMode) {
                    IsInLiveViewMode = false;
                    LiveViewDevice = LiveViewDevice.Camera;
                }
                //Remove callback handlers for EDSDK events
                _ = EDSDK.EdsSetCameraStateEventHandler(MainWindow.cameraPtr, EDSDK.StateEvent_All, null!, IntPtr.Zero);
                _ = EDSDK.EdsSetObjectEventHandler(MainWindow.cameraPtr, EDSDK.ObjectEvent_All, null!, IntPtr.Zero);
                _ = EDSDK.EdsSetPropertyEventHandler(MainWindow.cameraPtr, EDSDK.PropertyEvent_All, null!, IntPtr.Zero);

                //close session and release camera
                _ = EDSDK.EdsCloseSession(MainWindow.cameraPtr);
                _ = EDSDK.EdsRelease(MainWindow.cameraPtr);
                IsSessionOpen = false;
            }
        }

        // test if the camera is supported by the SDK
        public bool IsEosCamera()
        {
            if (MainWindow.cameraPtr != IntPtr.Zero) {
                uint Error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, EDSDK.PropID_Artist, 0, out string StrArtist);
                if (Error != EDSDK.EDS_ERR_OK) {
                    MainWindow.ReportError("cameraModel is not accepted because it is not an EOS camera:  " + DeviceName);
                    Application.Current.Shutdown();
                }
            }
            return true;
        }
        #endregion

        // Callback handlers
        #region 
        // Declare EDSDK callback handlers
        private EDSDK.EdsObjectEventHandler   _edsObjectEventHandler;
        private EDSDK.EdsPropertyEventHandler _edsPropertyEventHandler;
        private EDSDK.EdsStateEventHandler    _edsStateEventHandler;
        private EDSDK.EdsProgressCallback     _edsProgressCallbackHandler;

        // Declare Custom Event handlers
        public event EventHandler Shutdown;
        public event EventHandler BulbExposureTime;
        public event EventHandler ConfPropChanged;
        public event EventHandler IsoSpeedChanged;
        public event EventHandler AvValueChanged;
        public event EventHandler TvValueChanged;
        public event EventHandler DriveModeChanged;
        public event EventHandler MeterModeChanged;
        public event EventHandler AFModeChanged;
        public event EventHandler EvcChanged;
        public event EventHandler BatteryLevelChanged;

        public event EventHandler<EvfOutputDeviceChangedEventArgs>  EvfOutputDeviceChanged;
        public event EventHandler<AEModeChangedEventArgs>           AEModeChanged;
        public event EventHandler<ImageEventArgs>                   PictureTakenTransfer;
        public event EventHandler<EventArgs>                        PictureTaken;
        public event EventHandler<VolumeInfoEventArgs>              VolumeInfoChanged;
        public event EventHandler<ProgressEventArgs>                OnProgressMade;
        public event PropertyChangedEventHandler?                   PropertyChanged;

        // Register EDSDK callback handlers for State-, Object- and Property events
        private void RegisterCallbackHandlers()
        {
            uint error = 0;
            try {
                // Register EDSDK callback handler for state events
                _edsStateEventHandler = new EDSDK.EdsStateEventHandler(HandleStateEvent);
                error = EDSDK.EdsSetCameraStateEventHandler(MainWindow.cameraPtr, EDSDK.StateEvent_All, _edsStateEventHandler, IntPtr.Zero);
                if (error != (uint)ErrorCode.Ok)
                    throw new EosException(error, "Cannot register EdsStateEvent_All handler");

                // Register callback handler for object events
                _edsObjectEventHandler = new EDSDK.EdsObjectEventHandler(HandleObjectEvent);
                error = EDSDK.EdsSetObjectEventHandler(MainWindow.cameraPtr, EDSDK.ObjectEvent_All, _edsObjectEventHandler, IntPtr.Zero);
                if (error != (uint)ErrorCode.Ok)
                    throw new EosException(error, "Cannot register EdsObjectEvent_All handler");

                // Register callback handler for property events
                _edsPropertyEventHandler = new EDSDK.EdsPropertyEventHandler(HandlePropertyEvent);
                error = EDSDK.EdsSetPropertyEventHandler(MainWindow.cameraPtr, EDSDK.PropertyEvent_All, _edsPropertyEventHandler, IntPtr.Zero);
                if (error != (uint)ErrorCode.Ok)
                    throw new EosException(error, "Cannot register EdsPropertyEvent_All handler");
            }
            catch (EosException ex) {
                MessageBoxResult messageBoxResult = MessageBox.Show(ex.EosErrorMessage + " : " + ex.EosErrorCode.ToString(), "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Register handler for ProgressMade / ProgressCallback events
        public  void RegisterProgressMadeEventHandler(IntPtr camera, EDSDK.EdsProgressOption option)
        {
            uint error = 0;
            try {
                _edsProgressCallbackHandler = new EDSDK.EdsProgressCallback(HandleProgressCallbackEvent);
                error = EDSDK.EdsSetProgressCallback(camera, _edsProgressCallbackHandler, option, IntPtr.Zero);
                if (error != (uint)ErrorCode.Ok) {
                    throw new EosException(error, "Cannot register ProgressCallback handler");
                }
            }
            catch (EosException ex) {
                MessageBox.Show(ex.EosErrorMessage + " : " + ex.EosErrorCode.ToString(), "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private uint HandleProgressCallbackEvent(uint inPercent, IntPtr inContext, ref bool outCancel)
        {
            OnProgressMade?.Invoke(this, new ProgressEventArgs((int)inPercent));
            return 0x0;
        }
        #endregion

        // cameraModel constants 
        #region 
        const int    maxLength_Copyright    = 64;
        const  int   maxLenght_Artist       = 64;
        const  int   maxLength_OwnerName    = 32;
        public int   standardDelay          = 500;
        public int   waitTime_FocusInfo     = 2000;    
        #endregion
                    
        // cameraModel properties
        #region
        // Raise a property changed event
        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        // Artist: artist name
        private string _Artist;
        public  string Artist { 
            get { return _Artist; }
            set { 
            _Artist = value;
            SetPropertyStringData(EDSDK.PropID_Artist, value, maxLenght_Artist); 
            RaisePropertyChanged("Artist");
            }
        }

        // Copyright: copyright information
        private string _Copyright;
        public  string Copyright {
            get { return _Copyright; }
            set { 
                _Copyright = value;
                SetPropertyStringData(EDSDK.PropID_Copyright, value, CameraModel.maxLength_Copyright);
                RaisePropertyChanged("Copyright");
            }
        }

        // BatteryLevel:  battery charging conditon: 0 - 100%, 0xffffffff=AC power
        private long _BatteryLevel;
        public  long BatteryLevel {
            get { 
                _BatteryLevel = GetPropertyIntegerData(EDSDK.PropID_BatteryLevel); 
                return _BatteryLevel; 
            }
        }

        // Battery Icon:  shows the battery fill status
        private string _BatteryIcon;
        public string BatteryIcon
        {
            get { return _BatteryIcon; }
            set {
                _BatteryIcon = value;
                RaisePropertyChanged("BatteryIcon");
            }
        }

        // CameraDate/CameraTime: date and time of the camera
        private EDSDK.EdsTime _Time;
        private string _CameraDate;
        private string _CameraTime;
        public  string CameraDate { 
            get {
                _Time = GetPropertyStruct<EDSDK.EdsTime>(EDSDK.PropID_DateTime, EDSDK.EdsDataType.Time);
                _CameraDate = _Time.Year + "-" + _Time.Month + "-" + _Time.Day;
                return _CameraDate;
            }             
        }
        public  string CameraTime { 
            get {
                _Time = GetPropertyStruct<EDSDK.EdsTime>(EDSDK.PropID_DateTime, EDSDK.EdsDataType.Time);
                _CameraTime = _Time.Hour + "-" + _Time.Minute + "-" + _Time.Second;
                return _CameraTime;
            } 
        }

        // Device Info 
        public EDSDK.EdsDeviceInfo _deviceInfo;
        public string   DeviceName { 
            get { return _deviceInfo.szDeviceDescription; } 
        }
        public string   PortName   { 
            get { return _deviceInfo.szPortName; } 
        } 
        public bool     IsLegacy
        { // earlier than the 30D , the camera UI must be locked before commands are reissued
            get { return _deviceInfo.DeviceSubType == 0; } 
        }

        // cameraModel type
        // DSLM cameras are named:   "EOS R...
        // DSLR cameras are named:   "EOS x ...  (where x is a digit)
        // EOS M  cameras are named: "EOS M ...
        private string _CameraType;
        public string CameraType
        {
            get {
                string type = DeviceName.Substring(10, 1);
                switch (type) {
                    case "R": return "DSLM";
                    case "M": return "EOSM";
                    default:  return "DSLR";
                }
            }
        }  

        // FirmwareVersion: camera firmware version
        private string _FirmwareVersion;
        public  string FirmwareVersion { 
            get { return _FirmwareVersion; }
        }

        // OwnerName: owner name
        private string _OwnerName;
        public string OwnerName
        {
            get { return _OwnerName; }
            set
            {
                _OwnerName = value;
                SetPropertyStringData(EDSDK.PropID_OwnerName, value, CameraModel.maxLength_OwnerName);
                RaisePropertyChanged("OwnerName");
            }
        }

        // ProductName: product name  
        private string _ProductName;
        public  string ProductName { 
            get { return _ProductName; } 
        }

        // SerialNumber: serial number
        private string _SerialNumber;
        public  string SerialNumber { 
            get { return _SerialNumber; } 
        }
        #endregion

        // Flags:  Session open/closed - shutter button state - LvStartStop - InitLiveView
        #region 
        // SessionIsOpen: indicates that a camera session is open
        private bool _IsSessionOpen;
        public  bool IsSessionOpen {
            get { return _IsSessionOpen;  }
            set { _IsSessionOpen = value; }
        }

        // Shutter Button State: Off / Halfway / Complete
        private ShutterButtonState _ShutterButtonState;
        public ShutterButtonState ShutterButtonState {
            get { return _ShutterButtonState; }
            set { _ShutterButtonState = value; } 
        }

        private string _LvStartStop;
        public string LvStartStop
        {
            get { return _LvStartStop; }
            set {
                _LvStartStop = value;
                RaisePropertyChanged("LvStartStop");
            }
        }

        // Live View Icon
        private string _LiveViewIcon;
        public string LiveViewIcon
        {
            get { return _LiveViewIcon; }
            set {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!(MainWindow._inShutdown)) {
                    _LiveViewIcon = value;
                    RaisePropertyChanged("LiveViewIcon");
                }
            }
        }
        #endregion

        // Available shots - White balance
        #region 
        // AvailableShots: available shots
        private long _AvailableShots;
        public long AvailableShots
        {
            get {
                _AvailableShots = GetPropertyIntegerData(EDSDK.PropID_AvailableShots);
                return _AvailableShots;
            }
        }

        // WhiteBalance  
        private WhiteBalance _WhiteBalance;
        public WhiteBalance WhiteBalance
        {
            get {
                _WhiteBalance = (WhiteBalance)GetPropertyIntegerData(EDSDK.PropID_WhiteBalance);
                return _WhiteBalance;
            }
            set {
                _WhiteBalance = value;
                SetPropertyIntegerData(EDSDK.PropID_WhiteBalance, (long)value);
            }
        }
        #endregion

        // AE (AutoExposure) mode
        #region 
        // current AEMode: readonly, is set by the cameras AE mode dial
        private long _AEMode;
        public long AEMode
        {
            get {
                // Changes of the AEMode can only initiated by the cameras mode dial:
                _AEMode = GetPropertyIntegerData(EDSDK.PropID_AEMode);
                return _AEMode;
            }
        }

        // Current AEMode value as string
        private string _AEModeStr;
        public string AEModeStr
        {
            get { return _AEModeStr; }
            set {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!(MainWindow._inShutdown)) {
                    _AEModeStr = value;
                    RaisePropertyChanged("AEModeStr");
                }
                else
                   _AEModeStr = "";
            }
        }

        // Current AEMode value as Icon
        private string _AEModeIcon;
        public string AEModeIcon
        {
            get { return _AEModeIcon; }
            set {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!(MainWindow._inShutdown)) {
                    _AEModeIcon = value;
                    RaisePropertyChanged("AEModeIcon");
                }
            }
        }
        #endregion

        // Bulb mode properties
        #region 
        private bool _ShutterIsClosed;
        public bool ShutterIsClosed {
            get { return _ShutterIsClosed; }
            set {
                _ShutterIsClosed = value;
            }
        }

        private bool _BulbCanceld;
        public bool BulbCanceld {
            get { return _BulbCanceld; }
            set {
                _BulbCanceld = value;
            }
        }

        private bool _IsBulbTimerSet;
        public  bool IsBulbTimerSet {
            get { return _IsBulbTimerSet; }
            set {
                _IsBulbTimerSet = value;
                // show green timer icon if Timer value > 0 and Timer is not yet started
                if (_IsBulbTimerSet && ShutterIsClosed)
                    BulbTimerIcon = "../Resources/AEModeIcons/BulbTimerIcon_green.png";
                if (_IsBulbTimerSet && !ShutterIsClosed)
                    BulbTimerIcon = "../Resources/AEModeIcons/BulbTimerIcon_red.png";
                if (!_IsBulbTimerSet)
                    BulbTimerIcon = "../Resources/AEModeIcons/BulbTimerIcon_gray.png";
            }
        }

        private long _BTCounter;
        public  long BTCounter {
            get { return _BTCounter; }
            set {
                _BTCounter = value;
            }
        }

        private uint _BTHours;
        public uint BTHours {
            get { return _BTHours; }
            set {
                _BTHours = value;
                IsBulbTimerSet = (BTSeconds + BTMinutes + value > 0);
                BTHoursStr = value.ToString("D2");
            }
        }
        private uint _BTMinutes;
        public uint BTMinutes {
            get { return _BTMinutes; }
            set {
                _BTMinutes = value;
                IsBulbTimerSet = (BTSeconds + value + BTHours > 0);
                BTMinutesStr = value.ToString("D2");
            }
        }
        private uint _BTSeconds;
        public uint BTSeconds {
            get { return _BTSeconds; }
            set {
                _BTSeconds = value;
                IsBulbTimerSet = (value + BTMinutes + BTHours > 0);
                BTSecondsStr = value.ToString("D2");
            }
        }

        private string _BTHoursStr;
        public string BTHoursStr {
            get { return _BTHoursStr; }
            set {
                _BTHoursStr = value;
                RaisePropertyChanged("BTHoursStr");
            }
        }
        private string _BTMinutesStr;

        public string BTMinutesStr {
            get { return _BTMinutesStr; }
            set {
                _BTMinutesStr = value;
                RaisePropertyChanged("BTMinutesStr");
            }
        }
        private string _BTSecondsStr;
        public string BTSecondsStr {
            get { return _BTSecondsStr; }
            set {
                _BTSecondsStr = value;
                RaisePropertyChanged("BTSecondsStr");
            }
        }

        // Bulb timer Icon
        private string _BulbTimerIcon;
        public  string BulbTimerIcon {
            get { return _BulbTimerIcon; }
            set {
                _BulbTimerIcon = value;
                RaisePropertyChanged("BulbTimerIcon");
            }
        }
        #endregion

        // Aperture parameters
        #region 
        // AvValue: current aperture value
        private long _AvValue;
        public long AvValue
        {
            get {
                _AvValue = GetPropertyIntegerData(EDSDK.PropID_Av);
                return _AvValue;
            }
            set {
                _AvValue = value;
                SetPropertyIntegerData(EDSDK.PropID_Av, value);
            }
        }

        // Current AvValue as string
        private string _ApertureStr;
        public string ApertureStr
        {
            get { return _ApertureStr; }
            set {
                _ApertureStr = value;
                RaisePropertyChanged("ApertureStr");
            }
        }

        // AvValueList: List of available Aperture Values
        private EDSDK.EdsPropertyDesc _AvailableAvValues;
        public int[] AvValueList
        {
            get {
               _AvailableAvValues = GetPropertyDescriptor(EDSDK.PropID_Av);   
               return [.. _AvailableAvValues.PropDesc];
            }
        }

        // AvValueListLength: number of aperture values
        private int _AvValueListLength;
        public int AvValueListLength
        {
            get {
                _AvValueListLength = GetPropertyDescriptor(EDSDK.PropID_Av).NumElements;
                return _AvValueListLength;
            }
        }
        #endregion

        // Autofocus mode parameters
        #region 
        private long _AFMValue;
        public  long AFMValue {
            get {
                _AFMValue = GetPropertyIntegerData(EDSDK.PropID_AFMode);
                return _AFMValue; 
            }
            set {
                _AFMValue = value;
                SetPropertyIntegerData(EDSDK.PropID_AFMode, value);
            }
        }
        
        private string _AFModeIcon;
        public string AFModeIcon
        {
            get { return _AFModeIcon; }
            set
            {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!MainWindow._inShutdown) {
                    _AFModeIcon = value;
                    RaisePropertyChanged("AFModeIcon");
                }
            }
        }

        // Current Autofocus Mode as string
        private string _AFModeStr;
        public  string AFModeStr {
            get { return _AFModeStr; }
            set {
                _AFModeStr = value;
                RaisePropertyChanged("AFModeStr");
            }
        }

        // AFMValueList: List of available AF Mode Values
        private EDSDK.EdsPropertyDesc _AvailableAFMValues;
        public int[] AFMValueList
        {
            get {
                _AvailableAFMValues = GetPropertyDescriptor(EDSDK.PropID_AFMode);
                return [.. _AvailableAFMValues.PropDesc];
            }
        }

        // AFMValueListLength: number of AFMValueList items
        private int _AFMValueListLength;
        public  int AFMValueListLength {
            get {
                _AFMValueListLength = GetPropertyDescriptor(EDSDK.PropID_AFMode).NumElements;
                return _AFMValueListLength; 
            }
        }
        #endregion

        // Live View Autofocus mode properties
        #region 
        // Current Autofocus mode
        private long _LvAFValue;
        public long LvAFValue
        {
            get {
                _LvAFValue = GetPropertyIntegerData(EDSDK.PropID_Evf_AFMode);
                return _LvAFValue;
            }
            set {
                _LvAFValue = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_AFMode, value);
            }
        }

        // Current LiveView Autofocus Mode as string
        private string _LvAFStr;
        public string LvAFStr
        {
            get { return _LvAFStr; }
            set {
                _LvAFStr = value;
                RaisePropertyChanged("LvAFStr");
            }
        }

        // EvfAFMValueList: List of available AF Mode Values
        private EDSDK.EdsPropertyDesc _LvAFValueList;
        public int[] LvAFValueList
        {
            get {
                _LvAFValueList = GetPropertyDescriptor(EDSDK.PropID_Evf_AFMode);
                return [.. _LvAFValueList.PropDesc];
            }
        }

        // AFMValueListLength: number of autofocus items
        private int _LvAFListLength;
        public int LvAFListLength
        {
            get {
                _LvAFListLength = GetPropertyDescriptor(EDSDK.PropID_Evf_AFMode).NumElements;
                return _LvAFListLength;
            }
        }
        #endregion

        // Drive mode parameters
        #region 
        private long _DrMValue;
        public long DrMValue
        {
            get {
                _DrMValue = GetPropertyIntegerData(EDSDK.PropID_DriveMode);
                return _DrMValue;
            }
            set {
                _DrMValue = value;
                SetPropertyIntegerData(EDSDK.PropID_DriveMode, value);
            }
        }

        // Current Drive Mode value as Icon
        private string _DrModeIcon;
        public string DrModeIcon
        {
            get { return _DrModeIcon; }
            set {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!(MainWindow._inShutdown)) {
                    _DrModeIcon = value;
                    RaisePropertyChanged("DrModeIcon");
                }
            }
        }

        // DrMValueList: List of available drive mode values
        private EDSDK.EdsPropertyDesc _AvailableDrMValues;
        public int[] DrMValueList
        {
            get {
                _AvailableDrMValues = GetPropertyDescriptor(EDSDK.PropID_DriveMode);
                return _AvailableDrMValues.PropDesc.ToArray();
            }
        }

        // DrMValueListLength: number of DrMValueList items
        private int _DrMValueListLength;
        public int DrMValueListLength
        {
            get {
                _DrMValueListLength = GetPropertyDescriptor(EDSDK.PropID_DriveMode).NumElements;
                return _DrMValueListLength;
            }
        }
        #endregion

        // Exposure Value Compensation - EVC - parameters
        #region 
        // EVCValue: current exposure compensation value
        private long _EVCValue;
        public long EVCValue
        {
            get { _EVCValue = GetPropertyIntegerData(EDSDK.PropID_ExposureCompensation);
                return _EVCValue;
            }
            set {
                _EVCValue = value;
                SetPropertyIntegerData(EDSDK.PropID_ExposureCompensation, value);
            }
        }

        // EVCValueList: list of available Evc Values
        private EDSDK.EdsPropertyDesc _EVCValueList;
        public int[] EVCValueList
        {
            get {
                _EVCValueList = GetPropertyDescriptor(EDSDK.PropID_ExposureCompensation);
                return _EVCValueList.PropDesc. ToArray();
            }
        }

        // EVCValueListLength: number of EVCValueList items
        private int _EVCValueListLength;
        public  int EVCValueListLength {
            get {
                _EVCValueListLength = GetPropertyDescriptor(EDSDK.PropID_ExposureCompensation).NumElements;
                return _EVCValueListLength; 
            }
        }
        #endregion

        // Iso speed parameters
        #region
        // ISOSpeed: get/set Iso Speed
        private long _ISOSpeed;
        public  long ISOSpeed {
            get {
            _ISOSpeed = GetPropertyIntegerData(EDSDK.PropID_ISOSpeed);
            return _ISOSpeed;
            }
            set {
            _ISOSpeed = value;
            SetPropertyIntegerData(EDSDK.PropID_ISOSpeed, value);
            }
        }

        // ISOSpeedStr: current Iso value as string
        private string _ISOSpeedStr;
        public  string ISOSpeedStr {
            get { return _ISOSpeedStr; }
            set {
                _ISOSpeedStr = value;
                RaisePropertyChanged("ISOSpeedStr");
            }
        }

        // ISOSpeedList: Array of available ISO Speed Values
        private EDSDK.EdsPropertyDesc _AvailableISOSpeeds;
        public int[] ISOSpeedList
        {
            get {
                _AvailableISOSpeeds = GetPropertyDescriptor(EDSDK.PropID_ISOSpeed);
                return _AvailableISOSpeeds.PropDesc.ToArray();
            }
        }

        // ISOSpeedListLength: number of available ISO speed values
        private int _ISOSpeedListLength;
        public  int ISOSpeedListLength {
            get {
                _ISOSpeedListLength = GetPropertyDescriptor(EDSDK.PropID_ISOSpeed).NumElements;
                return _ISOSpeedListLength; 
            }
        }
        #endregion

        // Metering mode parameters
        #region 
        // MMValue: current metering mode
        private long _MMValue;
        public  long MMValue {
            get {
                _MMValue = GetPropertyIntegerData(EDSDK.PropID_MeteringMode);
                return _MMValue; 
            }
            set {
                _MMValue = value;
                SetPropertyIntegerData(EDSDK.PropID_MeteringMode, value);
            }
        }

        // Current Metering Mode value as Icon
        private string _MModeIcon;
        public string MModeIcon {
            get { return _MModeIcon; }
            set {
                // Flag "_inShutDown" prevents "invalid PropID_AEMode events during Shutdown
                if (!(MainWindow._inShutdown)) {
                    _MModeIcon = value;
                    RaisePropertyChanged("MModeIcon");
                }
            }
        }

        // MMValueList: List of available metering mode values
        private EDSDK.EdsPropertyDesc _AvailableMMValues;
        public int[] MMValueList
        {
            get {
                _AvailableMMValues = GetPropertyDescriptor(EDSDK.PropID_MeteringMode);
                return [.. _AvailableMMValues.PropDesc];
            }
        }

        // MMValueListLength: number of MMValueList items
        private int _MMValueListLength;
        public  int MMValueListLength {
            get {
                _MMValueListLength = GetPropertyDescriptor(EDSDK.PropID_MeteringMode).NumElements;
                return _MMValueListLength; 
            }
        }
        #endregion

        // AutoExposure (Shutter) speed
        #region AE Speed
        // AESpeed: = current Shutter Speed
        private long _AESpeed;
        public  long AESpeed {
            get { 
            _AESpeed = GetPropertyIntegerData(EDSDK.PropID_Tv);         
            return _AESpeed; 
            }
            set {
                _AESpeed = value;
                SetPropertyIntegerData(EDSDK.PropID_Tv, value);
            }
        }

        // AESpeedStr: current shutter speed value as string
        private string _AESpeedStr;
        public  string AESpeedStr {
            get { return _AESpeedStr; }
            set {
                _AESpeedStr = value;
                RaisePropertyChanged("AESpeedStr");
            }
        }

        // AESpeedList: list of available shutter speed values
        private EDSDK.EdsPropertyDesc _AvailableAESpeeds;
        public int[] AESpeedList
        {
            get {
                _AvailableAESpeeds = GetPropertyDescriptor(EDSDK.PropID_Tv);
                return _AvailableAESpeeds.PropDesc.ToArray();
            }
        }

        // AESpeedListLength: number of AESpeedList items
        private int _AESpeedListLength;
        public  int AESpeedListLength {
            get {
                _AESpeedListLength = GetPropertyDescriptor(EDSDK.PropID_Tv).NumElements;
                return _AESpeedListLength; 
            }
        }
        #endregion

        // Live View mode parameters
        #region 
        // LiveViewDevice: get/set property Evf_OutputDevice from/on the camera
        // Setting LiveViewDevice to true/false starts/stops the camera Live View mode
        private LiveViewDevice _LiveViewDevice;
        public  LiveViewDevice LiveViewDevice {
            get {
                _LiveViewDevice = (LiveViewDevice)GetPropertyIntegerData(EDSDK.PropID_Evf_OutputDevice);
                return _LiveViewDevice; 
            }
            set {
                _LiveViewDevice = value;  
                // This starts/stops the camera Live View mode
                SetPropertyIntegerData(EDSDK.PropID_Evf_OutputDevice, (long)value);
            }
        }

        // IsInHostLiveViewMode
        private bool _IsInHostLiveViewMode;
        public bool IsInHostLiveViewMode {
            get {
                _IsInHostLiveViewMode = (IsInLiveViewMode && LiveViewDevice.HasFlag(LiveViewDevice.Host));
                return _IsInHostLiveViewMode;
            }
        }

        // IsInLiveViewMode: get/set property PropID_Evf_Mode: 0=disable | 1=enable
        private bool _IsInLiveViewMode;
        public bool IsInLiveViewMode {
            get {
                _IsInLiveViewMode = (GetSetting(EDSDK.PropID_Evf_Mode) != 0);
                return _IsInLiveViewMode;
            }
            set {
                _IsInLiveViewMode = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_Mode, value ? 1 : 0);
            }
        }

        // "_LockTakingPicture" == true while Live View is interrupted for taking a picture
        // Deactivates the take Picture Button while "true" 
        // Causes restart of Live View in TakePictureFinale" and to switch back to Live View
        private bool _LockTakingPicture;
        public bool LockTakePicture
        {
            get {
                return _LockTakingPicture;
            }
            set {
                _LockTakingPicture = value;
            }
        }

        // "_LockLiveView == true"  blocks the liveView button when "TakePicture" is active.
        private bool _LockLiveView;
        public bool LockLiveView
        {
            get {
                return _LockLiveView;
            }
            set {
                _LockLiveView = value;
            }
        }

        // "_WaitFor_2ndClick"  is set after the first click in Bulb mode without a timer set.
        private bool _WaitFor_2ndClick;
        public bool WaitFor_2ndClick
        {
            get {
                return _WaitFor_2ndClick;
            }
            set {
                _WaitFor_2ndClick = value;
            }
        }

        // Source path for the Live View Image
        private ImageSource _LiveViewImageSource;
        public ImageSource LiveViewImageSource {
            get { return _LiveViewImageSource; }
            set {
                _LiveViewImageSource = value;
                RaisePropertyChanged("LiveViewImageSource");
            }
        }

        // Source path for the Live View Histogram
        private PointCollection _LiveViewHistogram;
        public PointCollection LiveViewHistogram {
            get { return _LiveViewHistogram; }
            set {
                _LiveViewHistogram = value;
                RaisePropertyChanged("LiveViewHistogram");
            }
        }

        // Evf image zoom factor
        private EvfZoomFactor _EvfZoom;
        public EvfZoomFactor EvfZoom {
            get { return _EvfZoom; }
            set {
                SetPropertyIntegerData(EDSDK.PropID_Evf_Zoom, (uint)value);
                _EvfZoom = value; 
                }
        }

        // Evf histogram active flag
        private EnableState _EvfHistogramActive;
        public EnableState EvfHistogramActive {
            get { return _EvfHistogramActive; }
            set {
                _EvfHistogramActive = value;
            }
        }
        #endregion

        // Picture preview parameters
        #region 
        // Source path for the Preview Image
        private ImageSource _PreviewImageSource;
        public ImageSource PreviewImageSource {
            get { return _PreviewImageSource; }
            set {
                _PreviewImageSource = value;
                RaisePropertyChanged("PreviewImageSource");
            }
        }

        // IsPreviewActive: indicates wether the preview image is shown
        private bool _IsPreviewActive;
        public bool IsPreviewActive {
            get { return _IsPreviewActive; }
            set { _IsPreviewActive = value; }
        }
        #endregion

        // Depth of field preview
        #region 
        private bool _DepthOfFieldPreview;
        public  bool DepthOfFieldPreview {
            get {
                _DepthOfFieldPreview = (GetPropertyIntegerData(EDSDK.PropID_Evf_DepthOfFieldPreview) != 0);
                return _DepthOfFieldPreview; 
            }
            set { 
                _DepthOfFieldPreview = value;
                long dof;
                if (value) dof = 1; else dof = 0;
                SetPropertyIntegerData(EDSDK.PropID_Evf_DepthOfFieldPreview, dof); 
            }
        }
        #endregion

        // Zoom rectangle
        #region 
        private double _ZoomRect_Height;
        public double ZoomRect_Height {
            get { return _ZoomRect_Height; }
            set {
                _ZoomRect_Height = value;
                RaisePropertyChanged("ZoomRect_Height");
            }
        }

        private double _ZoomRect_Width;
        public double ZoomRect_Width {
            get { return _ZoomRect_Width; }
            set {
                _ZoomRect_Width = value;
                RaisePropertyChanged("ZoomRect_Width");
            }
        }

        private double _ZoomRect_X;
        public double ZoomRect_X {
            get { return _ZoomRect_X; }
            set {
                _ZoomRect_X = value;
                RaisePropertyChanged("ZoomRect_X");
            }
        }

        private double _ZoomRect_Y;
        public double ZoomRect_Y {
            get { return _ZoomRect_Y; }
            set {
                _ZoomRect_Y = value;
                RaisePropertyChanged("ZoomRect_Y");
            }
        }

        // ZoomPosition:  upper left coordinates of the focus and zoom border
        private EDSDK.EdsPoint _ZoomPosition;
        public EDSDK.EdsPoint ZoomPosition
        {
            get { 
                return _ZoomPosition; }
            set
            {
                uint[] ptr = [(uint)value.x, (uint)value.y];
                SetPropertyIntegerArrayData(EDSDK.PropID_Evf_ZoomPosition, ptr);
                _ZoomPosition.x = (int)ptr[0];
                _ZoomPosition.y = (int)ptr[1];
            }
        }
        private SolidColorBrush _ZoomRect_Color;
        public SolidColorBrush ZoomRect_Color {
            get { return _ZoomRect_Color; }
            set {
                _ZoomRect_Color = value;
                RaisePropertyChanged("ZoomRect_Color");
            }
        }
        #endregion

        // Focus Information and Focus Stacking
        #region 

        // Focus Info Parameters
        #region 
        // FocusInformation: holds the fields drawn from Eds FocusInfo, only accessible in Evf Mode:
        // Rectangle     Bounds          : image rectangle 
        // long          ExecuteMode     : EOS 60D:  Qick/Live/LvFace EOS R6: ???
        // FocusPoint[]  FocusPoints     : List of focus points 
        //
        // Each focus point consists of:
        // Rectangle     Bounds          : X, Y, Height, Width
        // bool          IsInFocus       : focus point is in focus
        // bool          IsValid         : normally FocusPoints[0] ... FocusPoints[FocusPoints.Lenght -1] are valid  
        // bool          IsSelected      : focus point is selected
        #endregion

        private Focus _FocusInformation;
        public Focus FocusInformation {
            get {
                EDSDK.EdsFocusInfo FocusInfo = GetPropertyStruct<EDSDK.EdsFocusInfo>(EDSDK.PropID_FocusInfo, EDSDK.EdsDataType.FocusInfo);
                _FocusInformation = Focus.Create(FocusInfo);
                LocalFocusInformation = _FocusInformation;
                return _FocusInformation;
            }
        }

        public Focus _LocalFocusInformation = new();
        public  Focus LocalFocusInformation { 
            get { return _LocalFocusInformation;   }  
            set { _LocalFocusInformation = value;
            }
        }

        // FocusRectPosition:  upper left coordinates of the focus and zoom border
        private EDSDK.EdsPoint _FocusRectPosition;
        public EDSDK.EdsPoint  FocusRectPosition
        {
            get { return _FocusRectPosition; }
            set
            {
                uint[] ptr = [(uint)value.x, (uint)value.y];
                SetPropertyIntegerArrayData(EDSDK.PropID_Evf_ZoomPosition, ptr);
                _FocusRectPosition.x = (int)ptr[0];
                _FocusRectPosition.y = (int)ptr[1];
            }
        }

        // Focus stack parametes
        // Number of focus steps (>0:  away from the camera / <0: towards the camera
        private string _NrFocusSteps="1";
        public string NrFocusSteps
        {
            get { return _NrFocusSteps; }
            set {
                _NrFocusSteps = value;
                RaisePropertyChanged("NrFocusSteps");
            }
        }

        // Focus step width
        private string _FocusStepWidth="1";
        public string FocusStepWidth
        {
            get { return _FocusStepWidth; }
            set {
                _FocusStepWidth = value;
                RaisePropertyChanged("FocusStepWidth");
            }
        }
        #endregion

        // EVF Mode parameters
        #region 
        // EVF_Mode: get/set property EVF_Mode Disbale=0/Enable=1 
        private EnableState _LiveShooting;
        public EnableState LiveShooting {
            get {
                _LiveShooting = (EnableState)GetPropertyIntegerData(EDSDK.PropID_Evf_Mode);
                return _LiveShooting;
            }
            set {
                _LiveShooting = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_Mode, (uint)value);
            }
        }

        // EVFColorTemperature: get/set property Evf_ColorTemperature on the camera
        private long _EVFColorTemperature;
        public  long EVFColorTemperature {
            get {
                _EVFColorTemperature = GetPropertyIntegerData(EDSDK.PropID_Evf_ColorTemperature);
                return _EVFColorTemperature; 
            }
            set { 
                _EVFColorTemperature = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_ColorTemperature, value); 
            }
        }

        // Current AEMode value as Icon
        private string _ZoomIcon;
        public string ZoomIcon {
            get { return _ZoomIcon; }
            set {
                _ZoomIcon = value;
                RaisePropertyChanged("ZoomIcon");
            }
        }

        // EvfAFButton icon image source
        private string _EVF_AFButtonIcon;
        public string EVF_AFButtonIcon {
            get {
            return _EVF_AFButtonIcon;
            }
            set {
                _EVF_AFButtonIcon = value;
                RaisePropertyChanged("EVF_AFButtonIcon");
            }
        }

        // HistogramButton icon image source
        private string _HistogramIcon;
        public string HistogramIcon {
            get {
                return _HistogramIcon;
            }
            set {
                _HistogramIcon = value;
                RaisePropertyChanged("HistogramIcon");
            }
        }

        // HistogramButton icon image source
        private string _CrosslinesIcon;
        public string CrosslinesIcon
        {
            get {
                return _CrosslinesIcon;
            }
            set {
                _CrosslinesIcon = value;
                RaisePropertyChanged("CrosslinesIcon");
            }
        }

        // EVF AutoFocus Mode is On/Off
        private bool _EVF_AutoFocusIsOn = false;
        public bool EVF_AutoFocusIsOn {
            get { return _EVF_AutoFocusIsOn;  }
            set { _EVF_AutoFocusIsOn = value; }
        }

        // EVFAutoFocusMode: Quickmode/Live View Mode/LiveViewFace Mode
        private LiveViewAutoFocus _EVF_AutoFocusMode;
        public LiveViewAutoFocus EVF_AutoFocusMode {
            get {
                _EVF_AutoFocusMode = (LiveViewAutoFocus)GetPropertyIntegerData(EDSDK.PropID_Evf_AFMode);
                return _EVF_AutoFocusMode;
            }
            set {
                _EVF_AutoFocusMode = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_AFMode, (long)value);
            }
        }

        // EVF_AutoFocusModeStr: Text representing the Live View Autofocus Mode, shown on the EvfAFModeButton
        private string _EVF_AutoFocusModeStr = "";
        public string EVF_AutoFocusModeStr {
            get { return _EVF_AutoFocusModeStr;  }
            set { 
                _EVF_AutoFocusModeStr = value;
                RaisePropertyChanged("EVF_AutoFocusModeStr");
            }
        }

        // EVF_WhiteBalance: get/set Live View Whitebalance from/on the camera
        private WhiteBalance _EVF_WhiteBalance;
        public  WhiteBalance EVF_WhiteBalance {
            get {
                _EVF_WhiteBalance = (WhiteBalance)GetPropertyIntegerData(EDSDK.PropID_Evf_WhiteBalance);
                return _EVF_WhiteBalance; 
            }
            set { 
                _EVF_WhiteBalance = value;
                SetPropertyIntegerData(EDSDK.PropID_Evf_WhiteBalance, (long)value); 
            }
        }
        #endregion

       // Picture path functions 
        #region
        // Directory path where downloaded images are stored
        private string _picturePath;
        public string picturePath {
            get { return _picturePath; }
            set { _picturePath = value; }
        }

        // SavePicturesToCamera: store pictures only on the camera
        public void SavePicturesToCamera()
        {
            picturePath = string.Empty;
            ModifyPictureSaveLocationOnCamera(SaveLocation.Camera);
        }

        // SavePicturesToHost: store pictures only on the host PC
        public void SavePicturesToHost(string pathFolder) {
            ModifyPicturePathOnHost(pathFolder);
            ModifyPictureSaveLocationOnCamera(SaveLocation.Host);
        }

        // SavePicturesToHostAndCamera: store pictures on host and camera
        public void SavePicturesToHostAndCamera(string pathFolder) {
            ModifyPicturePathOnHost(pathFolder);
            ModifyPictureSaveLocationOnCamera(SaveLocation.Camera | SaveLocation.Host);
        }

        // ModifyPicturePathOnHost: update picturePath, if necessary create a new directory
        private void ModifyPicturePathOnHost(string pathFolder) 
        {
            try {
                if ((pathFolder==null) || (pathFolder=="")) {
                    throw new EosException(0, "Picture path folder may not be null or white space");
            }
            else {
                CheckDisposed();
                picturePath = pathFolder;
                if (!Directory.Exists(picturePath))  Directory.CreateDirectory(picturePath);
            }
            }
            catch (EosException ex) {
            System.Windows.MessageBox.Show(ex.Message , "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }

         // ModifyPictureSaveLocationOnCamera: Set picture save location on camera
         public void ModifyPictureSaveLocationOnCamera(SaveLocation saveLocation) 
         {
            uint error;
            CheckDisposed();
            OpenSession(); 
            // Set picture save Location
            SetPropertyIntegerData(EDSDK.PropID_SaveTo, (long)saveLocation);
            // get "capacity"
            try {
                if (!IsLegacy) {
                    LockAndExecute(() => {
                        var capacity = new EDSDK.EdsCapacity { NumberOfFreeClusters = 0x7FFFFFFF, BytesPerSector = 0x1000, Reset = 1 };
                        error = EDSDK.EdsSetCapacity(MainWindow.cameraPtr, capacity);
                        if (error != (uint)ErrorCode.Ok)
                            throw new EosException(error, "Failed to set capacity");
                    });
                }
            }
            catch (EosException ex) {
            MessageBox.Show(ex.EosErrorMessage + " : " + ex.EosErrorCode, "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
         #endregion

        // Start Live View
        #region 
        // StopEVF_Autofocus:  command "Stop Evf Autofocus Mode" , called by the Timer or by a second Evf_AutofocusButton_Click
        public void StopEVF_Autofocus() 
        {
            if (EVF_AutoFocusIsOn) {
                AutoResetEvent syncEvent = new(false);
                cmdDoEvfAF cmd = new(0, syncEvent);
                MainWindow.cmdProcessor?.enqueueCmd(cmd);
                syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
                EVF_AutoFocusIsOn = false;
            }
        }

        // InitLiveView
        public uint InitLiveView()
        {
            uint error = EDSDK.EDS_ERR_OK;
           // IntPtr EfvImage = new();
            Bitmap bmp;

            // Download Evf image in a loop
            while (_IsInLiveViewMode) {
                // CreateBitMask a memory stream 
                error = EDSDK.EdsCreateMemoryStream(0, out IntPtr stream);

                // CreateBitMask a EvfImageRef
                if (error == EDSDK.EDS_ERR_OK)
                    error = EDSDK.EdsCreateEvfImageRef(stream, out MainWindow._efvImage);

                // Download the stream to Evf Image
                if (error == EDSDK.EDS_ERR_OK) {
                    do {
                        error = EDSDK.EdsDownloadEvfImage(MainWindow.cameraPtr, MainWindow._efvImage);
                    } while (error == EDSDK.EDS_ERR_OBJECT_NOTREADY);
                }

                // Get a Evf bitmap image from EvfImagePtr
                bmp = GetEvfImage(stream);

                // If Efv Histogramm is active, compute histgram values
                if (EvfHistogramActive == EnableState.enabled) {

                    // reset MainWindow._Histogram
                    for (uint i = 0; i < 255; i++) MainWindow._histogram[i] = 0;

                    // compute histogram vector
                    uint index;
                    for (int y = 0; y < bmp.Height; y += 8) {
                        for (int x = 0; x < bmp.Width; x += 8) {
                            double p = bmp.GetPixel(x, y).GetBrightness();
                            index = (uint)(255 * p);
                            MainWindow._histogram[index]++;
                        }
                    }
                }
                // Display the Evf image
                LiveViewImageSource = Converter.CreateBitmapSourceFromBitmap(bmp);

                error = EDSDK.EdsRelease(stream);
                MainWindow._efvImage = IntPtr.Zero;
            }
            return error;
        }

        // GetEvfImage:  Get Bitmap Image from EvfStream
        public unsafe static System.Drawing.Bitmap GetEvfImage(IntPtr EvfStream)
        {
            IntPtr jpgPointer;
            uint error;
            ulong length = 0;
            Bitmap Image = new (1, 1); 

            error = EDSDK.EdsGetPointer(EvfStream, out jpgPointer);
            if (error == EDSDK.EDS_ERR_OK)
                error = EDSDK.EdsGetLength(EvfStream, out length);

            if (error == EDSDK.EDS_ERR_OK) {
                if (length != 0) {
                    UnmanagedMemoryStream ums = new((byte*)jpgPointer.ToPointer(), (uint)length, (uint)length, FileAccess.Read);
                    Image = new Bitmap(ums, true);
                }
            }
            return Image;
        }
        #endregion

        // EDSDK.CameraState_UIUnLock handling
        #region 
        public bool IsLocked { get; private set; }

        // Lock: set camera state to UILock
        // Operations of the camera unit are disabled and only operations from the host PC are accepted
        public void Lock() 
        {
            CheckDisposed();
                if (!IsLocked) {
                uint error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UILock,0);
                if (error != (uint)ErrorCode.Ok)
                        throw new EosException(error, "Failed to lock camera.");
                IsLocked = true;
            }
            }

        // Unlock: set camera state to "UI Unock"
        // cameraModel operations are allowed but may conflict with data transfers
        public void Unlock() 
        {
            if (IsLocked) {
                MainWindow.Error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UIUnLock,1);
                IsLocked = false;
            }
        }

        // LockAndExecute: perform an action in UILock state
        public void LockAndExecute(Action action) 
        {
            Lock();
            try { action(); }
            finally { Unlock(); }
        }
        #endregion

        // Get / Set EDSDK properties
        #region
        // allowed only for "AEModeSelect", "ISO", "Av", "Tv", "MeteringMode" and "ExposureCompensation"
        public List<int> GetSettingsList(uint PropID)
        {
            EDSDK.EdsPropertyDesc des = new EDSDK.EdsPropertyDesc();

            if (MainWindow.cameraPtr != IntPtr.Zero) {
                //a list of settings can only be retrieved for following properties
                if (PropID == EDSDK.PropID_AEModeSelect || PropID == EDSDK.PropID_ISOSpeed || PropID == EDSDK.PropID_Av
                    || PropID == EDSDK.PropID_Tv || PropID == EDSDK.PropID_MeteringMode || PropID == EDSDK.PropID_ExposureCompensation) {
                    //get the list of possible values
                    MainWindow.Error = EDSDK.EdsGetPropertyDesc(MainWindow.cameraPtr, PropID, out des);

                    // If no elements are returned, return a not empty list
                    // This occurs with AV und TV list in Modes AV and TV, but never in manual mode ?????
                    // this must be fitted to the camera model
                    return des.PropDesc.Take(des.NumElements).ToList();
                }
                else throw new ArgumentException("Method cannot be used with this Property ID");
            }
            else { throw new ArgumentNullException("cameraModel or camera reference is null/zero"); }
        }
                    
        //  Get the current setting of given property ID as an uint
        public uint GetSetting(uint PropID)
        {
            if (MainWindow.cameraPtr != IntPtr.Zero) {
                MainWindow.Error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, PropID, 0, out uint property);
                return property;
            }
            else { throw new ArgumentNullException("cameraModel or camera reference is null/zero"); }
        }

        // Sets an uint value for the given property ID
        public void SetSetting(uint PropID, uint Value)
        {
            if (MainWindow.cameraPtr != IntPtr.Zero) {
                SendSDKCommand(delegate {
                    Thread.Sleep(standardDelay);    
                    int propsize;
                    EDSDK.EdsDataType proptype;
                    //get size of property
                    MainWindow.Error = EDSDK.EdsGetPropertySize(MainWindow.cameraPtr, PropID, 0, out proptype, out propsize);
                    //set given property
                    MainWindow.Error = EDSDK.EdsSetPropertyData(MainWindow.cameraPtr, PropID, 0, propsize, Value);
                });
            }
            else { 
                throw new ArgumentNullException("cameraModel or camera reference is null/zero"); 
            }
        }
        #endregion

        // SendSDKCommand, SafeCall (action)
        #region 
        // SendSDKCommand: Send a command to the camera via STAThread>
        public static void SendSDKCommand(Action command, bool longTask = false)
        {
            if (longTask) STAThread.Create(command).Start();
            else STAThread.ExecuteSafely(command);
        }

		// SafeCall
        public delegate void SafeCallDelegate();
        public void SafeCall(Action action, Action<Exception> exceptionHandler) { action(); }
        #endregion

        // Override methods of base classes/interfaces
        #region 
        // DisposeUnmanaged:
        // Override base.DisposeUnmanaged: before disposing unmanaged resources close session
        protected override void DisposeUnmanaged()
        {
            if (IsSessionOpen) MainWindow.Error = EDSDK.EdsCloseSession(MainWindow.cameraPtr);
            base.DisposeUnmanaged();
        }

        // ToString:
        // Override standard ToString(): if Devicename==null, assign an empty string
        public override string ToString() 
        {
            string x = DeviceName ?? string.Empty;
            return x;
        }
        #endregion
    }
}  


