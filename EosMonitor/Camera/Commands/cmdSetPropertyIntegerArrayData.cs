
using EDSDKLib;
using System.Runtime.InteropServices;
using System.Threading;

namespace EosMonitor
{
    // Class SetPropertyIntegerArrayData: SGet property integer array data - command
    public class cmdSetPropertyIntegerArrayData : Command
    {
      public uint   propertyId;      // Property identifier
      public uint[] IntArrayData;    // Property data size

      // Constructor: initializes the command action
      public cmdSetPropertyIntegerArrayData(uint _propertyId, uint[] _IntArrayData, AutoResetEvent _sync) 
      {
            // set the class members of the class "cmdGetPropertyDescriptor"
            propertyId   = _propertyId;
            IntArrayData = _IntArrayData;

            // set parameters of the base class "Command"
            cmdName      = "SetPropertyIntegerArrayData";
            retry        = false;
            syncEvent    = _sync;
            base.action  = () => { _SetPropertyIntegerArrayData(); };
      }

      // _SetPropertyIntegerArrayData:  "Set Property Integer Data" action to be executed by the command processor
      private void _SetPropertyIntegerArrayData() 
      {
            if (MainWindow.cameraModel == null)   return;

            // For cameras earlier than the 30D , the UI must be locked before commands are reissued
            if (MainWindow.cameraModel.IsLegacy && !MainWindow.cameraModel.IsLocked) {
                MainWindow.cameraModel.LockAndExecute(action);
                return;
            }

         // Ensure that camera session is opened
         MainWindow.cameraModel.OpenSession();

         // get Descriptor from SDK
         try {
            uint error = EDSDK.EdsSetPropertyData(MainWindow.cameraPtr, propertyId, 0, Marshal.SizeOf(typeof(uint)) * IntArrayData.Length, IntArrayData);
            checkResult(error, "Set Property Integer Array Data of PropertyID : ");
         }
         catch (EosException ex) {
            EosExceptionMessage(ex);
         }
         return;
      }
   }
}