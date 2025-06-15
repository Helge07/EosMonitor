
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
    // Class cmdDoEvfAF: Controls auto focus in live view modes
    public class cmdDoEvfAF : Command
    {
      // Drive Lens command code:   On=1, =ff=0
      public uint EvfAf_OnOff; 

      // Constructor: initializes the command action
      public cmdDoEvfAF(uint _EvfAf_OnOff, AutoResetEvent _sync) 
      {

         EvfAf_OnOff = _EvfAf_OnOff;

         // set parameters of the base class "Command"
         cmdName = "DoEvfAF";
         retry = false;
         syncEvent = _sync;
         action = () => { _DoEvfAF(); };
      }

      // _DoEvfAF:  action to be executed by the command processor
      private void _DoEvfAF()
      {
         if (MainWindow.cameraModel == null) return;

         // For cameras earlier than the 30D , the UI must be locked before commands are reissued
         if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
            MainWindow.cameraModel.LockAndExecute(_DoEvfAF);
            return;
         }

         // Ensure that camera session is opened
         MainWindow.cameraModel.OpenSession();

         // Send Live View Autofocus command to the camera
         try {
            uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_DoEvfAf, (int)EvfAf_OnOff);
            checkResult(error, "DoEvfAF command");
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
      }
   }
}
