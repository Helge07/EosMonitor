
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
   // Class cmdBulbStart: BulbStart - command
   // Opens the shutter. It must be closed by a second click on the Take Picture button  
   public class cmdBulbStart : Command
   {
      // Constructor: initializes the command action
      public cmdBulbStart(AutoResetEvent _sync) 
      {
         // set parameters of the base class "Command"
         cmdName = "Bulb";
         retry = false;
         syncEvent = _sync;
         base.action = () => { _BulbStart(); };
      }

      // _BulbStart:  "BulbStart" action to be executed by the command processor
      private void _BulbStart() 
      {
         // Ensure that camera session is opened
         MainWindow.cameraModel?.OpenSession();
         try {
            // Open the shutter
            uint error = BulbStart();
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }

      public uint BulbStart() 
      {
         uint error;
         bool locked = false;

         error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UILock, 0);
         if (error == EDSDK.EDS_ERR_OK)
            locked = true;

         // Normal BulbStart command
         if (error == EDSDK.EDS_ERR_OK)
            error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_BulbStart, 0);

         // Undocumented BulbStart command at least for:  Rebel Ti1= Eos 500D, Rebel Ti2=Eos 550D und Eos 60D
         if (error == EDSDK.EDS_ERR_INVALID_PARAMETER)
            error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, 0x00000004, 65539);

         // If anything not OK, unlock camera UI
         if (error != EDSDK.EDS_ERR_OK && locked)
            error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UIUnLock, 1);

        // The next click will close the shutter if it occurs before BulbTimer expiration
        if (MainWindow.cameraModel != null) {
            MainWindow.cameraModel.ShutterIsClosed = false;
        }

         return error;
      }
   }
}