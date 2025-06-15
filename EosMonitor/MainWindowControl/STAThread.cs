using System;
using System.Threading;

namespace EosMonitor
{
    public static class STAThread
    {
        public static readonly object ExecLock = new();     // used to lock a thread
        public static bool IsSTAThread                      // states if the calling thread is an STA thread or not
        {
            get { return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA; }
        }
        private static Thread? main;                        // The main thread where everything will be executed on
        private static Action? runAction;                   // The action to be executed
        private static Exception? runException;             // Storage for an exception that might have happened on the execution thread

        private static bool isRunning = false;              // States if the execution thread is currently running                                                     
        private static object runLock = new();             // Lock object to make sure only one command at a time is executed
        private static object threadLock = new();          // Lock object to synchronize between execution and calling thread

        // Start the execution thread
        internal static void Init()
        {
            if (!isRunning)
            {
                main = Create(SafeExecutionLoop);
                isRunning = true;
                main.Start();
            }
        }

        // Shut down the execution thread
        internal static void Shutdown()
        {
            if (isRunning)
            {
                isRunning = false;
                lock (threadLock) { Monitor.Pulse(threadLock); }
                main?.Join();
            }
        }

        // Create an STA thread that can safely execute SDK commands
        public static Thread Create(Action a)
        {
            var thread = new Thread(new ThreadStart(a));
            thread.SetApartmentState(ApartmentState.STA);
            return thread;
        }

        // Safely execute an SDK command
        public static void ExecuteSafely(Action a)
        {
            lock (runLock)
            {
                if (!isRunning) return;

                if (IsSTAThread)
                {
                    runAction = a;
                    lock (threadLock)
                    {
                        Monitor.Pulse(threadLock);
                        Monitor.Wait(threadLock);
                    }
                    if (runException != null) throw runException;
                }
                else lock (ExecLock)
                    {
                        a();
                    }
            }
        }

        // Safely execute an EDSDK command a with return value
        public static T ExecuteSafely<T>(Func<T> func)
        {
            T? result = default;
            ExecuteSafely(delegate { result = func(); });
            return result!;
        }

        // synchronize thread execution by a Wait/Pulse - lock
        private static void SafeExecutionLoop()
        {
            lock (threadLock)
            {
                while (true)
                {
                    Monitor.Wait(threadLock);
                    if (!isRunning) return;
                    runException = null;
                    try { lock (ExecLock) { runAction?.Invoke(); } }
                    catch (Exception ex) { runException = ex; }
                    Monitor.Pulse(threadLock);
                }
            }
        }
    }
}
