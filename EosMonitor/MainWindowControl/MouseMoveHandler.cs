using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace EosMonitor
{
    public partial class MainWindow : Window
    {
        // Handle mouse enter/leave events for the buttons

        private void TakePictureButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            if (cameraPtr == IntPtr.Zero) { ReportError("TakePictureButton_MouseEnter: cameraModel==null"); return; }

            // "PressHalfway" is fobidden in Bulb mode while the shutter is open and in Live View mode
            if (!cameraModel.ShutterIsClosed || cameraModel.IsInLiveViewMode) return;
            PressHalfway();
        }

        private void LVButtonHG_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LVButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void LVButtonHG_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            LVButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void CameraComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CameraComboBoxHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void CameraComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CameraComboBoxHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void IsoComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            IsoComboBoxHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void IsoComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            IsoComboBoxHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void ExposureTimeValueComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            ExposureTimeValueComboBoxHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void ExposureTimeValueComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            ExposureTimeValueComboBoxHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void ApertureValueComboBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            ApertureValueComboBoxHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void ApertureValueComboBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            ApertureValueComboBoxHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void DriveModeButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if (cameraCount > 0)
            {
                // hide "Drive" text, show Arrows
                DriveModeArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
                DriveModeLabel.Visibility = Visibility.Hidden;
                DriveModeBG.BorderThickness = new Thickness(1, 1, 1, 2);
            }
        }
        private void DriveModeButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if (cameraCount > 0)
            {
                // show "Drive" text, hide Arrows
                DriveModeArrow.Template = null;
                DriveModeLabel.Visibility = Visibility.Visible;
                DriveModeBG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            }
        }

        private void MeterModeButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;

            if (cameraCount > 0)
            {

                if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

                // hide "Drive" text, show Arrows
                MeterModeArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
                MeterModeLabel.Visibility = Visibility.Hidden;
                MeterModeButton.BorderThickness = new Thickness(1, 1, 1, 2);
            }
        }
        private void MeterModeButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if (cameraModel == null) { ReportError("ResetAutoFocusActiveColors: cameraModel==null"); return; }

            if (cameraCount > 0)
            {
                if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

                // show "Drive" text, hide Arrows
                MeterModeArrow.Template = null;
                MeterModeLabel.Visibility = Visibility.Visible;
                MeterModeButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            }
        }

        private void DSLR_AFMButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

            if (cameraCount > 0)
            {
                // hide "AF" text, show Arrows
                DSLR_AFMArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
                DSLR_AFMArrow.Visibility = Visibility.Visible;
                DSLR_AFMLabel.Visibility = Visibility.Hidden;
                DSLR_AFMButton.BorderThickness = new Thickness(1, 1, 1, 2);
            }
        }
        private void DSLR_AFMButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

            if (cameraCount > 0)
            {
                // show "AF" text, hide Arrows
                DSLR_AFMArrow.Visibility = Visibility.Hidden;
                DSLR_AFMLabel.Visibility = Visibility.Visible;
                DSLR_AFMButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            }
        }
        private void DSLM_AFMButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            // If not in LiveView mode show up/down-controls the button is active
            if ((cameraCount > 0) && (!cameraModel.IsInLiveViewMode))
            {
                // hide "AF" text, show Arrows
                DSLM_AFMArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
                DSLM_AFMArrow.Visibility = Visibility.Visible;
                DSLM_AFMLabel.Visibility = Visibility.Hidden;
                DSLM_AFMButton.BorderThickness = new Thickness(1, 1, 1, 2);
            }
        }
        private void DSLM_AFMButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) return;
            if ((cameraCount > 0) && (!cameraModel.IsInLiveViewMode))
            {
                // show "AF" text, hide Arrows
                DSLM_AFMArrow.Visibility = Visibility.Hidden;
                DSLM_AFMLabel.Visibility = Visibility.Visible;
                DSLM_AFMButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            }
        }

        private void Bulb_Timer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraCount > 0) Bulb_Timer.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void Bulb_Timer_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraCount > 0) Bulb_Timer.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void BTHours_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("BTButtonHoursTemplate") as ControlTemplate;
            BT_h.Visibility = Visibility.Hidden;
        }
        private void BTHours_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("StandardButtonTemplate1") as ControlTemplate;
            BT_h.Visibility = Visibility.Visible;
        }
        private void BTMinutes_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("BTButtonMinutesTemplate") as ControlTemplate;
            BT_min.Visibility = Visibility.Hidden;
        }
        private void BTMinutes_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("StandardButtonTemplate1") as ControlTemplate;
            BT_min.Visibility = Visibility.Visible;
        }
        private void BTSeconds_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("BTButtonSecondsTemplate") as ControlTemplate;
            BT_sec.Visibility = Visibility.Hidden;
        }
        private void BTSeconds_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Bulb_Timer.Template = FindResource("StandardButtonTemplate1") as ControlTemplate;
            BT_sec.Visibility = Visibility.Visible;
        }

        private void EVCButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ;
            if (cameraPtr == IntPtr.Zero) { return; }
            ;

            if (cameraModel.AEMode != 3)
            {
                EVCGroupHG.BorderThickness = new Thickness(1, 1, 1, 2);
                EVCGroupHG.BorderBrush = System.Windows.Media.Brushes.SlateGray;
                EVCGroupHG.Height = 40;
                EVCLeft.Visibility = Visibility.Visible;
                EVCRight.Visibility = Visibility.Visible;
                EVCLeftRect.Visibility = Visibility.Visible;
                EVCRightRect.Visibility = Visibility.Visible;
                EVPlusMinusIcon.Visibility = Visibility.Visible;
            }
        }
        private void EVCButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EVCGroupHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
            EVCGroupHG.BorderBrush = System.Windows.Media.Brushes.LightSlateGray;
            EVCGroupHG.Height = 30;
            EVCLeft.Visibility = Visibility.Hidden;
            EVCRight.Visibility = Visibility.Hidden;
            EVCLeftRect.Visibility = Visibility.Hidden;
            EVCRightRect.Visibility = Visibility.Hidden;
            EVPlusMinusIcon.Visibility = Visibility.Hidden;
        }

        private void Evf_ZoomIcon_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            ;
            //if (cameraModel.ZoomIcon == "../Resources/LiveViewIcons/ZoomPlus.png") cameraModel.ZoomIcon = "../Resources/LiveView/ZoomPlus_active.png";
            //if (cameraModel.ZoomIcon == "../Resources/LiveViewIcons/ZoomMinus.png") cameraModel.ZoomIcon = "../Resources/LiveView/ZoomMinus_active.png";
        }
        private void Evf_ZoomIcon_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            //if (cameraModel.ZoomIcon == "../Resources/LiveViewIcons/ZoomPlus_active.png") cameraModel.ZoomIcon = "../Resources/LiveView/ZoomPlus.png";
            //if (cameraModel.ZoomIcon == "../Resources/LiveViewIcons/ZoomMinus_active.png") cameraModel.ZoomIcon = "../Resources/LiveView/ZoomMinus.png";
        }

        private void Evf_Histogram_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            if (cameraModel.HistogramIcon == "../Resources/LiveViewIcons/HistogramIcon.png") cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon_active.png";
            if (cameraModel.HistogramIcon == "../Resources/LiveViewIcons/HistogramIcon_green.png") cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon_green_active.png";
        }
        private void Evf_Histogram_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            ;
            if (cameraModel.HistogramIcon == "../Resources/LiveViewIcons/HistogramIcon_active.png") cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon.png";
            if (cameraModel.HistogramIcon == "../Resources/LiveViewIcons/HistogramIcon_green_active.png") cameraModel.HistogramIcon = "../Resources/LiveViewIcons/HistogramIcon_green.png";
        }

        private void Evf_AFButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            ;
            cameraModel.CrosslinesIcon = "../Resources/LiveViewIcons/Crosslines_active.png";
        }
        private void Evf_AFButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null || _evf_AFTimer_Elapsed == false) return;
            cameraModel.CrosslinesIcon = "../Resources/LiveViewIcons/Crosslines.png";
        }

        private void LvAFButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;  

            DSLR_LvAFModeArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
            DSLR_LvAFLabel.Visibility = Visibility.Visible;
            DSLR_LvAFModeButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void LvAFButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            if ((cameraModel.CameraType == "DSLR") && cameraModel.IsInLiveViewMode) return;

            DSLR_LvAFModeArrow.Template = null;
            DSLR_LvAFLabel.Visibility = Visibility.Visible;
            DSLR_LvAFModeButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void ExitButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ExitButtonHG.Visibility = Visibility.Visible;
            ExitButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void ExitButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            ExitButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void RestartButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            RestartButtonHG.Visibility = Visibility.Visible;
            RestartButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void RestartButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            RestartButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void DriveRightButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            DriveRightButtonIcon.Foreground = Brushes.SlateGray;
            DriveRightButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void DriveRightButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            DriveRightButtonIcon.Foreground = Brushes.Black;
            DriveRightButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }
        private void DriveLeftButtom_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            DriveLeftButtonIcon.Foreground = Brushes.SlateGray;
            DriveLeftButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void DriveLeftButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { ReportError("cameraModel==null"); return; }
            DriveLeftButtonIcon.Foreground = Brushes.Black;
            DriveLeftButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void NrFocusStepsButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            FocusStepsArrow.Visibility = Visibility.Visible;
            FocusStepsArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
            FocusStepsButton.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void NrFocusStepsButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            FocusStepsArrow.Visibility = Visibility.Hidden;
            FocusStepsButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void WidthOfFocusStepsButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            WidthOfFocusStepsArrow.Visibility = Visibility.Visible;
            WidthOfFocusStepsArrow.Template = FindResource("ArrowButtonTemplate") as ControlTemplate;
            WidthOfFocusStepsButton.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void WidthOfFocusStepsButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            WidthOfFocusStepsArrow.Visibility = Visibility.Hidden;
            WidthOfFocusStepsArrow.Template = null;
            WidthOfFocusStepsButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void StartStackingButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            StartStackingEllipseDown.Visibility = Visibility.Visible;
            StartStackingEllipseUp.Visibility = Visibility.Hidden;
            StartStackingButton.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void StartStackingButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            StartStackingEllipseDown.Visibility = Visibility.Hidden;
            StartStackingEllipseUp.Visibility = Visibility.Visible;
            StartStackingButton.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

        private void StackingButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            if ((cameraCount > 0) && (!cameraModel.IsInLiveViewMode))
            DSLM_FStackingButtonHG.BorderThickness = new Thickness(1, 1, 1, 2);
        }
        private void StackingButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (cameraModel == null) { return; }
            if ((cameraCount > 0) && (!cameraModel.IsInLiveViewMode))
            DSLM_FStackingButtonHG.BorderThickness = new Thickness(0.5, 0.5, 0.5, 0.5);
        }

    }
}
