
using System.IO;

namespace EosMonitor
{
    public class MemoryImageEventArgs : ImageEventArgs
    {
        internal MemoryImageEventArgs(byte[] imageData) {
            ImageData = imageData;
        }

        public byte[] ImageData { get; private set; }

        public override Stream GetStream() {
            return new MemoryStream(ImageData);
        }
    }
}
