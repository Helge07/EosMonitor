using EDSDKLib;
using System;
using System.Threading;


namespace EosMonitor
{
    // Class cmdGetPropertyStruct: Get property data of generic type - command
    public class cmdGetPropertyStruct : Command
    {
      public uint     propertyId;      // Property identifier
      public int      dataSize;        // Property data size
      public IntPtr   ptr;             // Pointer to the data area

      // Constructor: initializes the command action
      public cmdGetPropertyStruct(uint _propertyId, int _dataSize, IntPtr _ptr, AutoResetEvent _sync) {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId = _propertyId;
         dataSize   = _dataSize;
         ptr        = _ptr;

         // set parameters of the base class "Command"
         cmdName     = "GetPropertyStruct";
         retry       = false;
         syncEvent   = _sync;
         base.action = () => { _GetPropertyStruct(); };
      }

      // _GetPropertyStruct:  "Get property data of generic type" - action to be executed by the command processor
      private void _GetPropertyStruct() {

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
            uint error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, propertyId, 0, dataSize, ptr);
            checkResult(error, "Get Property Integer Data of PropertyID : ");
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }
   }
}