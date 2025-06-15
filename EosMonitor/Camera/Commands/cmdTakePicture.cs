
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
   // Class cmdTakePicture: Take Picture Command
   public class cmdTakePicture : Command
   {
      // Constructor: initializes the command action
      public cmdTakePicture(AutoResetEvent _sync) 
      {
         // set parameters of the base class "Command"
         cmdName    = "TakePicture";
         retry      = false;
         syncEvent  = _sync;
         action     = () => { _TakePicture(); };  
      }

      // _TakePicture:  TakePicture action to be executed by the command processor
      private void _TakePicture() 
      {  
            if (MainWindow.cameraModel == null) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                MainWindow.cameraModel.LockAndExecute(_TakePicture);
                return;
            }
            // Ensure that camera session is opened
            MainWindow.cameraModel.OpenSession();
            if (!MainWindow._sessionIsOpen) return;

            // Send take picture command to the camera
            try {
                uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_TakePicture, 1);
                checkResult (error, "Take Picture command");
            }
            catch (EosException ex) {
                EosExceptionMessage(ex);
            }
      }
   }
}

