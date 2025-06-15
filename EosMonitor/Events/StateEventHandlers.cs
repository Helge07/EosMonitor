
using EDSDKLib;
using System;
using System.Windows;


namespace EosMonitor
{
    // State Events:
    // ------------------------------------------------------------
    // StateEvent_Shutdown:             -->     Shutdown(....)
    // StateEvent_WillSoonShutDown:     ---
    // StateEvent_InternalError:        ---
    // StateEvent_CaptureError:         ---
    // StateEvent_JobStatusChanged:     ---
    // StateEvent_All:                  ---

    public partial class CameraModel : Object
    {
        private uint HandleStateEvent(uint stateEvent, uint param, IntPtr context)
        {
            switch (stateEvent) {
                case EDSDK.StateEvent_Shutdown:
                    //OnStateEventShutdown(EventArgs.Empty);  //   causes error message "Unexpected parameters" 
                    //Shutdown(this, EventArgs.Empty);        //  If the USB connection is removed
                    break;

                case EDSDK.StateEvent_WillSoonShutDown:
                    break;

                case EDSDK.StateEvent_InternalError:
                    break;

                case EDSDK.StateEvent_CaptureError:
                    break;

                case EDSDK.StateEvent_JobStatusChanged:
                    break;

                case EDSDK.StateEvent_All:
                    break;

                default:
                    break;
            }
            return EDSDK.EDS_ERR_OK;
        }

        private void OnStateEventShutdown(EventArgs eventArgs)
        {
            if (Shutdown != null)
                Shutdown.BeginInvoke(this, eventArgs, null, null);
            try
            {
                uint error = EDSDK.EdsCloseSession(MainWindow.cameraPtr);
            }
            catch (Exception) {  System.Windows.MessageBox.Show("Error in EDSDK.EdsCloseSession during camera shutdown", "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void StateEvent_WillSoonShutDown(EventArgs eventArgs)
        {
            // not implemented
        }
        private void StateEvent_InternalError(EventArgs eventArgs)
        {
            // not implemented
        }
        private void StateEvent_CaptureError(EventArgs eventArgs)
        {
            // not implemented
        }
        private void StateEvent_JobStatusChanged(EventArgs eventArgs)
        {
            // not implemented
        }
        private void StateEvent_All(EventArgs eventArgs)
        {
            // not implemented
        }
    }
}

