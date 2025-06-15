
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
    // Class cmdReleaseShButton: Release Shutter button 
    public class cmdReleaseShButton : Command
    {
       public ShutterButtonState ShButtonState;        // Shutter Button State

       // Constructor: initializes the command action
       public cmdReleaseShButton(AutoResetEvent _sync) 
       {
           // set parameters of the base class "Command"
           cmdName = "ReleaseShButton";
           retry = false;
           syncEvent = _sync;
           action = () => { _ReleaseShButton(); };
       }

       // _ReleaseShButton:  Release Shutter button command to be executed by the command processor
       private void _ReleaseShButton() 
       {
            if (MainWindow.cameraModel == null) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                 MainWindow.cameraModel.LockAndExecute(_ReleaseShButton);
                 return;
            }

            // Ensure that camera session is opened
            MainWindow.cameraModel.OpenSession();

           // Sendcommand "Press shutter button halfway" to the camera
           try {
                uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_PressShutterButton, (int)EDSDK.EdsShutterButton.CameraCommand_ShutterButton_OFF);
                checkResult(error, "Press shutter button halfway command");
                if (ShButtonState != ShutterButtonState.Off) ShButtonState = ShutterButtonState.Off;
           }
           catch (EosException ex) {
                EosExceptionMessage(ex);
           }
       }
    }
}

