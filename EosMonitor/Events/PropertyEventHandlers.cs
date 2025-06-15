
using EDSDKLib;
using System;

// Implemented property change event handler:
// PropID_Evf_OutputDevice
// PropID_AEMode
// PropID_ISOSpeed
// PropID_Av
// PropID_Tv
// PropID_BatteryQuality
// PropID_DriveMode
// PropID_AFMode
// PropID_ExposureCompensation
// PropID_MeteringMode
//
// No property descriptor change event handler are implemented

namespace EosMonitor
{
    public partial class CameraModel : Object
    {
        // define handler for OnPropertyChanged and PropertyDescChanged events
        private uint HandlePropertyEvent(uint propertyEvent, uint propertyId, uint param, IntPtr context) {

            if (MainWindow._lockPropChanged == true) return EDSDK.EDS_ERR_OK;

            switch (propertyEvent) {
                case EDSDK.PropertyEvent_PropertyChanged:
                    HandlePropertyEvent_EventPropertyChanged(propertyId, param, context);
                    break;
                case EDSDK.PropertyEvent_PropertyDescChanged:
                     // No actions implemented
                     HandlePropertyEvent_PropertyDescChanged(propertyId, param, context);
                     break;
            }
            return EDSDK.EDS_ERR_OK;
        }

        // Handle for "OnPropertyChanged" events
        private void HandlePropertyEvent_EventPropertyChanged(uint propertyId, uint param, IntPtr context) {
           
            if (MainWindow._lockPropChanged == true) return;

            switch (propertyId) {
                case EDSDK.PropID_Evf_OutputDevice:       
                       EvfOutputDeviceChanged?.Invoke(this, new (EDSDK.PropID_Evf_OutputDevice));
                    break;

                case EDSDK.PropID_AEMode:                // activated by AE Mode changes on the camera
                    AEModeChanged?.Invoke(this, new AEModeChangedEventArgs(EDSDK.PropID_AEMode));
                    break;

                case EDSDK.PropID_ISOSpeed:              // activated by Iso speed changes on the camera
                    IsoSpeedChanged?.Invoke(this, new EventArgs());
                    break;

                case EDSDK.PropID_Av:                    // Av value changes on the camera
                    AvValueChanged?.Invoke(this, new EventArgs());
                    break;

                 case EDSDK.PropID_Tv:                   // Tv value changes on the camera
                      if (TvValueChanged != null) TvValueChanged(this, new EventArgs());
                      break;

                 case EDSDK.PropID_BatteryQuality:       // activated battery load changes on the camera
                      BatteryLevelChanged?.Invoke(this, new EventArgs());
                      break;

                 case EDSDK.PropID_DriveMode:            // activated by DriveMode changes on the camera
                      DriveModeChanged?.Invoke(this, new EventArgs());
                      break;

                 case EDSDK.PropID_AFMode:               // activated by Autofocus mode changes on the camera
                      AFModeChanged?.Invoke(this, new EventArgs());
                      break;
    
                 case EDSDK.PropID_ExposureCompensation:         // activated by exposure compensation changes on the camera
                      EvcChanged?.Invoke(this, new EventArgs());
                      break;
                            
                 case EDSDK.PropID_MeteringMode:         // activated by metering mode changes on the camera
                      MeterModeChanged?.Invoke(this, new EventArgs());
                      break;           
            }
        }

        // Dummy handler for PropertyDescChanged events 
        private void HandlePropertyEvent_PropertyDescChanged(uint propertyId, uint param, IntPtr context) 
        {
           // not implemented
        }
    }
}
