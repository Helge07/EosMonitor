
using EDSDKLib;
using System.Threading;


namespace EosMonitor
{
   // Class cmdEvfDriveLens: Drives the lens to adjust focus
   public class cmdEvfDriveLens : Command
   {
      // Drive Lens command code:
      // Allowed values are of enum Type "EvfDriveLens"
      public uint DriveLensCode;      

      // Constructor: initializes the command action
      public cmdEvfDriveLens(EvfDriveLens _DriveLensCode, AutoResetEvent _sync) 
      {
         DriveLensCode = (uint)_DriveLensCode;

         // set parameters of the base class "Command"
         cmdName = "EvfDriveLens";
         retry = true;
         syncEvent = _sync;
         action = () => { _EvfDriveLens(); };
      }

      // _EvfDriveLens:  Drives Lens action to be executed by the command processor
      private void _EvfDriveLens() 
      {

         if (MainWindow.cameraModel == null) return;

         // For cameras earlier than the 30D , the UI must be locked before commands are reissued
         if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
            MainWindow.cameraModel.LockAndExecute(_EvfDriveLens);
            return;
         }

         // Ensure that camera session is opened
         MainWindow.cameraModel.OpenSession();

         // Send DriveLens command to the camera
         try {
            uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_DriveLensEvf, (int)DriveLensCode);
            checkResult(error, "EvfDriveLens command");
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
      }
   }
}

