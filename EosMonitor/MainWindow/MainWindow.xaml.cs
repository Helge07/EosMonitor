using EDSDKLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace EosMonitor
{
    public partial class MainWindow : Window
    {
        // global Variables
        #region 
        public static IntPtr            cameraPtr = IntPtr.Zero;            // Pointer to the selected camera device
        int                             cameraCount = 0;                    // number of detected cameras
        public static                   CameraModel? cameraModel = null;    // camera controller
        int                             cameraIndex = -1;                   // index of the selected camera
        public IntPtr                   cameraList = IntPtr.Zero;           // list of attached cameras
        public static string            cameraName = "";                    // name of the selected camera        

        public static                   CmdProcessor? cmdProcessor;         // command processor
        ThreadPoolWorker                threadPoolWorker = new();           // Interface to the thread pool
        ManagementEventWatcher          watcher = new();                    // detects if a _camera is removed or inserted
        uint                            watcherCounter = 0;                 // prevents multiple "ManagementEventWatcher" events

        public static uint              Error                               // Report EDSDK errors via 'ReportError()'
        {
            get { return EDSDK.EDS_ERR_OK; }
            set { if (value != EDSDK.EDS_ERR_OK) ReportError("SDK Error:  " + errorList.getErrorItem(value).CodeString); }
        }
        static object                   ErrLock = new();                    // synchronizes access to 'ReportError()' 
        public static int               ErrCount = 0;                       // counts the # of errorrs
        public static                   ErrorList errorList = new();                     // List of error items: [<CodeNumber> <CodeString> <Description>]

        public static bool              _cameraIsInitialized = false;       // true if a CameraModel object exists
        public static int               _delayDriveLens = 3000;             // Default delay for DriveLens commands during stacking
        public static int               _delayTakeStack = 3000;             // Default delay for TakePicture commands during stacking
        public static bool              _evf_AFTimer_Elapsed = true;        // is set to false within 5 seconds after Evf AF Start
        public static IntPtr            _efvImage = new();                  // image displayed on the host
        public static long[]            _histogram = new long[256];         // histogram vector of the image
        string?                         _imageSaveDirectory { get; set; }                // Directory where photos are saved on the Host
        public static bool              _inShutdown = false;
        public static double            _logoOpacity = 1;                   // Opacity of the Logo image: is stepwise decreased during the start phase
        public static bool              _lockPropChanged = false;           // suppress handling of Property Changed Events during Initialization
        public static LvState           _nextLvState = LvState.LiveViewON;  // next state when clicking the LiveViwe Start/Stop button
        public static double            _oldWindowWidth;                    // remebers old Width when resizing the MainWindow
        private UIElement               _originalElement = null!;
        private FocusRectangleAdorner   _overlayElement = null!;
        public static bool              _picturePathOK = false;             // Indicates, that the PictureSaveDestination was initialized
        public static bool              _progressbarActive = false;         // indicates active progress bar
        public static LvState           _savedLvState = LvState.LiveViewOFF;// remember InitLiveView status before "TakePicture"
        public static bool              _savedPictureState = false;         // remember _storePicture status before "TakePicture"
        FolderBrowserDialog             _saveFolderBrowser = new();         // Dialog for folder selection
        public static bool              _saveToCamera = true;               // Indicates, that pictures will be stored on the host
        public static AutoResetEvent    _syncCmdProcessor = new(false);

        public static bool              _sessionIsOpen = false;
        public readonly Lock            _stackingLock = new();              // coordinates the stacking threads
        public readonly Lock            _takePictureLock = new();           // coordinates TakePicture clicks
        public static bool              _zoomPositionValid = false;         // is set true when dragging is finished, used in UpdateFocusInfo()

        /* Fallback lists: used, if the camera delivers incorrect descriptors( length parameter missing)*/
        // Fallback list of the available ISO values: 
        // 0x00   0x48  0x50  0x58  0x60  0x68  0x70  0x78
        // Auto    100   200   400   800  1600  3200  6400
        public static List<int> ISOList =
        [0x00, 0x48, 0x50, 0x58, 0x60, 0x68, 0x70, 0x78];

        // Fallback list  of the available Tv values
        // 30"     25"     20"     15"     13"     10"     
        // 8"      6"      5"      4"      3"2     2".5  
        // 2"      1"6     1"3     1"      0"8      0"6     
        // 0"5     0"4     0"3     1/4     1/5     1/6     
        // 1/8     1/10    1/13    1/15    1/20    1/25
        // 1/30    1/40    1/50    1/60    1/80    1/100  
        // 1/125   1/160   1/200   1/250   1/320   1/400
        // 1/500   1/640   1/800   1/1000  1/1250  1/1600 
        // 1/2000  1/2500  1/3200  1/4000  1/5000  1/6400  1/8000
        public static List<int> TvList =
        [   0x10,  0x13,  0x14,  0x18,  0x1B,  0x1C,
            0x20,  0x24,  0x25,  0x28,  0x2B,  0x2D,
            0x30,  0x33,  0x35,  0x38,  0x3B,  0x3D,
            0x40,  0x43,  0x44,  0x48,  0x4B,  0x4C,
            0x50,  0x54,  0x55,  0x58,  0x5C,  0x5D,
            0x60,  0x63,  0x65,  0x68,  0x6B,  0x6D,
            0x70,  0x73,  0x75,  0x78,  0x7B,  0x7D,
            0x80,  0x83,  0x85,  0x88,  0x8B,  0x8D,
            0x90,  0x93,  0x95,  0x98,  0x9B,  0x9D, 0xA0
        ];

        // Fallback list of the available Av values:
        // 2.8   3.2   3.5   4   4.5   5.0   5.6   6.3   7.1   8   9  10   11   13   14   16   18   20 22   25   29  32
        // EOS 60D:  4.0 ... 22
        // R6:       2.8 ... 32
        public static List<int> AvList =
        // List of the available Av values
        [   0x28,  0x2B,  0x2D,  0x30,  0x33,  0x35,  0x38,  0x3B,  0x3D,
            0x40,  0x43,  0x45,  0x48,  0x4B,  0x4D,  0x50,  0x53,  0x55, 0x58
        ];

        #endregion



        public MainWindow()
        {
            InitializeComponent();

            // Initialize the SDK
            Error = EDSDK.EdsInitializeSDK();

            // Get the cameraList from the SDK
            if (Error == EDSDK.EDS_ERR_OK) {
                Error = EDSDK.EdsGetCameraList(out cameraList);
            }
            // Get the number of attached cameras from the SDK
            if (Error == EDSDK.EDS_ERR_OK) {
                Error = EDSDK.EdsGetChildCount(cameraList, out cameraCount);
            }
            // Fill the CameraComboBox with names of attached cameras
            CameraComboBox.Items.Clear();
            int i;
            for (i = 0; i < cameraCount; i++) {
                uint error = EDSDK.EdsGetChildAtIndex(cameraList, i, out nint _camera);
                error = EDSDK.EdsGetDeviceInfo(_camera, out EDSDK.EdsDeviceInfo _deviceInfo);
                string _cameraname = _deviceInfo.szDeviceDescription.ToString();
                CameraComboBox.Items.Add(_cameraname);
                if (i == 0) {
                    CameraLabel.Content = "Select Camera!";
                    CameraLabel.Foreground = System.Windows.Media.Brushes.MediumVioletRed;
                }
            }

            // Read the EosMonitor config file
            ReadConfigFile();

            // Setup the ManagementEventWatcher: if removed/inserted cameras are detected the application is resetted
            // EventType 2:  device Inserted
            // EventType 3:  device removed
            WqlEventQuery query = new("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 or EventType = 3");
            watcher.EventArrived += new EventArrivedEventHandler(OnWatcher_EventArrived);
            watcher.Query = query;
            watcher.Start();
        }

        // Read the EosMonitor config file
        private void ReadConfigFile()
        {
            try {
                string? line = " ";
                string path = Directory.GetCurrentDirectory();
                StreamReader sr = new StreamReader(path + "\\EosMonitor.cfg");

                // Ignore the first line (header)) 
                line = sr.ReadLine()!;

                // Read and parse the DriveLens delay:  
                line = sr.ReadLine();
                if ((line != "") && (line != null)){
                    line = line.Substring(line.IndexOf('=') + 1);
                    if (int.TryParse(line, out int num)) { _delayDriveLens = num; }
                }
                // Read and parse the TakePicture delay 
                line = sr.ReadLine();
                if ((line != "") && (line != null)) {
                    line = line.Substring(line.IndexOf('=') + 1);
                    if (int.TryParse(line, out int num)) { _delayTakeStack = num; }
                }
                sr.Close();
                // ReportError("_delay_DriveLens = " + _delay_DriveLens + "    _delay_TakeStack = " + _delay_TakeStack.ToString());
            }
            catch {
                ReportError("Error reading EosMonitor.cfg");
            }
        }
    }
}


