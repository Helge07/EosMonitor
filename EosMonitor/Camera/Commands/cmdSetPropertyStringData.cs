
using EDSDKLib;
using System.Text;
using System.Threading;

namespace EosMonitor
{
    // Class cmdSetPropertyStringData: SGet property string data - command
    public class cmdSetPropertyStringData : Command
    {
          public uint propertyId;      // Property identifier
          public string StringData;    // String data to be set

          // Constructor: initializes the command action
          public cmdSetPropertyStringData(uint _propertyId, string _StringData, AutoResetEvent _sync) 
          {
                // set the class members of the class "cmdGetPropertyDescriptor"
                propertyId = _propertyId;
                StringData = _StringData;

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
                    var bytes = Encoding.ASCII.GetBytes(StringData + "\0");
                    uint error = EDSDK.EdsSetPropertyData(MainWindow.cameraPtr, propertyId, 0, bytes.Length, bytes);
                    checkResult(error, "Set Property String Data of PropertyID : ");
                }
                catch (EosException ex) {
                    EosExceptionMessage(ex);         }
                return;
          }
    }
}