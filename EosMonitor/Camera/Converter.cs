
using EDSDKLib;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EosMonitor
{

    // Class Converter: 
    // Converts System.Drawing.Bitmap a Bitmap or Stream to a System.Windows.Media.Imaging.Bitmapsource
    public class Converter
    {
        // ConvertImageStreamToBytes(Bitmap): convert an imageStram to a byte array
        public byte[] ConvertImageStreamToBytes(IntPtr imageStream) 
        {
            IntPtr imagePtr;
            uint error = 0;
            error = EDSDK.EdsGetPointer(imageStream, out imagePtr);
            if (error != (uint)ErrorCode.Ok)
               throw new EosException(error, "Failed to get image imageDataPtr.");

            ulong imageLen;
            error = EDSDK.EdsGetLength(imageStream, out imageLen); 
            if (error != (uint)ErrorCode.Ok)
               throw new EosException(error, "Failed to get image imageDataPtr length.");

            var bytes = new byte[imageLen];
            Marshal.Copy(imagePtr, bytes, 0, bytes.Length);
            return bytes;
        }

        // Convert a System.Drawing.Bitmap to a System.Windows.Media.Imaging.Bitmapsource  
        public static BitmapSource CreateBitmapSourceFromBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));
            if (Application.Current.Dispatcher == null)
                throw new InvalidOperationException("Application dispatcher is not available.");
            try {
                using (MemoryStream memoryStream = new MemoryStream()) {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Make sure to create the bitmap in the UI thread
                    if (InvokeRequired)
                        return (BitmapSource)Application.Current.Dispatcher.Invoke(
                            new Func<Stream, BitmapSource>(CreateBitmapSourceFromStream),
                            DispatcherPriority.Normal,
                            memoryStream
                        );
                    return CreateBitmapSourceFromStream(memoryStream);
                }
            }
            catch (Exception) {
                throw new InvalidOperationException("Failed to create BitmapSource from Bitmap.");
            }
        }

        // CreateBitmapSourceFromStream(Stream): converts a System.Drawing.Bitmap Stream to a System.Windows.Media.Imaging.Bitmapsource  
        public static BitmapSource CreateBitmapSourceFromStream(Stream stream) 
        {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            // This will disconnect the stream from the image completely...
            WriteableBitmap writable = new WriteableBitmap(bitmapDecoder.Frames.Single());
            writable.Freeze();
            return writable;
        }

        private static bool InvokeRequired 
        {
            get { return Dispatcher.CurrentDispatcher != Application.Current.Dispatcher; }
        }
    }
}

