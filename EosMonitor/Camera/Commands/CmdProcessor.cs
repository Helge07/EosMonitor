using System;
using System.Collections.Generic;
using System.Threading;

namespace EosMonitor
{
    public class CmdProcessor
    {
      private bool      _running  = false;
      public  CmdQueue  _cmdQueue = new CmdQueue();
      public  Command   _closeCmd = new Command();
      private Command   _nullCmd  = new Command();

      // CmdProcessor: Constructor: set _running=false, _closeProcessorCmd= empty comand
      public CmdProcessor () { }

      // ~CmdProcessor: Destructor: clear the command queue
      ~CmdProcessor () { clear(); }

      // setCloseCmd: Set command "_closeProcessorCmd" to be executed when ending
      public void setCloseCmd(Command cmd) { _closeCmd = cmd; }

      // enqueueCmd: put a command into the command queue
      public void enqueueCmd(Command command) 
      {
         Monitor.Enter(this);
         _cmdQueue.Enqueue(command);
         Monitor.Exit(this);
      }

      // stopProcessor: stop the CmdProcessor
      public void stopCmd() { _running = false; }

      // clear:  remove all items from the queue
      public void clear() 
      { 
          Monitor.Enter(_cmdQueue); 
          _cmdQueue.Clear(); 
          Monitor.Exit(_cmdQueue); 
      }

      // dequeueCmd: obtain the next command from the queue
      public Command dequeueCmd() 
      {
          Command cmd = _nullCmd;
          Monitor.Enter(_cmdQueue);
          if ((_running) && (_cmdQueue.Count > 0)) {
               cmd = _cmdQueue.Dequeue();
          }
          Monitor.Exit(_cmdQueue);
          return cmd;
      }
      
      // run:  dequeue and execute commands while _running 
      public void run() 
      {
            _running = true;
            // Allow the Main Thread to continue 
            MainWindow._syncCmdProcessor.Set();

            // loop until the command queue is stopped
            while (_running) {
                Thread.Sleep(100);
                if (MainWindow.cameraModel != null) {
                    Command cmd = dequeueCmd();
                    if (cmd.cmdName != "") {
                        cmd.action();
                        if (cmd.retry) {
                            enqueueCmd(cmd);
                        }
                        cmd.syncEvent.Set();
                    }
                }
                else {
                    MainWindow.ReportError("Unvalid command:  ' '");
                    clear();   // we should not arrive here
                }
            }
            // here we are if the queue was stopped.
            clear();
      }  
    }

    // Class CmdQueue:   FiFo-Queue for camera commands
    public class CmdQueue : Queue<Command> 
    {
      Action   action = () => { };
    }

}