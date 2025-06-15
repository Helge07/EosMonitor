using EDSDKLib;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace EosMonitor
{
    public abstract partial class Object : Disposable 
    {
        // Handle of this object
        private readonly IntPtr _handle;
        public           IntPtr Handle 
        { 
            get { return _handle; } 
        }

        // Constructor
        internal Object(IntPtr handle) 
        {
            _handle = handle;
        }

        // DisposeUnmanaged: Release camera handle
        protected override void DisposeUnmanaged()
        {
            if(_handle != IntPtr.Zero) EDSDK.EdsRelease(_handle);
            base.DisposeUnmanaged();
        }

        // "Get" functions for SDK properties
        #region      
        // GetPropertyDescript
        protected EDSDK.EdsPropertyDesc GetPropertyDescriptor(uint propertyId) 
        {
            if (MainWindow.cmdProcessor == null) 
                return default(EDSDK.EdsPropertyDesc);

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdGetPropertyDescriptor cmd = new cmdGetPropertyDescriptor(propertyId, syncEvent);
            MainWindow.cmdProcessor.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
            return cmd.descriptor;
        }

        // GetPropertyDataSize: Gets the byte size of a property from a camera or image object
        protected int GetPropertyDataSize(uint propertyId, EDSDK.EdsDataType expectedDataType) 
        {
            if (MainWindow.cmdProcessor == null) return 0;

            AutoResetEvent   syncEvent = new AutoResetEvent(false);
            cmdGetPropertyDataSize cmd = new cmdGetPropertyDataSize(propertyId, syncEvent);
            MainWindow.cmdProcessor?.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
            return cmd.dataSize;
        }

        // GetPropertyStruct: Get property data of generic type T
        public T GetPropertyStruct<T>(uint propertyId, EDSDK.EdsDataType expectedType) where T : struct
        {
            // At first get the data size
            int dataSize = 0;

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdGetPropertyDataSize cmd = new cmdGetPropertyDataSize(propertyId, syncEvent);
            MainWindow.cmdProcessor?.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
            dataSize = cmd.dataSize;

            var ptr = Marshal.AllocHGlobal(dataSize);
            // dataSize==0 may occur if propertyId == PropID_FocusInfo in case the FocusInfo is not yet ready 
            if (dataSize != 0) {
                syncEvent.Reset();
                cmdGetPropertyStruct cmd1 = new cmdGetPropertyStruct(propertyId, dataSize, ptr, syncEvent);
                MainWindow.cmdProcessor?.enqueueCmd(cmd1);
                syncEvent.WaitOne(new TimeSpan(0, 0, 1000), false);
            }
            T result = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return result;
        }
        
        // GetPropertyIntegerData: get property integer data from a camera object
        public long GetPropertyIntegerData(uint propertyId) {
            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdGetPropertyIntegerData cmd = new cmdGetPropertyIntegerData(propertyId, syncEvent);
            MainWindow.cmdProcessor?.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
            return cmd.IntData;
        }

        // GetPropertyPointData: get x- and y-coordinate of a point object
        public System.Drawing.Point GetPropertyPointData(uint propertyId) {
            var point = GetPropertyStruct<EDSDK.EdsPoint>(propertyId, EDSDK.EdsDataType.Point);
            return new System.Drawing.Point { X = point.x, Y = point.y };
        }

        // GetPropertyRectangleData: get coordinate of the upper left corner, width and height
        public Rectangle GetPropertyRectangleData(uint propertyId) {
            var rect = GetPropertyStruct<EDSDK.EdsRect>(propertyId, EDSDK.EdsDataType.Rect);
            return new Rectangle { X = rect.x, Y = rect.y, Height = rect.height, Width = rect.width };
        }

        // GetPropertyIntegerArrayData: get an array of integer data for a given prOpertyId
        public long[] GetPropertyIntegerArrayData(uint propertyId) {
            var dataSize = GetPropertyDataSize(propertyId, EDSDK.EdsDataType.UInt32_Array);
            var ptr = Marshal.AllocHGlobal(dataSize);

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdGetPropertyIntegerArrayData cmd = new cmdGetPropertyIntegerArrayData(propertyId, dataSize, ptr, syncEvent);
            MainWindow.cmdProcessor?.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);

            var signed = new int[dataSize / Marshal.SizeOf(typeof(uint))];
            Marshal.Copy(ptr, signed, 0, signed.Length);
            Marshal.FreeHGlobal(ptr);
            return signed.Select(i => (long)(uint)i).ToArray();
        }

        // GetPropertyStringData: get property string data from a camera object 
        protected string GetPropertyStringData(uint propertyId) {
            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdGetPropertysStringData cmd = new cmdGetPropertysStringData(propertyId, syncEvent);
            MainWindow.cmdProcessor?.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
            return cmd.StringData;
        }

        #endregion

        // "Set" functions for SDK properties
        #region 

        // SetPropertyIntegerData: set property integer data for a camera object
        public void SetPropertyIntegerData(uint propertyId, long IntData)
        {
            if (MainWindow.cmdProcessor == null) return;

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdSetPropertyIntegerData cmd = new cmdSetPropertyIntegerData(propertyId, (uint)IntData, syncEvent);
            MainWindow.cmdProcessor.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
        }

        // SetPropertyIntegerArrayData: set property integer array data for a camera object
        public void SetPropertyIntegerArrayData(uint propertyId, uint[] _data)
        {
            if (MainWindow.cmdProcessor == null) return;

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdSetPropertyIntegerArrayData cmd = new cmdSetPropertyIntegerArrayData(propertyId, _data, syncEvent);
            MainWindow.cmdProcessor.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
        }

        // SetPropertyStringData: set property string data from a camera object 
        public void SetPropertyStringData(uint propertyId, string data, int maxByteLength) {
            if (MainWindow.cmdProcessor == null) return;

            var bytes = Encoding.ASCII.GetBytes(data + "\0");
            if (bytes.Length > maxByteLength)
            throw new ArgumentException(string.Format("'{0}' converted to bytes is longer than {1}.", data, maxByteLength), "data");

            AutoResetEvent syncEvent = new AutoResetEvent(false);
            cmdSetPropertyStringData cmd = new cmdSetPropertyStringData(propertyId, data, syncEvent);
            MainWindow.cmdProcessor.enqueueCmd(cmd);
            syncEvent.WaitOne(new TimeSpan(0, 0, 100), false);
        }

        #endregion
    }
}

