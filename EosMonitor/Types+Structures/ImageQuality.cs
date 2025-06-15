
namespace EosMonitor
{
   public struct ImageQuality
   {
      private ImageSize _secondaryImageSize;

      // PrimaryImageSize
      public ImageSize PrimaryImageSize { get; set; }

      // PrimaryImageFormat
      public ImageFormat PrimaryImageFormat { get; set; }

      // PrimaryCompressLevel
      public CompressLevel PrimaryCompressLevel { get; set; }

      // SecondaryImageSize
      public ImageSize SecondaryImageSize {
         get { return (byte)_secondaryImageSize >= 0xF ? ImageSize.Unknown : _secondaryImageSize; }
         set { _secondaryImageSize = value; }
      }

      // SecondaryImageFormat
      public ImageFormat SecondaryImageFormat { get; set; }

      // SecondaryCompressLevel
      public CompressLevel SecondaryCompressLevel { get; set; }

      // ToString(): returns a sting representation of the image quality parameters
      public override string ToString() 
      {
         return string.Format("Primary Image: Size <{0}>, Format <{1}>, CompressLevel <{2}>\n"
             + "Secondary Image: Size <{3}>, Format <{4}>, CompressLevel <{5}>",
             PrimaryImageSize, PrimaryImageFormat, PrimaryCompressLevel,
             SecondaryImageSize, SecondaryImageFormat, SecondaryCompressLevel);
      }

      // CreateBitMask: sets the image quality parameters according to a given bit mask
      internal static ImageQuality CreateBitMask(long bitMask) 
      {
         var quality = new ImageQuality {
            PrimaryImageSize = (ImageSize)((bitMask >> 24) & 0xFF),
            PrimaryImageFormat = (ImageFormat)((bitMask >> 20) & 0xF),
            PrimaryCompressLevel = (CompressLevel)((bitMask >> 16) & 0xF),
            SecondaryImageSize = (ImageSize)((bitMask >> 8) & 0xF),
            SecondaryImageFormat = (ImageFormat)((bitMask >> 4) & 0xF),
            SecondaryCompressLevel = (CompressLevel)(bitMask & 0xF),
         };
         return quality;
      }

      // ToBitMask: converts the image quality Parameters to a Bit-mask
      internal long ToBitMask() 
      {
         return (uint)PrimaryImageSize << 24 |
                (uint)PrimaryImageFormat << 20 |
                (uint)PrimaryCompressLevel << 16 |
                (uint)SecondaryImageSize << 8 |
                (uint)SecondaryImageFormat << 4 |
                (uint)SecondaryCompressLevel;
      }
   }
}
