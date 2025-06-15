using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EDSDKLib;

namespace EosMonitor
{
    public partial class MainWindow : Window
   {

        public void UpdateFocusInfo()
        {
            // called from the FocusInfoTimer 
            // Get 'FococusInformation' from the camera and store it in 'localFococusInformation'
            // Update the focus and zoom rectangles

            if (cameraModel == null) { ReportError("UpdateFocusInfo: cameraModel==null"); return; }
            if (cameraModel.IsInLiveViewMode == false) { return; }        // LiveView mode must be ON
            if (cameraModel.EvfZoom != EvfZoomFactor.fit) { return; }        // Zoom must be off

            // No focus Information is available in Liveview Face detection Mode
            if ((cameraModel.EVF_AutoFocusMode == LiveViewAutoFocus.LiveFaceMode) || (cameraModel.LockTakePicture = false)) {
                cameraModel.LockTakePicture = false; return;
            }
                cameraModel.LocalFocusInformation = cameraModel.FocusInformation;

            if (_zoomPositionValid == false) {
                //  Get the zoom rectangle from the camera
                EDSDK.EdsRect ZoomRect = new();
                EDSDK.EdsPoint zoomPos;
                ZoomRect = GetEvfZoomRect(_efvImage);
                zoomPos.x = ZoomRect.x;
                zoomPos.y = ZoomRect.y;
                if ((zoomPos.x > 0) && (zoomPos.x > 0)) cameraModel.ZoomPosition = zoomPos;
                _zoomPositionValid = true;
            }
            ShowZoomRectangle();
            ShowAFPoints();
        }

        public void ShowZoomRectangle() 
        {
            if (cameraModel == null) { ReportError("ShowZoomRectangle: cameraModel==null"); return; }

            // Scale factor:  (width of the display image) / (width of the camera image)
            double scaleFactor = (Width - 30 - 8) / cameraModel.LocalFocusInformation.Bounds.Width;

            // Zoom rectangle parameters on the Display (for zoom factor fit)
            cameraModel.ZoomRect_Width  = cameraModel.LocalFocusInformation.Bounds.Width / 5 * scaleFactor;
            cameraModel.ZoomRect_Height = cameraModel.LocalFocusInformation.Bounds.Height / 5 * scaleFactor;
            cameraModel.ZoomRect_X      = cameraModel.ZoomPosition.x * scaleFactor;
            cameraModel.ZoomRect_Y      = cameraModel.ZoomPosition.y * scaleFactor;
            cameraModel.ZoomRect_Color  = GetCyanBrush(); 

            ZoomRectangle.Visibility = Visibility.Visible;
        }

        // Show focus points 
        public void ShowAFPoints()
        {
            if (cameraModel == null) { ReportError("ShowAFPoints: cameraModel==null"); return; };

            // Scale factor:  (width of the display image) / (width of the camera image)
            double scaleFactor = (Width - 30 - 8) / cameraModel.LocalFocusInformation.Bounds.Width;

            List<Rectangle> FP = new List<Rectangle>()
            { FP_0, FP_1, FP_2, FP_3, FP_4,FP_5, FP_6, FP_7, FP_8, FP_9, FP_10, FP_11, FP_12, FP_13, FP_14, FP_15 };

            if (cameraModel.LocalFocusInformation.FocusPoints.Length > 0) {
                int FpNumber = cameraModel.LocalFocusInformation.FocusPoints.Length; 
                for (int fpIndex = 0; fpIndex < 15; fpIndex++) {
                    if (fpIndex < FpNumber)
                    {
                        FP[fpIndex].Height = cameraModel.LocalFocusInformation.FocusPoints[fpIndex].Bounds.Height * scaleFactor;
                        FP[fpIndex].Width = cameraModel.LocalFocusInformation.FocusPoints[fpIndex].Bounds.Width * scaleFactor;
                        FP[fpIndex].SetValue(Canvas.LeftProperty, (double)(cameraModel.LocalFocusInformation.FocusPoints[fpIndex].Bounds.X * scaleFactor));
                        FP[fpIndex].SetValue(Canvas.TopProperty,  (double)(cameraModel.LocalFocusInformation.FocusPoints[fpIndex].Bounds.Y * scaleFactor));
                        FP[fpIndex].Visibility = Visibility.Visible;
                        FP[fpIndex].Stroke = GetFocusRectangleBrush(0);
                        if (cameraModel.EvfZoom == EvfZoomFactor.fit) FP[fpIndex].Visibility = Visibility.Visible;
                    }
                    else {
                        FP[fpIndex].Visibility = Visibility.Hidden;
                   }
                }
            }
        }

        // Get zoom rectangle from the camera
        private EDSDK.EdsRect GetEvfZoomRect(IntPtr imgRef)
        {
            int size = Marshal.SizeOf(typeof(EDSDK.EdsRect));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            uint err = EDSDK.EdsGetPropertyData(imgRef, EDSDK.PropID_Evf_ZoomRect, 0, size, ptr);

            EDSDK.EdsRect rect = (EDSDK.EdsRect)Marshal.PtrToStructure(ptr, typeof(EDSDK.EdsRect));
            Marshal.FreeHGlobal(ptr);
            if (err == EDSDK.EDS_ERR_OK) return rect;
            else return new EDSDK.EdsRect();
        }

        // HideFocus and ZoomRectangles
        private void HideFocusAndZoomRectangles() 
        {
            ZoomRectangle.Visibility = Visibility.Hidden;
            FP_0.Visibility  = Visibility.Hidden;
            FP_1.Visibility  = Visibility.Hidden;
            FP_2.Visibility  = Visibility.Hidden;
            FP_3.Visibility  = Visibility.Hidden;
            FP_4.Visibility  = Visibility.Hidden;
            FP_5.Visibility  = Visibility.Hidden;
            FP_6.Visibility  = Visibility.Hidden;
            FP_7.Visibility  = Visibility.Hidden;
            FP_8.Visibility  = Visibility.Hidden;
            FP_9.Visibility  = Visibility.Hidden;
            FP_10.Visibility = Visibility.Hidden;
            FP_11.Visibility = Visibility.Hidden;
            FP_12.Visibility = Visibility.Hidden;
            FP_13.Visibility = Visibility.Hidden;
            FP_14.Visibility = Visibility.Hidden;
            FP_15.Visibility = Visibility.Hidden;
        }

        // Set brush for zoom and focus rectangles
        public SolidColorBrush GetFocusRectangleBrush(uint i) 
        {
           if(cameraModel  == null) return new SolidColorBrush(Colors.Black);
           if (cameraModel == null) { ReportError("GetFocusRectangleBrush: cameraModel==null"); return new SolidColorBrush(Colors.Black); }

           SolidColorBrush FP_Brush = new SolidColorBrush();
           if (cameraModel.LocalFocusInformation.FocusPoints[i].IsSelected)
              FP_Brush.Color = Colors.Red;
           if (cameraModel.LocalFocusInformation.FocusPoints[i].IsInFocus)
              FP_Brush.Color = Colors.White;
           else FP_Brush.Color = Colors.LightGray;

           return FP_Brush;
        }
   }    
}
