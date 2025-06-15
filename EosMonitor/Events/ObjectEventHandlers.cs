
using EDSDKLib;
using System;


// Object events Events:
// ------------------------------------------------------------
// ObjectEvent_All:                         Indicates changes of the volume object (memory card)
// ObjectEvent_VolumeInfoChanged            Indicates changes of memory card state 
// ObjectEvent_VolumeUpdateItems            Indicates changes of object on the memory card 
// ObjectEvent_FolderUpdateItems            Indicates changes of files or folders on the memory card
// ObjectEvent_DirItemCreated               Indicates creation of files or folders on the memory card
// ObjectEvent_DirItemRemoved               Indicates deletion of files or folders on the memory card
// ObjectEvent_DirItemInfoChanged           Indicates information of DirItem objects have been changed
// ObjectEvent_DirItemContentChanged        Indicates that file header information has been updated (p.e.rotation info)
// ObjectEvent_DirItemRequestTransfer       Indicates that there are objects on a camera to be transferred to a computer
// ObjectEvent_DirItemRequestTransferDT     Indicates that the camera's direct transfer button is pressed
// ObjectEvent_DirItemCancelTransferDT      Indicates requests from a camera to cancel object transfer
// ObjectEvent_VolumeAdded                  Indicates that a memory card was added
// ObjectEvent_VolumeRemoved                Indicates that a memory card was removed


namespace EosMonitor
{
    public partial class CameraModel : Object
    {
        // Call the handlers for object events
        private uint HandleObjectEvent(uint objectEvent, IntPtr sender, IntPtr context)
        {
            try {
                switch (objectEvent) {
                    case EDSDK.ObjectEvent_VolumeInfoChanged:
                        // OnObjectEventVolumeInfoChanged(sender);
                        break;
                    case EDSDK.ObjectEvent_VolumeUpdateItems:
                        OnObjectEventVolumeUpdateItems(sender, context);
                        break;
                    case EDSDK.ObjectEvent_FolderUpdateItems:
                        OnObjectEventFolderUpdateItems(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemCreated:
                        OnObjectEventDirItemCreated(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemRemoved:
                        OnObjectEventDirItemRemoved(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemInfoChanged:
                        OnObjectEventDirItemInfoChanged(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemContentChanged:
                        OnObjectEventDirItemContentChanged(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemRequestTransfer:
                        OnObjectEventDirItemRequestTransfer(sender);
                        break;
                    case EDSDK.ObjectEvent_DirItemRequestTransferDT:
                        OnObjectEventDirItemRequestTransferDt(sender, context);
                        break;
                    case EDSDK.ObjectEvent_DirItemCancelTransferDT:
                        OnObjectEventDirItemCancelTransferDt(sender, context);
                        break;
                    case EDSDK.ObjectEvent_VolumeAdded:
                        OnObjectEventVolumeAdded(sender, context);
                        break;
                    case EDSDK.ObjectEvent_VolumeRemoved:
                        OnObjectEventVolumeRemoved(sender, context);
                        break;
                    case EDSDK.ObjectEvent_All:
                        break;

                }
            }
            catch (Exception ex) {
                string s = ex.ToString();     // dummy -Anweisung;
            }
            finally {
                EDSDK.EdsRelease(sender);
            }
            return EDSDK.EDS_ERR_OK;
        }

        private readonly ImageTransfer _imagetransfer = new ImageTransfer();

        // Get new volume information if object volume info has changed
        private void OnVolumeInfoChanged(VolumeInfoEventArgs eventArgs)
        {
            if (VolumeInfoChanged != null)
                VolumeInfoChanged(this, eventArgs);
        }
        private void OnObjectEventVolumeInfoChanged(IntPtr sender) {
            EDSDK.EdsVolumeInfo volumeInfo;
            uint error = EDSDK.EdsGetVolumeInfo(sender, out volumeInfo);
            if (error != (uint)ErrorCode.Ok)
            throw new EosException (error, "Failed to get volume ImageInfo.");

            OnVolumeInfoChanged(new VolumeInfoEventArgs(new VolumeInfo {
            Access = volumeInfo.Access,
            FreeSpaceInBytes = volumeInfo.FreeSpaceInBytes,
            MaxCapacityInBytes = volumeInfo.MaxCapacity,
            StorageType = volumeInfo.StorageType,
            VolumeLabel = volumeInfo.szVolumeLabel
            }));
        }
        private void OnObjectEventVolumeUpdateItems(IntPtr sender, IntPtr context) {
        }               // not implemeted
        private void OnObjectEventFolderUpdateItems(IntPtr sender, IntPtr context) {
        }               // not implemeted

        // Show preview image and throw PictureTaken-Event
        private void OnObjectEventDirItemCreated(IntPtr sender, IntPtr context) 
        {
            if (MainWindow.cameraModel == null) return;

            if (MainWindow.cameraModel.IsPreviewActive)
            _imagetransfer.DownloadImageToViewer(sender);
            EventArgs e = new EventArgs();
            if (PictureTaken != null) PictureTaken(this, e);
        }
        private void OnObjectEventDirItemRemoved(IntPtr sender, IntPtr context) {
        }                  // not implemeted
        private void OnObjectEventDirItemInfoChanged(IntPtr sender, IntPtr context) {
        }              // not implemeted
        private void OnObjectEventDirItemContentChanged(IntPtr sender, IntPtr context) {
        }           // not implemeted

        // Download Image to a File
        private void OnPictureTakenTransfer(ImageEventArgs eventArgs)
        {   
            if ( PictureTakenTransfer != null)
                PictureTakenTransfer(this, eventArgs);
        }
        private void OnObjectEventDirItemRequestTransfer(IntPtr sender) {
            OnPictureTakenTransfer(_imagetransfer.DownloadImageToFile(sender, picturePath));
        }
        private void OnObjectEventDirItemRequestTransferDt(IntPtr sender, IntPtr context) {
        }        // not implemeted
        private void OnObjectEventDirItemCancelTransferDt(IntPtr sender, IntPtr context) {
        }         // not implemeted

        private void OnObjectEventVolumeAdded(IntPtr sender, IntPtr context) {
        }                     // not implemeted
        private void OnObjectEventVolumeRemoved(IntPtr sender, IntPtr context) {
        }                   // not implemeted

    }
}