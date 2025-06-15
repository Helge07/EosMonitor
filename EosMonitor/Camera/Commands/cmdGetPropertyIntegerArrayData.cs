
using EDSDKLib;
using System;
using System.Threading;

namespace EosMonitor
{
    // Class cmdGetPropertyIntegerArrayData: Get property integer array data - command
    public class cmdGetPropertyIntegerArrayData : Command
    {
      public uint   propertyId;      // Property identifier
      public int    dataSize;        // Property data size
      public IntPtr ptr;             // Pointer to the data area

      // Constructor: initializes the command action
      public cmdGetPropertyIntegerArrayData(uint _propertyId, int _dataSize, IntPtr _ptr, AutoResetEvent _sync) 
      {
         // set the class members of the class "cmdGetPropertyDescriptor"
         propertyId = _propertyId;
         dataSize   = _dataSize;
         ptr        = _ptr;

         // set parameters of the base class "Command"
         cmdName     = "GetPropertyIntegerArrayData";
         retry       = false;
         syncEvent   = _sync;
         base.action = () => { _GetPropertyIntegerArrayData(); };
      }

      // _GetPropertyIntegerArrayData:  " Get property integer array data" - action to be executed by the command processor
      private void _GetPropertyIntegerArrayData() 
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
           uint error = EDSDK.EdsGetPropertyData(MainWindow.cameraPtr, propertyId, 0, dataSize, ptr);
           checkResult(error, "Get Property Integer Array Data of PropertyID : ");
        }
        catch (EosException ex) {
           EosExceptionMessage(ex);
        }
        return;
      }
   }
}