
using System;

namespace EosMonitor
{
    public delegate void ProgressEventHandler(CameraModel sender, ProgressEventArgs e);

    public class ProgressEventArgs : EventArgs
    {
        private int percentComplete;
        public int PercentComplete
        {
            get { return percentComplete; }
            set { percentComplete = value; }
        }

        public ProgressEventArgs(int percentComplete)
        {
            PercentComplete = percentComplete;
        }
    }
}