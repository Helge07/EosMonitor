using EDSDKLib;
using System.Threading;


namespace EosMonitor
{
    // Class cmdPressShButtonHalfway: Press Shutter button halfway for AF an Metering Operations
    public class cmdPressShButtonHalfway : Command
    {
        public ShutterButtonState ShButtonState;        // Shutter Button State

        // Constructor: initializes the command action
        public cmdPressShButtonHalfway(AutoResetEvent _sync)
        {
            // set parameters of the base class "Command"
            cmdName = "PressShButtonHalfway";
            retry = false;
            syncEvent = _sync;
            action = () => { _PressShButtonHalfway(); };
        }

        // _PressShButtonHalfway:  PressShButtonHalfway action to be executed by the command processor
        private void _PressShButtonHalfway()
        {
            if (MainWindow.cameraModel == null) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                MainWindow.cameraModel.LockAndExecute(_PressShButtonHalfway);
                return;
            }

            // Ensure that camera session is opened
            MainWindow.cameraModel.OpenSession();

            // Sendcommand "Press shutter button halfway" to the camera
            try {
                uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_PressShutterButton, (int)EDSDK.EdsShutterButton.CameraCommand_ShutterButton_Halfway);
                checkResult(error, "Press shutter button halfway command");
                if (ShButtonState != ShutterButtonState.Halfway) ShButtonState = ShutterButtonState.Halfway;
            }
            catch (EosException ex) {
                EosExceptionMessage(ex);
            }
        }
    }
}

