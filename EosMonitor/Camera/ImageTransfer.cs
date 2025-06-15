
using EDSDKLib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace EosMonitor
{
   class ImageTransfer
   {
      private static EDSDK.EdsDirectoryItemInfo GetDirectoryItemInfo(IntPtr directoryItem) 
      {
         EDSDK.EdsDirectoryItemInfo directoryItemInfo;
         uint error = EDSDK.EdsGetDirectoryItemInfo(directoryItem, out directoryItemInfo);
         if (error != (uint)ErrorCode.Ok)
            throw new EosException(error, "DirectoryItemInfo structure cannot be obtained from the camera  ");
         return directoryItemInfo;
      }

      // CreateFileStream: Creates a new file on the host and  a file stream for access to this file
      private static IntPtr CreateFileStream(string imageFilePath) 
      {
         IntPtr stream;
         uint error = EDSDK.EdsCreateFileStream(imageFilePath, EDSDK.EdsFileCreateDisposition.CreateAlways, EDSDK.EdsAccess.ReadWrite, out stream);
         if (error != (uint)ErrorCode.Ok)
            throw new EosException(error, "File stream could not be created");
         return stream;
      }

      // CreateMemoryStream: Creates a stream in the memory of the host
      private static IntPtr CreateMemoryStream(uint size) 
      {
         IntPtr stream;
         uint error = EDSDK.EdsCreateMemoryStream(size, out stream);
         if (error != (uint)ErrorCode.Ok)
            throw new EosException(error, "Memory stream could not be created");
         return stream;
      }

      // DestroyStream: Decrements the reference counter RefCtr to a stream; if RefCtr becomes 0, the stream is released.
      private static void DestroyStream(ref IntPtr stream) 
      {
         if (stream != IntPtr.Zero) {
            uint error = EDSDK.EdsRelease(stream);
            if (error != (uint)ErrorCode.Ok)
               throw new EosException(error, "Failed to release stream");  
            stream = IntPtr.Zero;
         }
      }

      // Download: download a "directoryItem" on the camera of size "size" to a previously craeted stream
      private static void Download(IntPtr directoryItem, uint size, IntPtr stream) 
      {
         uint error = 0;
         if (stream == IntPtr.Zero) return;
         try {
            error = EDSDK.EdsDownload(directoryItem, size, stream);
            if (error != (uint)ErrorCode.Ok)
               throw new EosException(error, "Error during downloading to a stream.");
            error = EDSDK.EdsDownloadComplete(directoryItem);
            if (error != (uint)ErrorCode.Ok)
               throw new EosException(error, "Error during downloading to a stream.");
         }
         catch (Exception ex) {
            EDSDK.EdsDownloadCancel(directoryItem);
            throw new EosException(error, "Error during downloading to a stream.", ex);
         }
         finally {
         }
      }

      // GetFilePath:  If a file exists at a location  "_BasePath + _Filename"
      private static string GetFilePath(string _BasePath, string _Filename) 
      {

         // Combine the full file path from the "_BasePath" and the _Filename
         var FilePath = System.IO.Path.Combine(_BasePath ?? Environment.CurrentDirectory, _Filename);

         // If the file exists on the host, ask for selection of a new file resp. confirmation to overwrite:

         // If image file exists on the host ask user
         if (File.Exists(FilePath)) {

            // Show Warning "Overwrite?":  If accepted, the image file will be overwritten 
            string message = "File " + FilePath + " exists. Overwrite?";
            string caption = "Confirmation";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;
            if (System.Windows.MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes) {
               // On "Yes" the FilePath remains as it is, the file will be overwritten
            } else {
               // Display a SaveFileDialog so that the user can save the Image to another file

               // get the file type from the FilePath and build the filter string
               string extension = "*" + FilePath.Substring(FilePath.Length - 4);
               SaveFileDialog saveFileDialog = new SaveFileDialog();
               if (extension == "*.CR2")
                  saveFileDialog.Filter = "RAW Image|" + extension;
               else
                  saveFileDialog.Filter = "JPeg Image|" + extension;
               saveFileDialog.Title = "Save Image File";

               do {
                  saveFileDialog.ShowDialog();
                  if (saveFileDialog.FileName != "")
                     FilePath = saveFileDialog.FileName;
               } while (saveFileDialog.FileName == "");
            }
         }
         return FilePath;
      }

      // DownloadImageToFile: Download a directory item on the camera to a file path
      public ImageEventArgs DownloadImageToFile(IntPtr directoryItem, string imageBasePath) 
      {
         // Get the "directoryItemInfo" structure of the object to be transferred
         var directoryItemInfo = GetDirectoryItemInfo(directoryItem);

         // Combine the full image file path from the "imageBasePath" (== cameraModel.picturePath) 
         // and the image filename on the camera (== directoryItemInfo.szfilename)
         string imageFilePath = GetFilePath(imageBasePath, directoryItemInfo.szFileName);

         // CreateBitMask a file stream
         var stream = CreateFileStream(imageFilePath);

         // activate progressMade callbacks and start image download to FileStream
         try {
            // Register ProgressMade event handler
            MainWindow.cameraModel?.RegisterProgressMadeEventHandler(stream, EDSDK.EdsProgressOption.Periodically);

            // Download a directory item on the camera to a file stream
            Download(directoryItem, (uint)directoryItemInfo.Size, stream);

            // Unregister ProgressMade event handler
            MainWindow.cameraModel?.RegisterProgressMadeEventHandler(stream, EDSDK.EdsProgressOption.NoReport);

            // Hide the progressbar
            MainWindow._progressbarActive = false;
         }
         catch (Exception ex) {
            throw new EosException(EDSDK.EDS_ERR_STREAM_IO_ERROR, "Exception during stream IO.", ex);
         }
         finally {
            DestroyStream(ref stream);
         }
         return new FileImageEventArgs(imageFilePath);
      }

      // DownloadImageToMemory: Register the "ProgressMade" event handler" and download a directory item on the camera to a file stream
      public ImageEventArgs DownloadImageToMemory(IntPtr directoryItem) 
      {
         var directoryItemInfo = GetDirectoryItemInfo(directoryItem);
         var stream = CreateMemoryStream((uint)directoryItemInfo.Size);

         try {
            Download(directoryItem, (uint)directoryItemInfo.Size, stream);
            var converter = new Converter();
            return new MemoryImageEventArgs(converter.ConvertImageStreamToBytes(stream));
         }
         finally {
            DestroyStream(ref stream);
         }
      }

      // DownloadImageToViewer: Download a preview image via a memory stream and display it
      public void DownloadImageToViewer(IntPtr directoryItem) 
      {
         uint    error = 0;            // error return code for SDK functions
         IntPtr  streamPtr;            // pointer to the memory stream
         IntPtr  imagePtr;             // pointer to the downloaded image      
         IntPtr  imageDataStream;      // memory stream for the downloaded image data
         IntPtr  imageDataPtr;         // pointer to the pixel data of the downloaded image
         Bitmap  _PreviewImage;        // Bitmap image to be shown as preview
         EDSDK.EdsImageInfo ImageInfo; // image information structure for the image to downloaded

         // Get DirectoryItemInfo:
         var directoryItemInfo = GetDirectoryItemInfo(directoryItem);

         // CreateBitMask a memory stream
         var stream = CreateMemoryStream((uint)directoryItemInfo.Size);

         // Register the ProgressMade event handler
         MainWindow.cameraModel?.RegisterProgressMadeEventHandler(stream, EDSDK.EdsProgressOption.Periodically);

         // Download the image from the camera 
         Download(directoryItem, (uint)directoryItemInfo.Size, stream);

         // Unregister the ProgressMade event handler
         MainWindow.cameraModel?.RegisterProgressMadeEventHandler(stream, EDSDK.EdsProgressOption.NoReport);

         // create pointers to the memory stream and to the image data
         error = EDSDK.EdsGetPointer(stream, out streamPtr);
         error = EDSDK.EdsCreateImageRef(stream, out imagePtr);

         // Get the ImageInfo of the image:
         // uint width;                // image width
         // uint height;               // image height
         // uint numOfComponents;      // number of color components per pixel.
         // uint componentDepth;       // bits per sample (8 or 16)
         // EdsRect effectiveRect;     // Effective rectangles (excludes potential black lines at top/bottom)
         error = EDSDK.EdsGetImageInfo(imagePtr, EDSDK.EdsImageSource.Preview, out ImageInfo);

         // Rectangle to be retrieved (processed) from the source image
         EDSDK.EdsRect DestRectangle = new EDSDK.EdsRect();
         DestRectangle.height=(int)ImageInfo.Height;
         DestRectangle.width=(int)ImageInfo.Width;
         DestRectangle.x=0;
         DestRectangle.y=0;

         // Rectangle size for output image
         EDSDK.EdsSize size;
         size.height = DestRectangle.height;
         size.width  = DestRectangle.width;

         // Number of bytes we need to store the image data
         uint bufferSize = (uint)(ImageInfo.NumOfComponents * ImageInfo.ComponentDepth * ImageInfo.Height * ImageInfo.Width) / 8;

         // Alocate buffer for the image data
         Byte[] buffer    = new Byte[bufferSize];
         GCHandle h0      = GCHandle.Alloc(buffer, GCHandleType.Pinned);
         IntPtr bufferPtr = h0.AddrOfPinnedObject();

         // CreateBitMask memory Stream to the buffer
         error = EDSDK.EdsCreateMemoryStreamFromPointer(bufferPtr, bufferSize, out imageDataStream);

         // Download the image data to the buffer
         error = EDSDK.EdsGetImage(imagePtr, EDSDK.EdsImageSource.Preview, EDSDK.EdsTargetImageType.RGB, DestRectangle, size, imageDataStream);

         // CreateBitMask a Bitmap image from the downloaded camera image:

         // Exchange Red and Blue channels
         Byte tmpByte;
         for (int i = 0; i < bufferSize; i += 3) {
            tmpByte = buffer[i]; buffer[i] = buffer[i + 2]; buffer[i + 2] = tmpByte;
         }

         // Get Pointer to the image data
         error = EDSDK.EdsGetPointer(imageDataStream, out imageDataPtr);

         // compute length of a pixel line: stride = number of bytes/line runded up to the next multiple of 4
         int stride = ((size.width * 3) + 3) & ~((int)3);

         // CreateBitMask a System.Drawing.Bitmap image from the buffered image
         _PreviewImage = new Bitmap(size.width, size.height, stride, PixelFormat.Format24bppRgb, imageDataPtr);

         // Set the ImageSource for the PreviewImage
         if (MainWindow.cameraModel != null)
            MainWindow.cameraModel.PreviewImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap
                       ( _PreviewImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                         BitmapSizeOptions.FromWidthAndHeight(_PreviewImage.Width, _PreviewImage.Height));
         // Hide the progressbar
         MainWindow._progressbarActive = false;

         // Release the streams
         DestroyStream(ref imagePtr);
         DestroyStream(ref imageDataStream);
         DestroyStream(ref stream);

         return;  
      }
   }
}
