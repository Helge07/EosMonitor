
using System;

namespace EosMonitor
{
    public class VolumeInfoEventArgs : EventArgs
    {
        internal VolumeInfoEventArgs(VolumeInfo volumeInfo) {
            VolumeInfo = volumeInfo;
        }

        public VolumeInfo VolumeInfo { get; private set; }
    }
}
