using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace EosMonitor
{
    public partial class MainWindow : Window
    {
        // Show error message box, if many errors occur only the first 4 are reported
        public static void ReportError(string message)
        {
            lock (ErrLock) { ErrCount = ++ErrCount; }
            if (ErrCount < 4)
                System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else if (ErrCount == 4)
                System.Windows.MessageBox.Show("Too many errors happened!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            lock (ErrLock) { ErrCount--; }
        }
        // ShowHistogram:  Displays the histogram of the pixel brightnes values
        public void ShowHistogram()
        {
            //Add the Polygon Element
            System.Windows.Point[] points = new System.Windows.Point[260];
            PointCollection pointCollection = new PointCollection();

            // search for the largest histogram value
            long max = 0;
            for (int i = 0; i < 255; i++)
                if (_histogram[i] > max) { max = _histogram[i]; }
            if (max == 0) return;

            // now valid histogram values available
            double maxX = 100.0 / 256.0;          // sacle factor for the histogram width
            double maxY = 60.0;                   // scale factor for the histogram height

            points[0].X = maxX * 256;             // set the starting point for the polygon
            points[0].Y = maxY;
            pointCollection.Add(points[0]);

            for (int i = 254; i >= 0; i--)
            {      // run through the polygon points counterclockwise
                points[i + 1].X = maxX * i;
                points[i + 1].Y = maxY - maxY * _histogram[i] / max;
                pointCollection.Add(points[i + 1]);
            }

            points[257].X = 0;                     // set the last point for the polygon
            points[257].Y = maxY;
            pointCollection.Add(points[257]);

            // display the histogram graph
            HistogramPolygon.Points = pointCollection;
        }

        // Display the battery Icon
        static void ShowBatteryLevel(long batteryLevel)
        {
            if (cameraModel != null) {
                batteryLevel = (batteryLevel / 10) * 10;
                string bitmapPath = batteryLevel switch
                {
                    0 => "../Resources/BatteryIcons/Battery_00.png",
                    10 => "../Resources/BatteryIcons/Battery_10.png",
                    20 => "../Resources/BatteryIcons/Battery_20.png",
                    30 => "../Resources/BatteryIcons/Battery_30.png",
                    40 => "../Resources/BatteryIcons/Battery_40.png",
                    50 => "../Resources/BatteryIcons/Battery/Battery_50.png",
                    60 => "../Resources/BatteryIcons/Battery_60.png",
                    70 => "../Resources/BatteryIcons/Battery_70.png",
                    80 => "../Resources/BatteryIcons/Battery_80.png",
                    90 => "../Resources/BatteryIcons/Battery_90.png",
                    100 => "../Resources/BatteryIcons/Battery_100.png",
                    _ => "../Resources/BatteryIcons/Battery_AC.png",
                };
                cameraModel.BatteryIcon = bitmapPath;
            }
        }

        // showProgress: Updates the progress bar _point.e. during image download
        void ShowProgress(long percent)
        {
            // Initialize the ProgressBar if it is not net active
            _progressbarActive = true;

            // _Progressbar_active will be reset to "false" after Completion of a transfer in HandleProgressBar()
            ProgressBarGroup.Visibility = Visibility.Visible;
            ProgressBar.Value = percent;
            ProgressBar.Refresh();
        }

        // Decrease opacity of the Eos Monitor logo 
        bool DecreaseLogoOpacity()
        {
            if (Dispatcher.CheckAccess()) {
                _logoOpacity = (_logoOpacity > 0.050) ? _logoOpacity -= 0.050 : 0;
                LogoImage.Opacity = (_logoOpacity > 0.05) ? _logoOpacity : 0;
            }
            else {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, DecreaseLogoOpacity);
            }
            return ((_logoOpacity > 0));
        }

        // Set picture save destination to camera before shootingFocus= new
        void SetPictureSaveDestination()
        {
            if (cameraModel == null) return;

            if (_saveToCamera == false) {
                // set Save Mode to "save to Host"
                if (_imageSaveDirectory != null) {
                    cameraModel.SafeCall(() => {
                        cameraModel.SavePicturesToHost(_imageSaveDirectory);
                    }, ex => ReportError(ex.ToString() + ":   Cannot set Mode to 'Save picture to host'"));
                }
                else {
                    ReportError("ImageSaveDirectory is null: Cannot set Mode to 'Save picture to host'");
                }
            }
            else {
                Thread.Sleep(1000);
                cameraModel.SafeCall(() => {
                    cameraModel.SavePicturesToCamera();
                }, ex => ReportError(ex.ToString() + ":   Cannot set Mode to 'Save picture to camera'"));
            }
        }

        void ResetAutoFocusActiveColors()
        {
            if (Dispatcher.CheckAccess()) {

                if (cameraModel == null) { ReportError("ResetAutoFocusActiveColors: cameraModel==null"); return; }

                cameraModel.EVF_AFButtonIcon = "../Resources/LiveViewIcons/Crosslines.png";
                cameraModel.ZoomRect_Color = GetCyanBrush();

                // Reset focus points
                FP_0.Stroke = GetWhiteBrush();
                FP_1.Stroke = GetWhiteBrush();
                FP_2.Stroke = GetWhiteBrush();
                FP_3.Stroke = GetWhiteBrush();
                FP_4.Stroke = GetWhiteBrush();
                FP_5.Stroke = GetWhiteBrush();
                FP_6.Stroke = GetWhiteBrush();
                FP_7.Stroke = GetWhiteBrush();
                FP_8.Stroke = GetWhiteBrush();
                FP_9.Stroke = GetWhiteBrush();
                FP_10.Stroke = GetWhiteBrush();
                FP_11.Stroke = GetWhiteBrush();
                FP_12.Stroke = GetWhiteBrush();
                FP_13.Stroke = GetWhiteBrush();
                FP_14.Stroke = GetWhiteBrush();
                FP_15.Stroke = GetWhiteBrush();
            }
            else    {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, ResetAutoFocusActiveColors);
            }
        }

        SolidColorBrush GetCyanBrush()
        {
            SolidColorBrush brush = new();
            brush.Color = Colors.Cyan;
            return brush;
        }
        SolidColorBrush GetGreenBrush()
        {
            SolidColorBrush brush = new();
            brush.Color = Colors.Green;
            return brush;
        }
        SolidColorBrush GetWhiteBrush()
        {
            SolidColorBrush brush = new();
            brush.Color = Colors.White;
            return brush;
        }
    }

    // Class DataConversion: Conversion between ID and string values
    public static class DataConversion
    {
        private static CultureInfo cInfo = new("en-US");

        // Get the Av string value from an Av ID
        public static string AV(uint value)
        {
            return value switch
            {
                0x00 => "Auto",
                0x08 => "1",
                0x0B => "1.1",
                0x0C => "1.2",
                0x0D => "1.2 (.3)",
                0x10 => "1.4",
                0x13 => "1.6",
                0x14 => "1.8",
                0x15 => "1.8 (.3)",
                0x18 => "2",
                0x1B => "2.2",
                0x1C => "2.5",
                0x1D => "2.5 (.3)",
                0x20 => "2.8",
                0x23 => "3.2",
                0x24 => "3.5",
                0x25 => "3.5 (.3)",
                0x28 => "4",
                0x2B => "4.5",
                0x2C => "4.5 (.3)",
                0x2D => "5.0",
                0x30 => "5.6",
                0x33 => "6.3",
                0x34 => "6.7",
                0x6D => "80",
                0x35 => "7.1",
                0x38 => " 8",
                0x3B => "9",
                0x3C => "9.5",
                0x3D => "10",
                0x40 => "11",
                0x43 => "13 (.3)",
                0x44 => "13",
                0x45 => "14",
                0x48 => "16",
                0x4B => "18",
                0x4C => "19",
                0x4D => "20",
                0x50 => "22",
                0x53 => "25",
                0x54 => "27",
                0x55 => "29",
                0x58 => "32",
                0x5B => "36",
                0x5C => "38",
                0x5D => "40",
                0x60 => "45",
                0x63 => "51",
                0x64 => "54",
                0x65 => "57",
                0x68 => "64",
                0x6B => "72",
                0x6C => "76",
                0x70 => "91",
                _ => "N/A",
            };
        }

        // Get the ISO string value from an ISO ID
        public static string ISO(uint value)
        {
            return value switch
            {
                0x00000000 => "Auto ISO",
                0x00000028 => "ISO 6",
                0x00000030 => "ISO 12",
                0x00000038 => "ISO 25",
                0x00000040 => "ISO 50",
                0x00000048 => "ISO 100",
                0x0000004b => "ISO 125",
                0x0000004d => "ISO 160",
                0x00000050 => "ISO 200",
                0x00000053 => "ISO 250",
                0x00000055 => "ISO 320",
                0x00000058 => "ISO 400",
                0x0000005b => "ISO 500",
                0x0000005d => "ISO 640",
                0x00000060 => "ISO 800",
                0x00000063 => "ISO 1000",
                0x00000065 => "ISO 1250",
                0x00000068 => "ISO 1600",
                0x00000070 => "ISO 3200",
                0x00000078 => "ISO 6400",
                0x00000080 => "ISO 12800",
                0x00000088 => "ISO 25600",
                0x00000090 => "ISO 51200",
                0x00000098 => "ISO 102400",
                _ => "N/A",
            };
        }

        // Get the Tv string value from an Tv ID
        public static string TV(uint value)
        {
            return value switch
            {
                0x00 => "Auto",
                0x0C => "Bulb",
                0x10 => "30\"",
                0x13 => "25\"",
                0x14 => "20\"",
                0x15 => "20\".3",
                0x18 => "15\"",
                0x1B => "13\"",
                0x1C => "10\"",
                0x1D => "10\" .3",
                0x20 => "8\"",
                0x23 => "6\" .3",
                0x24 => "6\"",
                0x25 => "5\"",
                0x28 => "4\"",
                0x2B => "3\"2",
                0x2C => "3\"",
                0x2D => "2\"5",
                0x30 => "2\"",
                0x33 => "1\"6",
                0x34 => "1\"5",
                0x35 => "1\"3",
                0x38 => "1\"",
                0x3B => "0\"8",
                0x3C => "0\"7",
                0x3D => "0\"6",
                0x40 => "0\"5",
                0x43 => "0\"4",
                0x44 => "0\"3",
                0x45 => "0\"3 .3",
                0x48 => "1/4",
                0x4B => "1/5",
                0x4C => "1/6",
                0x4D => "1/6 .3",
                0x50 => "1/8",
                0x53 => "1/10 .3",
                0x54 => "1/10",
                0x55 => "1/13",
                0x58 => "1/15",
                0x5B => "1/20 .3",
                0x5C => "1/20",
                0x5D => "1/25",
                0x60 => "1/30",
                0x63 => "1/40",
                0x64 => "1/45",
                0x65 => "1/50",
                0x68 => "1/60",
                0x6B => "1/80",
                0x6C => "1/90",
                0x6D => "1/100",
                0x70 => "1/125",
                0x73 => "1/160",
                0x74 => "1/180",
                0x75 => "1/200",
                0x78 => "1/250",
                0x7B => "1/320",
                0x7C => "1/350",
                0x7D => "1/400",
                0x80 => "1/500",
                0x83 => "1/640",
                0x84 => "1/750",
                0x85 => "1/800",
                0x88 => "1/1000",
                0x8B => "1/1250",
                0x8C => "1/1500",
                0x8D => "1/1600",
                0x90 => "1/2000",
                0x93 => "1/2500",
                0x94 => "1/3000",
                0x95 => "1/3200",
                0x98 => "1/4000",
                0x9B => "1/5000",
                0x9C => "1/6000",
                0x9D => "1/6400",
                0xA0 => "1/8000",
                0xA3 => "1/10000",
                0xA5 => "1/12800",
                0xA8 => "1/16000",
                _ => "---",
            };
        }

        // Get the Av ID from an Av string value
        public static uint AV(string value)
        {
            return value switch
            {
                "Auto" => 0x00,
                "1" => 0x08,
                "1.1" => 0x0B,
                "1.2" => 0x0C,
                "1.2 (.3)" => 0x0D,
                "1.4" => 0x10,
                "1.6" => 0x13,
                "1.8" => 0x14,
                "1.8 (.3)" => 0x15,
                "2" => 0x18,
                "2.2" => 0x1B,
                "2.5" => 0x1C,
                "2.5 (.3)" => 0x1D,
                "2.8" => 0x20,
                "3.2" => 0x23,
                "3.5" => 0x24,
                "3.5 (.3)" => 0x25,
                "4" => 0x28,
                "4.5" => 0x2B,
                "4.5 (.3)" => 0x2C,
                "5.0" => 0x2D,
                "5.6" => 0x30,
                "6.3" => 0x33,
                "6.7" => 0x34,
                "7.1" => 0x35,
                " 8" => 0x38,
                "9" => 0x3B,
                "9.5" => 0x3C,
                "10" => 0x3D,
                "11" => 0x40,
                "13 (.3)" => 0x43,
                "13" => 0x44,
                "14" => 0x45,
                "16" => 0x48,
                "18" => 0x4B,
                "19" => 0x4C,
                "20" => 0x4D,
                "22" => 0x50,
                "25" => 0x53,
                "27" => 0x54,
                "29" => 0x55,
                "32" => 0x58,
                "36" => 0x5B,
                "38" => 0x5C,
                "40" => 0x5D,
                "45" => 0x60,
                "51" => 0x63,
                "54" => 0x64,
                "57" => 0x65,
                "64" => 0x68,
                "72" => 0x6B,
                "76" => 0x6C,
                "80" => 0x6D,
                "91" => 0x70,
                _ => 0xffffffff,
            };
        }

        // Get the ISO ID from an ISO string value
        public static uint ISO(string value)
        {
            return value switch
            {
                "Auto ISO" => 0x00000000,
                "ISO 6" => 0x00000028,
                "ISO 12" => 0x00000030,
                "ISO 25" => 0x00000038,
                "ISO 50" => 0x00000040,
                "ISO 100" => 0x00000048,
                "ISO 125" => 0x0000004b,
                "ISO 160" => 0x0000004d,
                "ISO 200" => 0x00000050,
                "ISO 250" => 0x00000053,
                "ISO 320" => 0x00000055,
                "ISO 400" => 0x00000058,
                "ISO 500" => 0x0000005b,
                "ISO 640" => 0x0000005d,
                "ISO 800" => 0x00000060,
                "ISO 1000" => 0x00000063,
                "ISO 1250" => 0x00000065,
                "ISO 1600" => 0x00000068,
                "ISO 3200" => 0x00000070,
                "ISO 6400" => 0x00000078,
                "ISO 12800" => 0x00000080,
                "ISO 25600" => 0x00000088,
                "ISO 51200" => 0x00000090,
                "ISO 102400" => 0x00000098,
                _ => 0xffffffff,
            };
        }

        // Get the Tv ID from an Tv string value
        public static uint TV(string value)
        {
            return value switch
            {
                "Auto" => 0x00,
                "Bulb" => 0x0C,
                "30\"" => 0x10,
                "25\"" => 0x13,
                "20\"" => 0x14,
                "20\"(.3)" => 0x15,
                "15\"" => 0x18,
                "13\"" => 0x1B,
                "10\"" => 0x1C,
                "10\" (.3)" => 0x1D,
                "8\"" => 0x20,
                "6\" (.3)" => 0x23,
                "6\"" => 0x24,
                "5\"" => 0x25,
                "4\"" => 0x28,
                "3\"2" => 0x2B,
                "3\"" => 0x2C,
                "2\"5" => 0x2D,
                "2\"" => 0x30,
                "1\"6" => 0x33,
                "1\"5" => 0x34,
                "1\"3" => 0x35,
                "1\"" => 0x38,
                "0\"8" => 0x3B,
                "0\"7" => 0x3C,
                "0\"6" => 0x3D,
                "0\"5" => 0x40,
                "0\"4" => 0x43,
                "0\"3" => 0x44,
                "0\"3 (.3)" => 0x45,
                "1/4" => 0x48,
                "1/5" => 0x4B,
                "1/6" => 0x4C,
                "1/6 (.3)" => 0x4D,
                "1/8" => 0x50,
                "1/10 (.3)" => 0x53,
                "1/10" => 0x54,
                "1/13" => 0x55,
                "1/15" => 0x58,
                "1/20 (.3)" => 0x5B,
                "1/20" => 0x5C,
                "1/25" => 0x5D,
                "1/30" => 0x60,
                "1/40" => 0x63,
                "1/45" => 0x64,
                "1/50" => 0x65,
                "1/60" => 0x68,
                "1/80" => 0x6B,
                "1/90" => 0x6C,
                "1/100" => 0x6D,
                "1/125" => 0x70,
                "1/160" => 0x73,
                "1/180" => 0x74,
                "1/200" => 0x75,
                "1/250" => 0x78,
                "1/320" => 0x7B,
                "1/350" => 0x7C,
                "1/400" => 0x7D,
                "1/500" => 0x80,
                "1/640" => 0x83,
                "1/750" => 0x84,
                "1/800" => 0x85,
                "1/1000" => 0x88,
                "1/1250" => 0x8B,
                "1/1500" => 0x8C,
                "1/1600" => 0x8D,
                "1/2000" => 0x90,
                "1/2500" => 0x93,
                "1/3000" => 0x94,
                "1/3200" => 0x95,
                "1/4000" => 0x98,
                "1/5000" => 0x9B,
                "1/6000" => 0x9C,
                "1/6400" => 0x9D,
                "1/8000" => 0xA0,
                _ => 0xffffffff,
            };
        }
    }
}

