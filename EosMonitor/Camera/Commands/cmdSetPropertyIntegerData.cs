
using EDSDKLib;
using System.Runtime.InteropServices;
using System.Threading;


namespace EosMonitor
{
    // Class cmdSetPropertyIntegerData: SGet property integer data - command
    public class cmdSetPropertyIntegerData : Command
   {
      public uint propertyId;      // Property identifier
      public uint IntData;         // Property data size

      // Constructor: initializes the command action
      public cmdSetPropertyIntegerData(uint _propertyId, uint _IntData, AutoResetEvent _sync) {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId = _propertyId;
         IntData    = _IntData;

         // set parameters of the base class "Command"
         cmdName     = "SetPropertyIntegerData";
         retry       = false;
         syncEvent   = _sync;
         base.action = () => { _SetPropertyIntegerData(); };
      }

      // _SetPropertyIntegerData:  "Set Property Integer Data" action to be executed by the command processor
      private void _SetPropertyIntegerData() {

            if (MainWindow.cameraModel == null) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                MainWindow.cameraModel.LockAndExecute(action);
            return;
         }

            // If camera session is opened,get Descriptor from SDK
            if (MainWindow.cameraModel.IsSessionOpen)
                try {
                    uint error = EDSDK.EdsSetPropertyData(MainWindow.cameraPtr, propertyId, 0, Marshal.SizeOf(typeof(uint)), (uint)IntData);
                    checkResult(error, "Set Property Integer Data of PropertyID : ");
                }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }

   }
}