
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
   // Class GetPropertyIntegerData: Get property integer data - command
   public class cmdGetPropertyIntegerData : Command
   {
      public uint propertyId;      // Property identifier
      public long IntData;         // Property data size

      // Constructor: initializes the command action
      public cmdGetPropertyIntegerData(uint _propertyId, AutoResetEvent _sync) 
      {
             // set the class members of the class "cmdGetPropertyDescriptor"
             propertyId = _propertyId;
             IntData    = 0;

             // set parameters of the base class "Command"
             cmdName     = "GetPropertyIntegerData";
             retry       = false;
             syncEvent   = _sync;
             base.action = () => { _GetPropertyIntegerData(); };
      }

        // _GetPropertyIntegerData:  "Get Property Integer Data" action to be executed by the command processor
        private void _GetPropertyIntegerData()
        {
            if (MainWindow.cameraModel == null) return;

            // Ensure that camera session is open
            if (!MainWindow.cameraModel.IsSessionOpen) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                MainWindow.cameraModel.LockAndExecute(action);
                return;
            }
            // get Descriptor from SDK
            try
            {
                uint _data;
                uint error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, propertyId, 0, out _data);
                IntData = _data;
                checkResult(error, "Get Property Integer Data of PropertyID : ");
            }                
            catch (EosException ex) {
            EosExceptionMessage(ex);
        }
        return;
      }
   }
}