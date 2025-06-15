
using EDSDKLib;
using System.Threading;

namespace EosMonitor
{
    // Class cmdGetPropertyDescriptor: Get Property Descriptor - command
    public class cmdGetPropertyDescriptor : Command
    {
      public uint                   propertyId;    // Property identifier
      public EDSDK.EdsPropertyDesc  descriptor;    // Property descriptor

      // Constructor: initializes the command action
      public cmdGetPropertyDescriptor(uint _propertyId, AutoResetEvent _sync) 
      {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId  = _propertyId;

         // set parameters of the base class "Command"
         cmdName   = "GetPropertyDescriptor";
         retry     = false;
         syncEvent = _sync;
         action    = () => { _GetPropertyDescriptor(); };
      }

      // _GetPropertyDescriptor:  "Get Property Descriptor" action to be executed by the command processor
      private void _GetPropertyDescriptor() 
      {
             if (MainWindow.cameraModel == null) return;

            // Ensure that camera session is open
            if (!MainWindow.cameraModel.IsSessionOpen) return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && ! MainWindow.cameraModel.IsLocked) {
                 MainWindow.cameraModel.LockAndExecute(action);
                 return;
             }

            // get Descriptor from SDK
            try {
                uint error = EDSDK.EdsGetPropertyDesc(MainWindow.cameraPtr, propertyId, out descriptor);
                checkResult(error, "Get Property Descriptor of PropertyID : ");
            }
            catch (EosException ex) {
                EosExceptionMessage(ex);
            }
            return;
      }
    }
}