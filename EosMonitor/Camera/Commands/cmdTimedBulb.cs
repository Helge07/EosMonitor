
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
    // Class cmdTimedBulb: Timer controlled Bulb - command
    // Opens the shutter. It will be closed when a preset Time Interval expires. 
    public class cmdTimedBulb : Command
    {
      // Constructor: initializes the command action
      public cmdTimedBulb(AutoResetEvent _sync) {
         // set parameters of the base class "Command"
         cmdName = "Bulb";
         retry = false;
         syncEvent = _sync;
         base.action = () => { _TimedBulb(); };
      }

      // _TimedBulb:  "Timer controlled Bulb shooting" action to be executed by the command processor
      private void _TimedBulb() {
         // Start Timer controlled Bulb shooting as a seperate thread
         // This separate thread is necessary to avoid blocking the user interface
         var TimedBulbTask = new ThreadPoolWorker();
         TimedBulbTask.Work(() => { TimedBulbWork(); });
      }

      private void TimedBulbWork() 
      {
         if (MainWindow.cameraModel == null) return;

         MainWindow.cameraModel.OpenSession();
         MainWindow.cameraModel.BulbCanceld = false;
         try {
            // Open the shutter
            uint error = BulbStart();

            // Decrement the counter "BTCounter" every second
            // until timer expires or a second click occured
            do {
               Thread.Sleep(1000);
               MainWindow.cameraModel.BTCounter --;
            } while ((MainWindow.cameraModel.BTCounter != 0) 
                      && (MainWindow.cameraModel.BulbCanceld == false));

            // Close the shutter
            if (MainWindow.cameraModel.BulbCanceld == false)
               BulbEnd();

            // Reset BulbTimer
            MainWindow.cameraModel.BTHours = 0;
            MainWindow.cameraModel.BTMinutes = 0;
            MainWindow.cameraModel.BTSeconds = 0;
            MainWindow.cameraModel.IsBulbTimerSet = false;
            MainWindow.cameraModel.ShutterIsClosed = true;
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }

      public uint BulbStart() {

         if (MainWindow.cameraModel == null) return 0;

         uint error;
         bool locked = false;

         error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UILock,0);
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
            error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UIUnLock, 0);

         // ShutterIsClosed:  Prevent "PressHalfway" for the shutter button
         // The next click will close the shutter if it occurs before BulbTimer expiration
         MainWindow.cameraModel.ShutterIsClosed = false;
         return error;
      }

      public uint BulbEnd() {

        if (MainWindow.cameraModel == null) return 0;

        // Normal BulbEnd command
        uint error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, EDSDK.CameraCommand_BulbEnd, 0);

         // Undocumented BulbEnd command at least for:  Rebel Ti1= Eos 500D, Rebel Ti2=Eos 550D und Eos 60D
         if (error == EDSDK.EDS_ERR_INVALID_PARAMETER)
            error = EDSDK.EdsSendCommand(MainWindow.cameraPtr, 0x00000004, 0);

         // now the shutter is closed
         MainWindow.cameraModel.ShutterIsClosed = true;

         // Unlock camera UI
         error = EDSDK.EdsSendStatusCommand(MainWindow.cameraPtr, EDSDK.CameraState_UIUnLock,1);
         return error;
      }
    }
}