
using System;
using System.Threading;

namespace EosMonitor
{
    public class ThreadPoolWorker
    {
        // Work: pass an Action "work" to the threadpool for asynchronous execution
        public void Work(Action work) 
        {
            // setup in state the data for the downloadTask to be passed to the thread pool
            var state = new State<bool>(false) { Callback = () => { work(); return true; } };

            // pass the downloadTask to the thread pool
            if (!ThreadPool.QueueUserWorkItem(PerformUserWork<bool>!, state))
                throw new ApplicationException("Unable to queue user work item to the thread pool.");
        }

        // WorkAndWait: pass an Action "work" to the threadpool for asynchronous execution with a execution time limit
        public TResult WorkAndWait<TResult>(Func<TResult> work, int millisecondsWaitTimeout = Timeout.Infinite) 
        {
            var state = new State<TResult> { Callback = work };
            if (!ThreadPool.QueueUserWorkItem(PerformUserWork<TResult>, state))
                throw new ApplicationException("Unable to queue user work item to the thread pool.");
            if (!state.WaitHandle.WaitOne(millisecondsWaitTimeout))
                throw new TimeoutException();
            return state.Result;
        }

        // PerformUserWork: perform the user specified action
        private static void PerformUserWork<T>(object workItem) 
        {
            var state = (State<T>)workItem;
            state.Result = state.Callback();
            if (state.WaitHandle != null)
                state.WaitHandle.Set();
        }

        // class State: structure containing the downloadTask to be performed as a thread in the threadpool
        private class State<T>
        {
            public State(bool wait = true)
            {
                Result = default!; // Initialize Result with default value
                Callback = default!; // Initialize Callback with default value
                if (wait) {
                    WaitHandle = new ManualResetEvent(false);
                }
                else {
                    WaitHandle = null!; // Initialize WaitHandle with default value
                }
            }
            public T Result { get; set; }
            public Func<T> Callback { get; set; }
            public ManualResetEvent WaitHandle { get; private set; }
        }
    }
}
