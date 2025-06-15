using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Windows;
using EDSDKLib;


namespace EosMonitor
{
    public partial class MainWindow : Window
    {

        // Setup the AEModeLabel 
        private void Setup_AEModeLabel()
        {
            if (cameraModel == null) { ReportError("Setup_AEModeLabel: cameraModel==null"); return; }

            LVButton.Opacity = 1.0;
            LiveViewLabel.Opacity = 1.0;
            LVImage.Opacity = 1.0;

            string aemodeStr;
            uint aemodeNr = cameraModel.GetSetting(EDSDK.PropID_AEMode);
            switch (aemodeNr)
            {
                case 0: aemodeStr = " P"; break;
                case 1: aemodeStr = "Tv"; break;
                case 2: aemodeStr = "Av"; break;
                case 3: aemodeStr = " M"; break;
                case 4:
                    aemodeStr = " B "; Setup_BulbTimerControl(Visibility.Visible);
                    // In bulb mode no LiveView!!
                    LVButton.Opacity = 0.0;
                    LiveViewLabel.Opacity = 0.0;
                    LVImage.Opacity = 0.0;
                    cameraModel.LiveViewDevice = cameraModel.LiveViewDevice = LiveViewDevice.Camera;
                    break;
                default: aemodeStr = " "; break;
            }
            AutoExposureMode.Content = aemodeStr;
        }

        // Setup and update the bulb timer controls
        private void Setup_BulbTimerControl(Visibility vis)
        {
            if (cameraModel == null) { ReportError("Setup_BulbTimerControl: cameraModel==null"); return; }

            Bulb_Timer.Visibility = vis;
            Bulb_TimerCanvas.Visibility = vis;
            cameraModel.BTHours = 0;
            cameraModel.BTMinutes = 0;
            cameraModel.BTSeconds = 0;
            cameraModel.LvStartStop = "Start";
            LVButton.Opacity = 1.0;
            LVImage.Opacity = 1.0;
        }
        private void UpdateBulbTimer(int step)
        {
            if (cameraModel == null) { ReportError("UpdateBulbTimer: cameraModel==null"); return; }

            uint nextValue = (uint)(cameraModel.BTHours * 3600 + cameraModel.BTMinutes * 60 + cameraModel.BTSeconds + step);
            if ((nextValue > 359999) || (nextValue < 0)) return;
            cameraModel.BTHours = nextValue / 3600;
            cameraModel.BTMinutes = (nextValue - cameraModel.BTHours * 3600) / 60;
            cameraModel.BTSeconds = (nextValue - cameraModel.BTHours * 3600 - cameraModel.BTMinutes * 60);
        }

        //Setup the Av-, Tv- Iso- controls
        private void Setup_Av_Tv_Iso_ComboBoxes(long aemode)
        {
            // How AvValueList, TvValueList, ISOList are initialized
            #region 
            // lists that can be read by "GetSettingsList(aemode)" depends on the actual AE mode:
            // aemode 0="P" :  Iso 
            // aemode 1="Tv":  Iso, Tv 
            // aemode 2="Av":  Iso, Av 
            // aemode 3="M" :  Iso, Av and Tv
            // aemode 4="B" :  Iso, Av

            // How we get the Av- and Tv-value lists:
            // Starting in AE mode = Av the TvValueList remains empty until AE mode M or Tv is selected on the camera
            // Starting in AE mode = Tv the AvValueList remains empty until AE mode Av is selected on the camera
            // So fixed lists are used insted of requiring from the EDSDK by:
            // AvValueList  = cameraMgr.GetSettingsList(EDSDK.PropID_Av);
            // TvValueList  = cameraMgr.GetSettingsList(EDSDK.PropID_Tv);

            // The ISOList can always be read freom the EDSDK
            #endregion

            uint ISOValue;
            uint AvValue;
            uint TvValue;
            string SelectedValue;
            List<int> localList = new();

            if (cameraModel == null) { ReportError("Setup_Av_Tv_Iso_ComboBoxes: cameraModel==null"); return; }
            _lockPropChanged = true;

            // Get Iso values for the Combobox from the camera 
            localList = cameraModel.GetSettingsList(EDSDK.PropID_ISOSpeed);
            if (localList.Count != 0) ISOList = localList;

            IsoComboBox.Items.Clear();
            foreach (int ISO in ISOList) IsoComboBox.Items.Add(DataConversion.ISO((uint)ISO));
            ISOValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_ISOSpeed);
            SelectedValue = DataConversion.ISO(ISOValue);
            IsoComboBox.SelectedValue = SelectedValue;

            // Get Tv values for the Combobox from the camera for modes Tv and M
            if ((aemode == 1) || (aemode == 3)) {
                localList = cameraModel.GetSettingsList(EDSDK.PropID_Tv);
                if (localList.Count != 0) TvList = localList;

                ExposureTimeValueComboBox.Items.Clear();
                foreach (int Tv in TvList) ExposureTimeValueComboBox.Items.Add(DataConversion.TV((uint)Tv));
                TvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Tv);
                SelectedValue = DataConversion.TV(TvValue);
                ExposureTimeValueComboBox.SelectedValue = SelectedValue;
            }
            // Get Av values for the Combobox from the camera for modes Av and B
            if ((aemode == 2) || (aemode == 3) || (aemode == 4)) {
                localList = cameraModel.GetSettingsList(EDSDK.PropID_Av);
                if (localList.Count != 0) AvList = localList;

                ApertureValueComboBox.Items.Clear();
                foreach (int Av in AvList) ApertureValueComboBox.Items.Add(DataConversion.AV((uint)Av));
                AvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Av);
                SelectedValue = DataConversion.AV(AvValue);
                ApertureValueComboBox.SelectedValue = SelectedValue;
            }

            switch (aemode)
            {
                // AEMode = P:  deactivate both Av- and Tv-Comboboxes
                case 0:
                    ExposureTimeValueComboBox.IsEnabled = false;
                    ExposureTimeValueLabel.Visibility = Visibility.Hidden;
                    TVLabel.Opacity = 0.5;

                    ApertureValueComboBox.IsEnabled = false;
                    ApertureValueLabel.Visibility = Visibility.Hidden;
                    AVLabel.Opacity = 0.5;

                    EVCGroupHG.Opacity = 1.0;
                    EVCCanvas.Opacity = 1.0;
                    cameraModel.EVCValue = 0;
                    break;

                // AEMode = Tv: activate Tv-ComboBox, deactivate Av-ComboBox
                case 1:
                    ExposureTimeValueComboBox.IsEnabled = true;
                    ExposureTimeValueLabel.Visibility = Visibility.Visible;
                    TVLabel.Opacity = 1.0;
                    TvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Tv);
                    SelectedValue = DataConversion.TV(TvValue);
                    ExposureTimeValueComboBox.SelectedValue = SelectedValue;

                    ApertureValueComboBox.IsEnabled = false;
                    ApertureValueLabel.Visibility = Visibility.Hidden;
                    AVLabel.Opacity = 0.5;

                    EvcMarker.Visibility = Visibility.Visible;
                    EVCCanvas.Opacity = 1.0;
                    cameraModel.EVCValue = 0;
                    break;

                // AEMode = Av: activate Av-ComboBox, deactivate Tv-ComboBox
                case 2:
                    ApertureValueComboBox.IsEnabled = true;
                    ApertureValueLabel.Visibility = Visibility.Visible;
                    AVLabel.Opacity = 1.0;
                    AvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Av);
                    SelectedValue = DataConversion.AV(AvValue);
                    ApertureValueComboBox.SelectedValue = SelectedValue;
                    cameraModel.ApertureStr = SelectedValue;

                    ExposureTimeValueComboBox.IsEnabled = false;
                    ExposureTimeValueLabel.Visibility = Visibility.Hidden;
                    TVLabel.Opacity = 0.5;

                    EvcMarker.Visibility = Visibility.Visible;
                    EVCCanvas.Opacity = 1.0;
                    cameraModel.EVCValue = 0;
                    break;

                // AEMode = M:  activate Av-ComboBox and Tv-ComboBox
                case 3:
                    ExposureTimeValueComboBox.IsEnabled = true;
                    ExposureTimeValueLabel.Visibility = Visibility.Visible;
                    TVLabel.Opacity = 1.0;
                    TvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Tv);
                    SelectedValue = DataConversion.TV(TvValue);
                    ExposureTimeValueComboBox.SelectedValue = SelectedValue;

                    ApertureValueComboBox.IsEnabled = true;
                    ApertureValueLabel.Visibility = Visibility.Visible;
                    AVLabel.Opacity = 1.0;
                    AvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Av);
                    SelectedValue = DataConversion.AV(AvValue);
                    ApertureValueComboBox.SelectedValue = SelectedValue;
                    cameraModel.ApertureStr = SelectedValue;

                    EvcMarker.Visibility = Visibility.Hidden;
                    EVCCanvas.Opacity = 0.5;
                    break;

                //AEMode = Bulb: activate Av-ComboBox, deactivate Tv-ComboBox
                case 4:
                    ApertureValueComboBox.IsEnabled = true;
                    ApertureValueLabel.Visibility = Visibility.Visible;
                    AVLabel.Opacity = 1.0;
                    AvValue = (uint)cameraModel.GetPropertyIntegerData(EDSDK.PropID_Av);
                    SelectedValue = DataConversion.AV(AvValue);
                    ApertureValueComboBox.SelectedValue = SelectedValue;
                    cameraModel.ApertureStr = SelectedValue;

                    ExposureTimeValueComboBox.IsEnabled = false;
                    ExposureTimeValueLabel.Visibility = Visibility.Hidden;
                    TVLabel.Opacity = 0.5;

                    EvcMarker.Visibility = Visibility.Hidden;
                    EVCCanvas.Opacity = 0.5;
                    break;
            }
            _lockPropChanged = false;
        }

        // Setup drive-, Meter, AF-Mode controls
        private void Setup_DriveMode_Button(uint UpDown)
        {
            // Parameter UpDown:
            // UpDown:  0  Initialize
            // 1: called from DriveModeUp_Click
            // 2: called from DriveModeDown_Click

            // Definition of the Drive Mode Codes
            #region
            //DriveModeName[0] = "Single shooting";             value=0 
            //DriveModeName[1] = "Continuous Shooting";
            //DriveModeName[2] = "Video";
            //DriveModeName[3] = "---";
            //DriveModeName[4] = "High speed continuous";       value = 4
            //DriveModeName[5] = "Low speed continuous";        value = 5
            //DriveModeName[6] = "Single Silent shooting";      
            //DriveModeName[7] = "Self-timer:Continuous";
            //DriveModeName[8] = "---";
            //DriveModeName[9] = "---";
            //DriveModeName[10] = "Self-timer:10 sec";           value=17
            //DriveModeName[11] = "Self-timer: 2 sec";           value=16
            //DriveModeName[12] = "14 fps super high speed";
            //DriveModeName[13] = "Silent single shooting";
            //DriveModeName[14] = "Silent contin shooting";
            //DriveModeName[15] = "Silent HS continuous";
            //DriveModeName[16] = "Silent LS continuous";
            #endregion

            if (cameraModel == null) { ReportError("Setup_DriveMode_Button: cameraModel==null"); return; }
            if (cameraCount == 0) { ReportError("cameraCount==0"); return; }

            // Get the current drive mode from the camera
            uint data = (uint)cameraModel.DrMValue;

            // Initialize the drive Mode Button 
            switch (UpDown) {
                case 0:
                    // Initialize the drive mode button
                    ShowDriveModeIcon(data);
                    break;
                case 1:
                    // Update the drive mode button after DriveModeUp_Click
                    if (data == 0) data = 4;
                    else if (data == 4) data = 5;
                    else if (data == 5) data = 16;
                    else if (data == 16) data = 17;
                    else data = 0;
                    cameraModel.DrMValue = data;
                    ShowDriveModeIcon(data);
                    break;
                case 2:
                    // Update the drive mode button after DriveModeDown_Click
                    if (data == 0) data = 17;
                    else if (data == 17) data = 16;
                    else if (data == 16) data = 5;
                    else if (data == 5) data = 4;
                    else data = 0;
                    cameraModel.DrMValue = data;
                    ShowDriveModeIcon(data);
                    break;
                default:
                    break;
            }
        }
        private void Setup_MeterMode_Button(uint UpDown)
        {
            // UpDown:  0   Initialize
            //          1: called from DriveModeUp_Click
            //          2: called from DriveModeDown_Click

            // Definition of the meter mode codes
            #region
            // MeterMode Spot
            // Spot metering                        1
            // Evaluative metering                  3
            // Partial metering                     4
            // Center-weighted averaging metering   5

            #endregion

            if (cameraModel == null) { ReportError("Setup_MeterMode_Button: cameraModel==null"); return; }
            if (cameraCount == 0) { ReportError("cameracount==0"); return; }

            //Get the current meter mode from the camera
            uint data = (uint)cameraModel.MMValue;

            // Initialize the drive Mode Button 
            switch (UpDown) {
                case 0:
                    // Initialize the meter mode button
                    ShowMeterModeIcon();
                    break;
                case 1:
                    // Update the meter mode button after DriveModeUp_Click
                    if (data == 1) data = 3;
                    else if (data == 3) data = 4;
                    else if (data == 4) data = 5;
                    else if (data == 5) data = 1;
                    else data = 1;
                    cameraModel.MMValue = data;
                    ShowMeterModeIcon();
                    break;
                case 2:
                    // Update the meter mode button after DriveModeDown_Click
                    if (data == 1) data = 5;
                    else if (data == 5) data = 4;
                    else if (data == 4) data = 3;
                    else if (data == 3) data = 1;
                    else data = 1;
                    cameraModel.MMValue = data;
                    ShowMeterModeIcon();
                    break;
                default:
                    break;
            }
        }
        private void Setup_DSLR_AFMode_Button(uint UpDown)
        {

            // Routine is called from:
            // UpDown:  0  Initialize
            //          1: called from DSLR_AFMUp_Click
            //          2: called from DSLR_AFMDown_Click
            #region
            // Autofocus modes:
            // OneShot          0   
            // AIServo          1   
            // AIFocus          2   
            // Manual           0xFF;  read only !!!          
            #endregion

            if (cameraModel == null) { ReportError("Setup_DSLR_AFMode_Button: cameraModel==null"); return; }
            if (cameraCount == 0) { ReportError("cameraCount===0"); return; }

            // Get the current meter mode from the camera
            uint data = (uint)cameraModel.AFMValue;

            // Initialize the drive Mode Button 
            switch (UpDown) {
                case 0: // Initialize the AF mode button
                    ShowAFMText(data);
                    break;
                case 1: // Update the autofocus mode button after DriveModeUp_Click
                    if (data == 0) data = 1;            // OneShot --> AIServo
                    else if (data == 1) data = 2;       // AIServo --> AIFocus
                    else if (data == 2) data = 0;       // AIFocus --> OneShot
                    ShowAFMText(data);
                    cameraModel.AFMValue = data;
                    break;
                case 2: // Update the meter mode button after DriveModeDown_Click
                    if (data == 0) data = 2;            // OneShot --> AIFocus
                    else if (data == 2) data = 1;       // AIFocus --> AIServo
                    else if (data == 1) data = 0;       // AIServo --> OneShot
                    ShowAFMText(data);
                    cameraModel.AFMValue = data;
                    break;
                default:
                    break;
            }
        }
        private void Setup_DSLM_AFMButton(uint UpDown)
        {
            // UpDown:  0  Initialize
            //          1: called from DriveModeUp_Click
            //          2: called from DriveModeDown_Click

            // Definition of the autofocus mode codes
            #region 
            //  EOS R6 AF modes:
            // -----------------------------------------------------------------------------------------------------------------
            //   0   Evf_AFMode_Quick                       Quick Mode                      Quick-Modus (den optischen Sucher) 
            //   2   Evf_AFMode_Liv_Face                    Face + Tracking                 Gesichtserkennung + Motiv-Verfolgung
            //  10   Evf_AFMode_Spot                        Spot                            Spot AF
            //   1   Evf_AFMode_Live                        Live                            Einzelfeld AF
            //   5   Evf_AFMode_LiveSingleExpandCross       Expand AF area                  AF-Bereich erweitert
            //   6   Evf_AFMode_LiveSingleExpandSurround    Expand AF area surrounding      AF-Bereich Umgebung
            //   4   Live Zone AF                           Zone AF                         AF-Bereich Zone
            //   8   Evf_AFMode_LiveZoneLargeH              Large Zone AF: Horizontal       AF-Bereich Zone groß: Horizontal
            //   7   Evf_AFMode_LiveZoneLargeV              Large Zone AF: Vertical         AF-Bereich Zone groß: Vertikal

            // EOS 60D: AF mode codes
            // OneShot          0   
            // AIServo          1   
            // AIFocus          2   
            // Manual           0xFF;  read only !!!
            #endregion

            if (cameraModel == null) { ReportError("Setup_DSLM_AFMButton: cameraModel==null"); return; }
            if (cameraCount == 0) { ReportError("cameraCount===0"); return; }

            // Get the current AF mode and EvfAFValueList from the camera
            long current_AFM = (long)cameraModel.LvAFValue;
            int[] myEvfAFValueList = cameraModel.LvAFValueList;
            int myEvfAFMValueListLenght = cameraModel.LvAFListLength;

            // search index of current AF mode in the EvfAFValueList
            uint currentAFMindex = 0;
            for (int i = 0; i < myEvfAFValueList.Length; i++) {
                if (myEvfAFValueList[i] == current_AFM)
                {
                    currentAFMindex = (uint)i;
                    break;
                }
            }
            // Select the next or previous AF mode
            switch (UpDown) {
                case 0: // Initialize the AF mode button
                    Show_AFM_button();
                    break;
                case 1: // Update after DriveModeUp_Click
                    if (currentAFMindex == myEvfAFMValueListLenght - 1)
                        currentAFMindex = 0;
                    else currentAFMindex++;
                    cameraModel.LvAFValue = myEvfAFValueList[currentAFMindex];
                    Show_AFM_button();
                    break;
                case 2: // Update after DriveModeDown_Click
                    if (currentAFMindex == 0)
                        currentAFMindex = (uint)myEvfAFMValueListLenght - 1;
                    else currentAFMindex--;
                    cameraModel.LvAFValue = myEvfAFValueList[currentAFMindex];
                    Show_AFM_button();
                    break;
                default:
                    break;
            }
            DragFinished(true);
        }

        // Display the icons for the drive-, Meter, AF-Mode controls
        private void ShowDriveModeIcon(long DrMode)
        {
            // drive mode names
            #region
            //DriveModeName[0] = "Single shooting";
            //DriveModeName[1] = "Continuous Shooting";
            //DriveModeName[2] = "Video";
            //DriveModeName[3] = "---";
            //DriveModeName[4] = "High speed continuous";
            //DriveModeName[5] = "Low speed continuous";
            //DriveModeName[6] = "Single Silent shooting";
            //DriveModeName[7] = "Self-timer:Continuous";
            //DriveModeName[8] = "---";
            //DriveModeName[9] = "---";
            //DriveModeName[10] = "Self-timer:10 sec";
            //DriveModeName[11] = "Self-timer: 2 sec";
            //DriveModeName[12] = "14 fps super high speed";
            //DriveModeName[13] = "Silent single shooting";
            //DriveModeName[14] = "Silent contin shooting";
            //DriveModeName[15] = "Silent HS continuous";
            //DriveModeName[16] = "Silent LS continuous";
            #endregion

            if (cameraModel == null) { ReportError("ShowDriveModeIcon: cameraModel==null"); return; }
            string bitmapPath = "";
            switch (DrMode)
            {
                case -1: break;
                case 0: bitmapPath = "../Resources/DriveModeIcons/DrM_Single.png"; break;                 // Single Frame Shooting
                case 1: bitmapPath = "../Resources/DriveModeIcons/DrM_continuous.png"; break;             // Continuous Shooting
                case 2: bitmapPath = "../Resources/DriveModeIcons/DrM_Video.png"; break;                  // Video
                case 3: bitmapPath = "  "; break;                                                         //  not specified
                case 4: bitmapPath = "../Resources/DriveModeIcons/DrM_continous_HighSpeed.png"; break;    // High Speed Continuous Shooting
                case 5: bitmapPath = "../Resources/DriveModeIcons/DrM_continous_LowSpeed.png"; break;     // Low Speed Continuous Shooting
                case 6: bitmapPath = "../Resources/DriveModeIcons/DrM_Single.png"; break;                 // Silent Single Shooting
                case 7: bitmapPath = "../Resources/DriveModeIcons/DrM_Timer_continuous.png"; break;       // Self-timer:Continuous
                case 8: bitmapPath = "  "; break;                                                         //  not specified
                case 16: bitmapPath = "../Resources/DriveModeIcons/DrM_Timer_10.png"; break;               // Self-timer: 10 sec
                case 17: bitmapPath = "../Resources/DriveModeIcons/DrM_Timer_2.png"; break;                // Self-timer: 2 sec
                case 18: bitmapPath = "../Resources/DriveModeIcons/DrM_continous_HighSpeed.png"; break;    // 14 fps super high speed
                case 19: bitmapPath = "../Resources/DriveModeIcons/DrM_silent_Single.png"; break;          // Silent single shooting
                case 20: bitmapPath = "../Resources/DriveModeIcons/DrM_silent_continuous.png"; break;      // Silent contin shooting
                case 21: bitmapPath = "../Resources/DriveModeIcons/DrM_continous_HighSpeed.png"; break;    // Silent HS continuous
                case 22: bitmapPath = "../Resources/DriveModeIcons/DrM_continous_LowSpeed.png"; break;     // Silent LS continuous 
                default: bitmapPath = "../Resources/dummy.png"; break;
            }
            cameraModel.DrModeIcon = bitmapPath;    // Set the DriveMode Image source path 
        }
        private void ShowMeterModeIcon()
        {
            if (cameraModel == null) { ReportError("ShowMeterModeIcon: cameraModel==null"); return; }

            int MMode = (int)cameraModel.MMValue;
            string bitmapPath = MMode switch
            {
                -1 => "",
                1 => "../Resources/MeteringModeIcons/MM_Spot.png",
                3 => "../Resources/MeteringModeIcons/MM_Evaluative.png",
                4 => "../Resources/MeteringModeIcons/MM_Partial.png",
                5 => "../Resources/MeteringModeIcons/MM_CenterW.png",
                _ => "../Resources/dummy.png",
            };
            cameraModel.MModeIcon = bitmapPath;
        }
        private void ShowAFMText(uint AFMode)
        {
            if (cameraModel == null) { ReportError("ShowAFMText: cameraModel==null"); return; }
            ;

            // string[] hexAfmValues = ["0", "1", "2", "3", "ff"];
            string[] AfmValues = ["OneShot", "AIServo", "AIFocus", "AF Off", "---"];

            if (AFMode == 0) cameraModel.AFModeStr = AfmValues[0];
            if (AFMode == 1) cameraModel.AFModeStr = AfmValues[1];
            if (AFMode == 2) cameraModel.AFModeStr = AfmValues[2];
            if (AFMode == 3) cameraModel.AFModeStr = AfmValues[3];
        }
        private void Show_AFM_button()
        {
            if (cameraModel == null) { ReportError("Show_AFM_button: cameraModel==null"); return; }
            ;
            if (cameraModel.IsInLiveViewMode) return;

            if (cameraModel.CameraType == "DSLM")
            {
                //DSLR_AFMCanvas.Visibility = Visibility.Hidden;
                //DSLM_AFMCanvas.Visibility = Visibility.Visible;

                if (cameraModel.LvAFValue == 2) cameraModel.AFModeIcon = "../Resources/AFModeIcons/Gesichtserkennung+Verfolgung.png";
                if (cameraModel.LvAFValue == 10) cameraModel.AFModeIcon = "../Resources/AFModeIcons/Spot AF.png";
                if (cameraModel.LvAFValue == 1) cameraModel.AFModeIcon = "../Resources/AFModeIcons/Einzelfeld AF.png";
                if (cameraModel.LvAFValue == 5) cameraModel.AFModeIcon = "../Resources/AFModeIcons/AF erweitert.png";
                if (cameraModel.LvAFValue == 6) cameraModel.AFModeIcon = "../Resources/AFModeIcons/AF erweiterte Umgebung.png";
                if (cameraModel.LvAFValue == 4) cameraModel.AFModeIcon = "../Resources/AFModeIcons/AF Zone.png";
                if (cameraModel.LvAFValue == 8) cameraModel.AFModeIcon = "../Resources/AFModeIcons/AF große Zone Vertikal.png";
                if (cameraModel.LvAFValue == 7) cameraModel.AFModeIcon = "../Resources/AFModeIcons/AF große Zone horizontal.png";
            }
            else
            {
                //DSLR_AFMCanvas.Visibility = Visibility.Visible;
                //DSLM_AFMCanvas.Visibility = Visibility.Hidden;

                string[] AfmValues = ["OneShot", "AIServo", "AIFocus", "Manual", "---"];
                if (cameraModel.AFMValue == 0) cameraModel.AFModeStr = AfmValues[0];
                if (cameraModel.AFMValue == 1) cameraModel.AFModeStr = AfmValues[1];
                if (cameraModel.AFMValue == 2) cameraModel.AFModeStr = AfmValues[2];
            }
        }
        private void ShowLvAFModeScale()
        {
            if (cameraModel == null) { ReportError("ShowLvAFModeScale: cameraModel==null"); return; }

            // Identify the camera type
            // DSLM   cameras are named:    EOS R ...
            // DSLR   cameras are named:    EOS x ...  (where x is a digit)
            // EOS M  cameras are named:    EOS M ...

            if (cameraModel.CameraType == "DSLM") {
                DSLR_LvAFModeButtonHG.Visibility = Visibility.Hidden;
                DSLR_LvAFModeCanvas.Visibility = Visibility.Hidden;
            }
            else
            {
                string[] EvfAFModes = ["Quick", "Live", "Live"];
                DSLR_LvAFModeButtonHG.Visibility = Visibility.Visible;
                DSLR_LvAFModeCanvas.Visibility = Visibility.Visible;
                cameraModel.EVF_AutoFocusModeStr = EvfAFModes[(int)cameraModel.EVF_AutoFocusMode];
            }
        }

        // Setup the current Exposure Value Compensation (Evc) scale the camera
        private void SetupEvcPointer()
        {
            if (cameraModel == null) { ReportError("SetupEvcPointer: cameraModel==null"); return; }

            // Get the current Evc values from the camera
            int EvcValueListLength = cameraModel.EVCValueListLength;
            int[] EvcValueList = cameraModel.EVCValueList;
            long Evc = cameraModel.EVCValue;
            EvcPreMarker.Text = "         3 . .-2 . .-1 . . 0 . . 1 . . 2 . . 3";

            // Get the current Evc index
            int EvcIndex;
            for (EvcIndex = 0; EvcIndex < EvcValueListLength; EvcIndex++) {
                if (Evc == EvcValueList[EvcIndex]) break;
            }
            // get the PreMarker text length for EVCValueListLength=31
            if (cameraModel.EVCValueListLength == 31) {
                if (EvcIndex <= 6) EvcIndex = 6;
                if (EvcIndex >= 25) EvcIndex = 24;
                EvcPreMarker.Text = EvcPreMarker.Text.Substring(0, (int)EvcIndex * 2 - 3);
            }
            // get the PreMarker text length for EVCValueListLength=19
            if (cameraModel.EVCValueListLength == 19) {
                if (EvcIndex < 0) EvcIndex = 0;
                if (EvcIndex >= 25) EvcIndex = 24;
                EvcIndex += 6;
                EvcPreMarker.Text = EvcPreMarker.Text.Substring(0, (int)EvcIndex * 2 - 3);
            }
        }

        // Show/hide/reset the buttons for normal, LiveView and Stacking mode
        private void Initialize_DSLR_DSLM_Controls()
        {
            if (cameraModel == null) { ReportError("Initialize_DSLR_DSLM_Controls: cameraModel==null"); return; }

            // Setup the AF and Stacking controls:  called from InitializeControls()
                 
            // Initialize the Visibility of the buttons for DSLR AF modes, DSLM AF modes and Stacking
            if (cameraModel.CameraType == "DSLR") {
                DSLR_AFMCanvas.Visibility = Visibility.Visible;
                DSLR_AFMArrow.Visibility = Visibility.Visible;
                DSLR_AFMButton.Visibility = Visibility.Visible;
                DSLR_AFMLabel.Visibility = Visibility.Visible;
                DSLR_AFMStr.Visibility = Visibility.Visible;

                DSLR_LvAFModeCanvas.Visibility = Visibility.Visible;
                DSLR_LvAFModeButtonHG.Visibility = Visibility.Visible;
                DSLR_LvAFModeArrow.Visibility = Visibility.Visible;
                DSLR_LvAFLabel.Visibility = Visibility.Visible;
                DSLR_LvAFStr.Visibility = Visibility.Visible;

                DSLM_AFMCanvas.Visibility = Visibility.Hidden;
                DSLM_AFMButton.Visibility = Visibility.Hidden;
                DSLM_AFMArrow.Visibility = Visibility.Hidden;
                DSLM_AFMIcon.Visibility = Visibility.Hidden;

                DSLM_FStackingCanvas.Visibility = Visibility.Hidden;
                DSLM_FStackingButtonHG.Visibility = Visibility.Hidden;
                DSLM_FStackingLabel.Visibility = Visibility.Hidden;
                DSLM_FStackingIcon.Visibility = Visibility.Hidden;
            }
            // Hide/Show the "LvAFMode-" resp. DSLM_FStacking button
            else if (cameraModel.CameraType == "DSLM") {
                DSLR_AFMCanvas.Visibility = Visibility.Hidden;
                DSLR_AFMArrow.Visibility = Visibility.Hidden;
                DSLR_AFMButton.Visibility = Visibility.Hidden;
                DSLR_AFMLabel.Visibility = Visibility.Hidden;
                DSLR_AFMStr.Visibility = Visibility.Hidden;

                DSLR_LvAFModeCanvas.Visibility = Visibility.Hidden;
                DSLR_LvAFModeButtonHG.Visibility = Visibility.Hidden;
                DSLR_LvAFModeArrow.Visibility = Visibility.Hidden;
                DSLR_LvAFLabel.Visibility = Visibility.Hidden;
                DSLR_LvAFStr.Visibility = Visibility.Hidden;

                DSLM_AFMCanvas.Visibility = Visibility.Visible;
                DSLM_AFMButton.Visibility = Visibility.Visible;
                DSLM_AFMArrow.Visibility = Visibility.Visible;
                DSLM_AFMIcon.Visibility = Visibility.Visible;

                DSLM_FStackingCanvas.Visibility = Visibility.Visible;
                DSLM_FStackingButtonHG.Visibility = Visibility.Visible;
                DSLM_FStackingLabel.Visibility = Visibility.Visible;
                DSLM_FStackingIcon.Visibility = Visibility.Visible;
            }
        }
        private void Reset_DSLR_DSLM_Controls()
        {
            // Setup the controls for LiveView mode OFF: Called from Reset_DSLR_DSLM_Controls()
            if (cameraModel == null) { ReportError("Reset_DSLR_DSLM_Controls: cameraModel==null"); return; }

            MeterModeCanvas.Opacity = 1.0;
            DSLR_AFMCanvas.Opacity = 1.0;
            DSLR_LvAFModeCanvas.Opacity = 1.0;

            if (cameraModel.CameraType == "DSLR") {
                MeterModeCanvas.Visibility = Visibility.Visible;
                DSLR_AFMCanvas.Visibility = Visibility.Visible;
                DSLR_LvAFModeCanvas.Visibility = Visibility.Visible;
                DSLM_FStackingCanvas.Visibility = Visibility.Hidden;
                DSLM_AFMCanvas.Visibility = Visibility.Hidden;
                MeterModeCanvas.Opacity = 1.0;
                DSLR_AFMCanvas.Opacity = 1.0;
                DSLR_LvAFModeCanvas.Opacity = 1.0;
            }
            else if (cameraModel.CameraType == "DSLM")
            {

                MeterModeCanvas.Visibility = Visibility.Visible;
                DSLR_AFMCanvas.Visibility = Visibility.Hidden;
                DSLR_LvAFModeCanvas.Visibility = Visibility.Hidden;
                DSLM_FStackingCanvas.Visibility = Visibility.Visible;
                DSLM_AFMCanvas.Visibility = Visibility.Visible;
                MeterModeCanvas.Opacity = 1.0;
                DSLM_FStackingCanvas.Opacity = 1.0;
                DSLM_AFMCanvas.Opacity = 1.0;
            }
        }
        private void Initialize_Controls_for_LiveView()
        {
            // Setup the controls for the LiveView mode: Called from Initialize_LiveView_Controls()
            if (cameraModel == null) { ReportError("Initialize_Controls_for_LiveView: cameraModel==null"); return; }

            // Meter mode (only for DSLR) and AF modes cannot be changed while the camera is in LiveVview mode:
            if (cameraModel.CameraType == "DSLR") {
                MeterModeCanvas.Opacity = 0.5;
                DSLR_AFMCanvas.Opacity = 0.5;
                DSLR_LvAFModeCanvas.Opacity = 0.5;
                DSLM_AFMCanvas.Visibility = Visibility.Hidden;
                DSLM_FStackingCanvas.Visibility = Visibility.Hidden;
            }
            else if (cameraModel.CameraType == "DSLM") {
                DSLR_AFMCanvas.Visibility = Visibility.Hidden;
                DSLR_LvAFModeCanvas.Visibility = Visibility.Hidden;
                DSLM_AFMCanvas.Opacity = 0.5;
                DSLM_FStackingCanvas.Opacity = 0.5;

            }
            // Hide the AF Button if AF mode = manual (only relevant for DLSM cameras)
            if (cameraModel.AFMValue == 3) { LiveViewAFButton.Visibility = Visibility.Hidden; }
            else { LiveViewAFButton.Visibility = Visibility.Visible; }

            // Hide the controls for the stacking mode
            DriveLeftButtonCanvas.Visibility = Visibility.Hidden;
            DriveRightButtonCanvas.Visibility = Visibility.Hidden;
            WidthOfFocusStepsCanvas.Visibility = Visibility.Hidden;
            StartStackingCanvas.Visibility = Visibility.Hidden;
            FocusStepsCanvas.Visibility = Visibility.Hidden;
        }
        private void Initialize_Controls_for_Stacking()
        {
            // Hide the  buttons for DSLR AF modes, reduce Opacity for DSLM AF modes and Stacking
            // Hide DSLR AF control on the LiveView menu
            // Show the stacking controls on the LiveView menu

            DSLR_AFMCanvas.Visibility = Visibility.Hidden;
            DSLR_LvAFModeCanvas.Visibility = Visibility.Hidden;
            DSLM_AFMCanvas.Opacity = 0.5;
            DSLM_FStackingCanvas.Opacity = 0.5;
            LiveViewAFButton.Visibility = Visibility.Hidden;

            // Show the Stacking mode controls
            DriveLeftButtonCanvas.Visibility = Visibility.Visible;
            DriveRightButtonCanvas.Visibility = Visibility.Visible;
            WidthOfFocusStepsCanvas.Visibility = Visibility.Visible;
            StartStackingCanvas.Visibility = Visibility.Visible;
            FocusStepsCanvas.Visibility = Visibility.Visible;
        }
    }
}
