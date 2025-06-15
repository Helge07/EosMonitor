
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
   // Class cmdGetPropertysStringData: Get property string data - command
   public class cmdGetPropertysStringData : Command
   {
      public uint   propertyId;         // Property identifier
      public string StringData;         // Property data size

      // Constructor: initializes the command action
      public cmdGetPropertysStringData(uint _propertyId, AutoResetEvent _sync) 
      {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId = _propertyId;
         StringData = "";

         // set parameters of the base class "Command"
         cmdName     = "SetPropertyStringData";
         retry       = false;
         syncEvent   = _sync;
         base.action = () => { _SetPropertyStringData(); };
      }

      // _SetPropertyStringData:  "Set Property String Data" action to be executed by the command processor
      private void _SetPropertyStringData() 
      {
         if (MainWindow.cameraModel == null) return;

         // For cameras earlier than the 30D , the UI must be locked before commands are reissued
         if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
            MainWindow.cameraModel.LockAndExecute(action);
            return;
         }

         // Ensure that camera session is opened
         MainWindow.cameraModel.OpenSession();

         // get Descriptor from SDK
         try {
            uint error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, propertyId, 0, out StringData);
            checkResult(error, "Set Property Integer Data of PropertyID : ");
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }
   }
}