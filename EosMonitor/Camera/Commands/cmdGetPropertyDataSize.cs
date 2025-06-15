
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
   // Class cmdGetPropertyDataSize: Get Property Data Size - command
   public class cmdGetPropertyDataSize : Command
   {
      public uint propertyId;     // Property identifier
      public int  dataSize;       // Property data size

      // Constructor: initializes the command datatype
      public cmdGetPropertyDataSize(uint _propertyId, AutoResetEvent _sync) {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId = _propertyId;
         dataSize   = 0;

         // set parameters of the base class "Command"
         cmdName   = "GetPropertyDataSize";
         retry     = false;
         syncEvent = _sync;
         action    = () => { _GetPropertyDataSize(); };
      }

      // _GetPropertyDataSize:  "Get Property data size" action to be executed by the command processor
      private void _GetPropertyDataSize() 
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
           EDSDK.EdsDataType dataType;
           uint error = EDSDK.EdsGetPropertySize(MainWindow.cameraPtr, propertyId, 0, out dataType, out dataSize);
           checkResult(error, "Get Property Data Size of PropertyID : ");
           // PropertiesUnavailable may occur during a GetPropertyDataSize command with propertyId PropID_FocusInfo 
           // If the FocusInfo is not yet ready. In this case checkResult produces no error message 
        }
        catch (EosException ex) {
           EosExceptionMessage(ex);     
        }
        return;
      }
   }
}