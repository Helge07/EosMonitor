using System;

namespace EosMonitor
{
    public delegate void EvfOutputDeviceChangedEventHandler(CameraModel sender, EvfOutputDeviceChangedEventArgs e);

    public class EvfOutputDeviceChangedEventArgs : EventArgs
    {
        private long _newEvfOutputDevice;
        public long newEvfOutputDevice
        {
            get { return newEvfOutputDevice; }
            set { _newEvfOutputDevice = value; }
        }
        public uint PropID_Evf_OutputDevice { get; }
        public EvfOutputDeviceChangedEventArgs(IntPtr context)
        {
            newEvfOutputDevice = _newEvfOutputDevice;
        }
        public EvfOutputDeviceChangedEventArgs(uint propID_Evf_OutputDevice)
        {
            PropID_Evf_OutputDevice = propID_Evf_OutputDevice;
        }
    }
}