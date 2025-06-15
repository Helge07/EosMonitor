using System;
using System.Threading;
using System.Windows;

namespace EosMonitor
{
    // Class Command: Base class of camera commands
    public class Command
    {
        public string cmdName;          // command name as string
        public bool retry;              // retry flag: set by the CmdProcessor
        public AutoResetEvent syncEvent;       // WaitOne Handle for synchronization with the caller
        public Action action;           // command action 

        // Konstructoren
        public Command()
        {
            cmdName = "";
            syncEvent = new AutoResetEvent(false); // Initialize syncEvent to avoid null
            action = () => { };
        }

        // Constructor: Command: Initializes the command parameters of the base class
        public Command(string _name, bool _retry, AutoResetEvent _complete, Action _action)
        {
            cmdName = _name;
            retry = _retry;
            syncEvent = _complete;
            action = _action;
        }


        // checkResult:  checks the error code after command execution
        public void checkResult(uint error, string errorMessage)
        {
            // retry=true if the camera is busy
            // retry=false in all other cases (including successful execution
            switch (error) {
                case (uint)ErrorCode.Ok:
                    retry = false;
                    break;
                case (uint)ErrorCode.DeviceBusy:
                    if (MainWindow.cameraModel != null)
                    retry = false;  
                    break;
                // PropertiesUnavailable may occur during a GetPropertyDataSize command with propertyId PropID_FocusInfo 
                // If the FocusInfo is not yet ready 
                case (uint)ErrorCode.PropertiesUnavailable:
                    retry = false;
                    break;
                // InternalError may occur if the camera is switched off
                case (uint)ErrorCode.InternalError:
                    retry = false;
                    break;
                // CommDisconnected may occur if the camera is switched off
                case (uint)ErrorCode.CommDisconnected:
                    retry = false;
                    break;
                default:
                    retry = false;
                    throw new EosException(error, "Error in :       " + errorMessage);
            }
        }

        // EosExceptionMessage: shows a message box with Exception information
        public void EosExceptionMessage(EosException ex)
        {

            MessageBox.Show(ex.EosErrorMessage + "\nError code :  " + MainWindow.errorList.getErrorItem((uint)ex.EosErrorCode).CodeString,
                                                 "Eos Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}