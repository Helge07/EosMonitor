using System;

namespace EosMonitor
{
    public delegate void AEModeChangedEventHandler(CameraModel sender, AEModeChangedEventArgs e);

    public class AEModeChangedEventArgs : EventArgs
    {
        private long _newAEMode;
        public long newAEMode
        {
            get { return newAEMode; }
            set { _newAEMode = value; }
        }

        public AEModeChangedEventArgs(long _nextAEMode)
        {
            newAEMode = _nextAEMode;
        }

    }
}