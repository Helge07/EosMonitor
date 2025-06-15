
using EDSDKLib;

namespace EosMonitor
{
    // Enable/Disable
    public enum LvState : uint
    {
        LiveViewON  = 0,
        LiveViewOFF = 1,
    }

    // Enable/Disable
    public enum EnableState: uint 
     {
       disabled = 0, 
       enabled  = 1,
    }
   
   // Shutter Button State
   public enum ShutterButtonState : long
   {
      Off = 0,     
      Halfway = 1,     
      Comletely = 2,     
   }

   // Compression Level for jpeg images
   public enum CompressLevel : byte 
   {
      JpegUncompressed  = 0x0,
      JpegCompression1  = 0x1,
      Normal            = 0x2,
      Fine              = 0x3,
      Lossless          = 0x4,
      SuperFine         = 0x5,
      JpegCompression6  = 0x6,
      JpegCompression7  = 0x7,
      JpegCompression8  = 0x8,
      JpegCompression9  = 0x9,
      JpegCompression10 = 0xa,
      Unknown           = 0xf,
   }

   // Drive Lens command in-Parameters
   public enum EvfDriveLens : uint
   {
      Near3  = EDSDK.EvfDriveLens_Near3,
      Near2  = EDSDK.EvfDriveLens_Near2,
      Near1  = EDSDK.EvfDriveLens_Near1,
      Far1   = EDSDK.EvfDriveLens_Far1,
      Far2   = EDSDK.EvfDriveLens_Far2,
      Far3   = EDSDK.EvfDriveLens_Far3,

   }

   // Evf zoom ratio
   public enum EvfZoomFactor : uint 
   {
      fit               = EDSDK.EvfZoom_Fit,
      x5                = EDSDK.EvfZoom_x5,
      x10               = EDSDK.EvfZoom_x10,
   }

   // images Formats
   public enum ImageFormat : byte 
   {
      Unknown           = 0x0,
      Jpeg              = 0x1,
      Crw               = 0x2,
      Raw               = 0x4,
      Cr2               = 0x6,
   }

   // Images size
   public enum ImageSize : uint 
   {
      Large             = 0,
      Middle            = 1,
      Small             = 2,
      Middle2           = 5,
      Middle3           = 6,
      Small2            = 14,
      Small3            = 15,
      Small4            = 16,
      Unknown           = 255,
   }

    // Live View Autofocus modes
    public enum LiveViewAutoFocus : uint
    {
        QuickAF_Mode                = 0x00,
        LiveAF_Mode                 = 0x01,
        LiveFaceMode                = 0x02,
        FlexiZone_Mode              = 0x03,
        ZoneAF_Mode                 = 0x04,
        ExpandAFArea_Mode           = 0x05,
        ExpandAFAreaAround_Mode     = 0x06,
        LargeZoneAF_Horizontal      = 0x07,
        LargeZoneAFVertikal         = 0x08,
        TrackingAF_Mode             = 0x09,
        SpotAF_Mode                 = 0x0a,
        FlexibleZoneAF1_Mode        = 0x0b,
        FlexibleZoneAF2_Mode        = 0x0c,
        FlexibleZoneAF3_Mode        = 0x0d,
        WholeAreaAF_Mode            = 0x0e,
        NoTrackingSpotAF            = 0x0f,
        NoTracking1pointAF_Mode     = 0x10,
        NoTrackingExpandAF_Mode     = 0x11,
        NoTrackingExpandAFArea_Mode = 0x12
    }

    // Live View Device
    public enum LiveViewDevice : int 
   {
      None              = 0,
      Camera            = 1,
      Host              = 2
   }

   // Picture save location
   public enum SaveLocation 
   { 
      Camera            = 1, 
      Host              = 2 
   };

   // White Balance
   public enum WhiteBalance : long 
   {
      Pasted            = -2,
      Click             = -1,
      Auto              = 0,
      Daylight          = 1,
      Cloudy            = 2,
      Tungsten          = 3,
      Fluorescent       = 4,
      Flash             = 5,
      Manual            = 6,
      Shade             = 8,
      ColorTemperature  = 9,
      Custom            = 10,
      Custom2           = 11,
      Custom3           = 12,
      Manual2           = 15,
      Manual3           = 16,
      Manual4           = 18,
      Manual5           = 19,
      Custom4           = 20,
      Custom5           = 21,
   }

}