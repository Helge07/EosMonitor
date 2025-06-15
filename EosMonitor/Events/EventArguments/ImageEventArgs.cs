
using System;
using System.Drawing;
using System.IO;

namespace EosMonitor
{
    public abstract class ImageEventArgs : EventArgs
    {
        public virtual Image GetImage() 
        {
            using (var stream = GetStream()) return Image.FromStream(stream);
        }

        public virtual Bitmap GetBitmap()
        {
            using (var stream = GetStream()) return new Bitmap(stream);
        }

        public abstract Stream GetStream();
    }
}
