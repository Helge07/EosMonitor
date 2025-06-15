
using System.Collections.Generic;

namespace EosMonitor
{
    public class PropIDList
    {
        // Constructor
        public PropIDList() 
          {
             _PropIDItems = new List<EdsPropID>();
             init();
          }   
      
        // List of PropID codes and strings
        private List<EdsPropID> _PropIDItems;

        // Initialization of the list _PropIDItems
        private void init() 
        {
             _PropIDItems.Add(new EdsPropID(0x0000ffff, "PropID_Unknown"                , " "));

             // cameraModel Setting Properties
             _PropIDItems.Add(new EdsPropID(0x00000002, "PropID_ProductName"            , " "));
             _PropIDItems.Add(new EdsPropID(0x00000015, "PropID_BodyIDEx"               , " "));
             _PropIDItems.Add(new EdsPropID(0x00000004, "PropID_OwnerName"              , " "));
             _PropIDItems.Add(new EdsPropID(0x00000005, "PropID_MakerName"              , " "));
             _PropIDItems.Add(new EdsPropID(0x00000006, "PropID_DateTime"               , " "));
             _PropIDItems.Add(new EdsPropID(0x00000007, "PropID_FirmwareVersion"        , " "));
             _PropIDItems.Add(new EdsPropID(0x00000008, "PropID_BatteryLevel"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000009, "PropID_CFn"                    , " "));
             _PropIDItems.Add(new EdsPropID(0x0000000b, "PropID_SaveTo"                 , " "));
             _PropIDItems.Add(new EdsPropID(0x0000000c, "kEdsPropID_CurrentStorage"     , " "));
             _PropIDItems.Add(new EdsPropID(0x0000000d, "kEdsPropID_CurrentFolder"      , " "));
             _PropIDItems.Add(new EdsPropID(0x0000000e, "kEdsPropID_MyMenu"             , " "));
             _PropIDItems.Add(new EdsPropID(0x00000010, "PropID_BatteryQuality"         , " "));
             _PropIDItems.Add(new EdsPropID(0x00000020, "PropID_HDDirectoryStructure"   , " "));

             // Image Properties
             _PropIDItems.Add(new EdsPropID(0x00000100, "PropID_ImageQuality"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000101, "PropID_JpegQuality"            , " "));
             _PropIDItems.Add(new EdsPropID(0x00000102, "PropID_Orientation"            , " "));
             _PropIDItems.Add(new EdsPropID(0x00000103, "PropID_ICCProfile"             , " "));
             _PropIDItems.Add(new EdsPropID(0x00000104, "PropID_FocusInfo"              , " "));
             _PropIDItems.Add(new EdsPropID(0x00000105, "PropID_DigitalExposure"        , " "));
             _PropIDItems.Add(new EdsPropID(0x00000106, "PropID_WhiteBalance"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000107, "PropID_ColorTemperature"       , " "));
             _PropIDItems.Add(new EdsPropID(0x00000108, "PropID_WhiteBalanceShift"      , " "));
             _PropIDItems.Add(new EdsPropID(0x00000109, "PropID_Contrast"               , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010a, "PropID_ColorSaturation"        , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010b, "PropID_ColorTone"              , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010c, "PropID_Sharpness"              , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010d, "PropID_ColorSpace"             , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010e, "PropID_ToneCurve"              , " "));
             _PropIDItems.Add(new EdsPropID(0x0000010f, "PropID_PhotoEffect"            , " "));
             _PropIDItems.Add(new EdsPropID(0x00000110, "PropID_FilterEffect"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000111, "PropID_ToningEffect"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000112, "PropID_ParameterSet"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000113, "PropID_ColorMatrix"            , " "));
             _PropIDItems.Add(new EdsPropID(0x00000114, "PropID_PictureStyle"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000115, "PropID_PictureStyleDesc"       , " "));
             _PropIDItems.Add(new EdsPropID(0x00000200, "PropID_PictureStyleCaption"    , " "));

             // Image Processing Properties
             _PropIDItems.Add(new EdsPropID(0x00000300, "PropID_Linear"                 , " "));
             _PropIDItems.Add(new EdsPropID(0x00000301, "PropID_ClickWBPoint"           , " "));
             _PropIDItems.Add(new EdsPropID(0x00000302, "PropID_WBCoeffs"               , " "));

             // Property Mask
             _PropIDItems.Add(new EdsPropID(0x80000000, "PropID_AtCapture_Flag"         , " "));
 
             // Capture Properties
            _PropIDItems.Add(new EdsPropID(0x00000400, "PropID_AEMode"                  , " "));
            _PropIDItems.Add(new EdsPropID(0x00000436, "PropID_AEModeSelect"            , " "));
            _PropIDItems.Add(new EdsPropID(0x00000401, "PropID_DriveMode"               , " "));
            _PropIDItems.Add(new EdsPropID(0x00000402, "PropID_ISOSpeed"                , " "));
            _PropIDItems.Add(new EdsPropID(0x00000403, "PropID_MeteringMode"            , " "));
            _PropIDItems.Add(new EdsPropID(0x00000404, "PropID_AFMode"                  , " "));
            _PropIDItems.Add(new EdsPropID(0x00000405, "PropID_Av"                      , " "));
            _PropIDItems.Add(new EdsPropID(0x00000406, "PropID_Tv"                      , " "));
            _PropIDItems.Add(new EdsPropID(0x00000407, "PropID_ExposureCompensation"    , " "));
            _PropIDItems.Add(new EdsPropID(0x00000408, "PropID_FlashCompensation"       , " "));
            _PropIDItems.Add(new EdsPropID(0x00000409, "PropID_FocalLength"             , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040a, "PropID_AvailableShots"          , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040b, "PropID_Bracket"                 , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040c, "PropID_WhiteBalanceBracket"     , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040d, "PropID_LensName"                , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040e, "PropID_AEBracket"               , " "));
            _PropIDItems.Add(new EdsPropID(0x0000040f, "PropID_FEBracket"               , " "));
            _PropIDItems.Add(new EdsPropID(0x00000410, "PropID_ISOBracket"              , " "));
            _PropIDItems.Add(new EdsPropID(0x00000411, "PropID_NoiseReduction"          , " "));
            _PropIDItems.Add(new EdsPropID(0x00000412, "PropID_FlashOn"                 , " "));
            _PropIDItems.Add(new EdsPropID(0x00000413, "PropID_RedEye"                  , " "));
            _PropIDItems.Add(new EdsPropID(0x00000414, "PropID_FlashMode"               , " "));
            _PropIDItems.Add(new EdsPropID(0x00000416, "PropID_LensStatus"              , " "));
            _PropIDItems.Add(new EdsPropID(0x00000418, "PropID_Artist"                  , " "));
            _PropIDItems.Add(new EdsPropID(0x00000419, "PropID_Copyright"               , " "));
            _PropIDItems.Add(new EdsPropID(0x0000041b, "PropID_DepthOfField"            , " "));
            _PropIDItems.Add(new EdsPropID(0x0000041e, "PropID_EFCompensation"          , " "));

		       // EVF Properties
            _PropIDItems.Add(new EdsPropID(0x00000500, "PropID_Evf_OutputDevice"        , " "));
            _PropIDItems.Add(new EdsPropID(0x00000501, "PropID_Evf_Mode"                , " "));
            _PropIDItems.Add(new EdsPropID(0x00000502, "PropID_Evf_WhiteBalance"        , " "));
            _PropIDItems.Add(new EdsPropID(0x00000503, "PropID_Evf_ColorTemperature"    , " "));
            _PropIDItems.Add(new EdsPropID(0x00000504, "PropID_Evf_DepthOfFieldPreview" , " "));

             // EVF IMAGE DATA Properties
            _PropIDItems.Add(new EdsPropID(0x00000507, "PropID_Evf_Zoom"                , " "));
            _PropIDItems.Add(new EdsPropID(0x00000508, "PropID_Evf_ZoomPosition"        , " "));
            _PropIDItems.Add(new EdsPropID(0x00000509, "PropID_Evf_FocusAid"            , " "));
            _PropIDItems.Add(new EdsPropID(0x0000050a, "PropID_Evf_Histogram"           , " "));
            _PropIDItems.Add(new EdsPropID(0x0000050B, "PropID_Evf_ImagePosition"       , " "));
            _PropIDItems.Add(new EdsPropID(0x0000050c, "PropID_Evf_HistogramStatus"     , " "));
            _PropIDItems.Add(new EdsPropID(0x0000050e, "PropID_Evf_AFMode"              , " "));

            _PropIDItems.Add(new EdsPropID(0x00000510, "PropID_Record"                  , " "));
            _PropIDItems.Add(new EdsPropID(0x00000541, "PropID_Evf_ZoomRect"            , " "));

            // Image GPS Properties
            _PropIDItems.Add(new EdsPropID(0x00000800, "PropID_GPSVersionID"            , " "));
            _PropIDItems.Add(new EdsPropID(0x00000801, "PropID_GPSLatitudeRef"          , " "));
            _PropIDItems.Add(new EdsPropID(0x00000802, "PropID_GPSLatitude"             , " "));
            _PropIDItems.Add(new EdsPropID(0x00000803, "PropID_GPSLongitudeRef"         , " "));
            _PropIDItems.Add(new EdsPropID(0x00000804, "PropID_GPSLongitude"            , " "));
            _PropIDItems.Add(new EdsPropID(0x00000805, "PropID_GPSAltitudeRef"          , " "));
            _PropIDItems.Add(new EdsPropID(0x00000806, "PropID_GPSAltitude"             , " "));
            _PropIDItems.Add(new EdsPropID(0x00000807, "PropID_GPSTimeStamp"            , " "));
            _PropIDItems.Add(new EdsPropID(0x00000808, "PropID_GPSSatellites"           , " "));
            _PropIDItems.Add(new EdsPropID(0x00000809, "PropID_GPSStatus"               , " "));
            _PropIDItems.Add(new EdsPropID(0x00000812, "PropID_GPSMapDatum"             , " "));
            _PropIDItems.Add(new EdsPropID(0x0000081d, "PropID_GPSDateStamp"            , " "));
        }

        // getPropIDItem: Find a PropIDList item to a given PropID CodeNumber
        public EdsPropID getPropIDItem(uint codeNumber) 
        {
              EdsPropID item = new EdsPropID() ;

              // Find an item by its CodeNumber
              EdsPropID? result = _PropIDItems.Find(delegate (EdsPropID Item) { return Item.CodeNumber == codeNumber; });

              if (result != null) { 
                    return  result; 
              } else {
                    item.CodeNumber  = codeNumber;
                    item.CodeString  = "PropID_Unknown";
                    item.Description = "PropID_Unknown";
                    return item;
              }
        }
    }

   // class EdsPropID:  Entry of the PropID-List
   public class EdsPropID
   {
      // class members
      private uint _CodeNumber;
      public uint CodeNumber 
      {
         get { return _CodeNumber; }
         set { _CodeNumber = value; }
      }

      private string _CodeString;
      public string CodeString 
      {
         get { return _CodeString; }
         set { _CodeString = value; }
      }

      private string _Description;
      public string Description
      {
         get { return _Description; }
         set { _Description = value; }
      }

      // Constructors
      public EdsPropID()
      {
          _CodeNumber = 0x0000ffff;
          _CodeString = "";
          _Description = "";
      }

      public EdsPropID(uint codeNumber, string codeString, string description)
      {
          _CodeNumber = codeNumber;
          _CodeString = codeString;
          _Description = description;
      }
    }
}
