
namespace EosMonitor
{
    public class VolumeInfo
    {
        internal VolumeInfo()
        {
            VolumeLabel = string.Empty; // Initialize VolumeLabel to a non-null value
        }

        // StorageType: non / CF / SD
        public long StorageType { get; internal set; }

        // Access: Access type: Read / Write / ReadWrite / Error
        public long Access { get; internal set; }

        // MaxCapacityInBytes: Maximum size (in bytes)
        public ulong MaxCapacityInBytes { get; internal set; }

        // FreeSpaceInBytes: Available capacity (in bytes)
        public ulong FreeSpaceInBytes { get; internal set; }

        // VolumeLabel: volume label (string)
        public string VolumeLabel { get; internal set; }
    }
}



