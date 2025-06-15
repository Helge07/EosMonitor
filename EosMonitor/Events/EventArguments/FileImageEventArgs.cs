
using System.IO;

namespace EosMonitor
{
    public class FileImageEventArgs : ImageEventArgs
    {
        internal FileImageEventArgs(string imageFilePath) { ImageFilePath = imageFilePath; }

        public string ImageFilePath { get; private set; }

        public override Stream GetStream()
        {
            return new FileStream(ImageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
