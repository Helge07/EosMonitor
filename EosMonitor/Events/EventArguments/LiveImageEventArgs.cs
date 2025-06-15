
using System.Drawing;

namespace EosMonitor
{
    public class LiveImageEventArgs : MemoryImageEventArgs
    {
        internal LiveImageEventArgs(byte[] imageData) : base(imageData) {
            Histogram = new long[256]; // Initialize Histogram with default values
        }

        public Point ImagePosition { get; internal set; }

        public long[] Histogram { get; internal set; }

        public long Zoom { get; internal set; }

        public Rectangle ZoomBounds { get; set; }
    }
}
