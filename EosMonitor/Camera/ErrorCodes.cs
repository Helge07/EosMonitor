using EDSDKLib;

namespace EosMonitor
{
   public enum ErrorCode : uint
   {
      /* Miscellaneous errors */
      Ok                         = EDSDK.EDS_ERR_OK,
      Unimplemented              = EDSDK.EDS_ERR_UNIMPLEMENTED,
      InternalError              = EDSDK.EDS_ERR_INTERNAL_ERROR,
      MemAllocFailed             = EDSDK.EDS_ERR_MEM_ALLOC_FAILED,
      MemFreeFailed              = EDSDK.EDS_ERR_MEM_FREE_FAILED,
      OperationCancelled         = EDSDK.EDS_ERR_OPERATION_CANCELLED,
      IncompatibleVersion        = EDSDK.EDS_ERR_INCOMPATIBLE_VERSION,
      NotSupported               = EDSDK.EDS_ERR_NOT_SUPPORTED,
      UnexpectedException        = EDSDK.EDS_ERR_UNEXPECTED_EXCEPTION,
      ProtectionViolation        = EDSDK.EDS_ERR_PROTECTION_VIOLATION,
      MissingSubcomponent        = EDSDK.EDS_ERR_MISSING_SUBCOMPONENT,
      SelectionUnavailable       = EDSDK.EDS_ERR_SELECTION_UNAVAILABLE,

      /* File errors */
      FileIoError                = EDSDK.EDS_ERR_FILE_IO_ERROR,
      FileTooManyOpen            = EDSDK.EDS_ERR_FILE_TOO_MANY_OPEN,
      FileNotFound               = EDSDK.EDS_ERR_FILE_NOT_FOUND,
      FileOpenError              = EDSDK.EDS_ERR_FILE_OPEN_ERROR,
      FileCloseError             = EDSDK.EDS_ERR_FILE_CLOSE_ERROR,
      FileSeekError              = EDSDK.EDS_ERR_FILE_SEEK_ERROR,
      FileTellError              = EDSDK.EDS_ERR_FILE_TELL_ERROR,
      FileReadError              = EDSDK.EDS_ERR_FILE_READ_ERROR,
      FileWriteError             = EDSDK.EDS_ERR_FILE_WRITE_ERROR,
      FilePermissionError        = EDSDK.EDS_ERR_FILE_PERMISSION_ERROR,
      FileDiskFullError          = EDSDK.EDS_ERR_FILE_DISK_FULL_ERROR,
      FileAlreadyExists          = EDSDK.EDS_ERR_FILE_ALREADY_EXISTS,
      FileFormatUnrecognized     = EDSDK.EDS_ERR_FILE_FORMAT_UNRECOGNIZED,
      FileDataCorrupt            = EDSDK.EDS_ERR_FILE_DATA_CORRUPT,
      FileNamingNa               = EDSDK.EDS_ERR_FILE_NAMING_NA,

      /* Directory errors */
      DirNotFound                = EDSDK.EDS_ERR_DIR_NOT_FOUND,
      DirIoError                 = EDSDK.EDS_ERR_DIR_IO_ERROR,
      DirEntryNotFound           = EDSDK.EDS_ERR_DIR_ENTRY_NOT_FOUND,
      DirEntryExists             = EDSDK.EDS_ERR_DIR_ENTRY_EXISTS,
      DirNotEmpty                = EDSDK.EDS_ERR_DIR_NOT_EMPTY,

      /* Property errors */
      PropertiesUnavailable      = EDSDK.EDS_ERR_PROPERTIES_UNAVAILABLE,
      PropertiesMismatch         = EDSDK.EDS_ERR_PROPERTIES_MISMATCH,
      PropertiesNotLoaded        = EDSDK.EDS_ERR_PROPERTIES_NOT_LOADED,

      /* Function Parameter errors */
      InvalidParameter           = EDSDK.EDS_ERR_INVALID_PARAMETER,
      InvalidHandle              = EDSDK.EDS_ERR_INVALID_HANDLE,
      InvalidPointer             = EDSDK.EDS_ERR_INVALID_POINTER,
      InvalidIndex               = EDSDK.EDS_ERR_INVALID_INDEX,
      InvalidLength              = EDSDK.EDS_ERR_INVALID_LENGTH,
      InvalidFunctionPointer     = EDSDK.EDS_ERR_INVALID_FN_POINTER,
      InvalidSortFunction        = EDSDK.EDS_ERR_INVALID_SORT_FN,

      /* Device errors */
      DeviceNotFound             = EDSDK.EDS_ERR_DEVICE_NOT_FOUND,
      DeviceBusy                 = EDSDK.EDS_ERR_DEVICE_BUSY,
      DeviceInvalid              = EDSDK.EDS_ERR_DEVICE_INVALID,
      DeviceEmergency            = EDSDK.EDS_ERR_DEVICE_EMERGENCY,
      DeviceMemoryFull           = EDSDK.EDS_ERR_DEVICE_MEMORY_FULL,
      DeviceInternalError        = EDSDK.EDS_ERR_DEVICE_INTERNAL_ERROR,
      DeviceInvalidParameter     = EDSDK.EDS_ERR_DEVICE_INVALID_PARAMETER,
      DeviceNoDisk               = EDSDK.EDS_ERR_DEVICE_NO_DISK,
      DeviceDiskError            = EDSDK.EDS_ERR_DEVICE_DISK_ERROR,
      DeviceCfGateChanged        = EDSDK.EDS_ERR_DEVICE_CF_GATE_CHANGED,
      DeviceDialChanged          = EDSDK.EDS_ERR_DEVICE_DIAL_CHANGED,
      DeviceNotInstalled         = EDSDK.EDS_ERR_DEVICE_NOT_INSTALLED,
      DeviceStayAwake            = EDSDK.EDS_ERR_DEVICE_STAY_AWAKE,
      DeviceNotReleased          = EDSDK.EDS_ERR_DEVICE_NOT_RELEASED,

      /* Stream errors */
      StreamIoError              = EDSDK.EDS_ERR_STREAM_IO_ERROR,
      StreamNotOpen              = EDSDK.EDS_ERR_STREAM_NOT_OPEN,
      StreamAlreadyOpen          = EDSDK.EDS_ERR_STREAM_ALREADY_OPEN,
      StreamOpenError            = EDSDK.EDS_ERR_STREAM_OPEN_ERROR,
      StreamCloseError           = EDSDK.EDS_ERR_STREAM_CLOSE_ERROR,
      StreamSeekError            = EDSDK.EDS_ERR_STREAM_SEEK_ERROR,
      StreamTellError            = EDSDK.EDS_ERR_STREAM_TELL_ERROR,
      StreamReadError            = EDSDK.EDS_ERR_STREAM_READ_ERROR,
      StreamWriteError           = EDSDK.EDS_ERR_STREAM_WRITE_ERROR,
      StreamPermissionError      = EDSDK.EDS_ERR_STREAM_PERMISSION_ERROR,
      StreamCouldntBeginThread   = EDSDK.EDS_ERR_STREAM_COULDNT_BEGIN_THREAD,
      StreamBadOptions           = EDSDK.EDS_ERR_STREAM_BAD_OPTIONS,
      StreamEndOfStream          = EDSDK.EDS_ERR_STREAM_END_OF_STREAM,

      /* Communications errors */
      CommPortIsInUse            = EDSDK.EDS_ERR_COMM_PORT_IS_IN_USE,
      CommDisconnected           = EDSDK.EDS_ERR_COMM_DISCONNECTED,
      CommDeviceIncompatible     = EDSDK.EDS_ERR_COMM_DEVICE_INCOMPATIBLE,
      CommBufferFull             = EDSDK.EDS_ERR_COMM_BUFFER_FULL,
      CommUsbBusError            = EDSDK.EDS_ERR_COMM_USB_BUS_ERR,

      /* Lock/Unlock */
      UsbDeviceLockError         = EDSDK.EDS_ERR_USB_DEVICE_LOCK_ERROR,
      UsbDeviceUnlockError       = EDSDK.EDS_ERR_USB_DEVICE_UNLOCK_ERROR,

      /* STI/WIA */
      StiUnknownError            = EDSDK.EDS_ERR_STI_UNKNOWN_ERROR,
      StiInternalError           = EDSDK.EDS_ERR_STI_INTERNAL_ERROR,
      StiDeviceCreateError       = EDSDK.EDS_ERR_STI_DEVICE_CREATE_ERROR,
      StiDeviceReleaseError      = EDSDK.EDS_ERR_STI_DEVICE_RELEASE_ERROR,
      DeviceNotLaunched          = EDSDK.EDS_ERR_DEVICE_NOT_LAUNCHED,

      EnumNa                     = EDSDK.EDS_ERR_ENUM_NA,
      InvalidFunctionCall        = EDSDK.EDS_ERR_INVALID_FN_CALL,
      HandleNotFound             = EDSDK.EDS_ERR_HANDLE_NOT_FOUND,
      InvalidId                  = EDSDK.EDS_ERR_INVALID_ID,
      WaitTimeoutError           = EDSDK.EDS_ERR_WAIT_TIMEOUT_ERROR,

      /* PTP */
      SessionNotOpen             = EDSDK.EDS_ERR_SESSION_NOT_OPEN,
      InvalidTransactionid       = EDSDK.EDS_ERR_INVALID_TRANSACTIONID,
      IncompleteTransfer         = EDSDK.EDS_ERR_INCOMPLETE_TRANSFER,
      InvalidStrageid            = EDSDK.EDS_ERR_INVALID_STRAGEID,
      DevicepropNotSupported     = EDSDK.EDS_ERR_DEVICEPROP_NOT_SUPPORTED,
      InvalidObjectformatcode    = EDSDK.EDS_ERR_INVALID_OBJECTFORMATCODE,
      SelfTestFailed             = EDSDK.EDS_ERR_SELF_TEST_FAILED,
      PartialDeletion            = EDSDK.EDS_ERR_PARTIAL_DELETION,
      SpecificationByFormatUnsupported = EDSDK.EDS_ERR_SPECIFICATION_BY_FORMAT_UNSUPPORTED,
      NoValidObjectinfo          = EDSDK.EDS_ERR_NO_VALID_OBJECTINFO,
      InvalidCodeFormat          = EDSDK.EDS_ERR_INVALID_CODE_FORMAT,
      UnknownVenderCode          = EDSDK.EDS_ERR_UNKNOWN_VENDER_CODE,
      CaptureAlreadyTerminated   = EDSDK.EDS_ERR_CAPTURE_ALREADY_TERMINATED,
      InvalidParentobject        = EDSDK.EDS_ERR_INVALID_PARENTOBJECT,
      InvalidDevicepropFormat    = EDSDK.EDS_ERR_INVALID_DEVICEPROP_FORMAT,
      InvalidDevicepropValue     = EDSDK.EDS_ERR_INVALID_DEVICEPROP_VALUE,
      SessionAlreadyOpen         = EDSDK.EDS_ERR_SESSION_ALREADY_OPEN,
      TransactionCancelled       = EDSDK.EDS_ERR_TRANSACTION_CANCELLED,
      SpecificationOfDestinationUnsupported = EDSDK.EDS_ERR_SPECIFICATION_OF_DESTINATION_UNSUPPORTED,
      UnknownCommand             = EDSDK.EDS_ERR_UNKNOWN_COMMAND,
      OperationRefused           = EDSDK.EDS_ERR_OPERATION_REFUSED,
      LensCoverClose             = EDSDK.EDS_ERR_LENS_COVER_CLOSE,
      LowBattery                 = EDSDK.EDS_ERR_LOW_BATTERY,
      ObjectNotReady             = EDSDK.EDS_ERR_OBJECT_NOTREADY,

      /* Capture Error */
      TakePictureAutoFocusFailed = EDSDK.EDS_ERR_TAKE_PICTURE_AF_NG,
      TakePictureReserved        = EDSDK.EDS_ERR_TAKE_PICTURE_RESERVED,
      TakePictureMirrorUp        = EDSDK.EDS_ERR_TAKE_PICTURE_MIRROR_UP_NG,
      TakePictureSensorCleaning  = EDSDK.EDS_ERR_TAKE_PICTURE_SENSOR_CLEANING_NG,
      TakePictureSilence         = EDSDK.EDS_ERR_TAKE_PICTURE_SILENCE_NG,
      TakePictureNoCard          = EDSDK.EDS_ERR_TAKE_PICTURE_NO_CARD_NG,
      TakePictureCard            = EDSDK.EDS_ERR_TAKE_PICTURE_CARD_NG,
      TakePictureCardProtect     = EDSDK.EDS_ERR_TAKE_PICTURE_CARD_PROTECT_NG,

      LastGenericErrorPlusOne    = EDSDK.EDS_ERR_LAST_GENERIC_ERROR_PLUS_ONE,
   }
}
