// ------------------------------------------------------------------
// Class Disposable implements the interface IDisposable. 
// As an abstract class it is the base class for classes implementing 
// explicit deallocation methods for managed/unmanaged  resources.
// ------------------------------------------------------------------

using System;

namespace EosMonitor
{
    public abstract class Disposable : IDisposable
    {
        protected bool _disposed=false;
      
        ~Disposable() { 
            // Deallocate only unmanaged resources
            Dispose(false); 
        }

        protected void CheckDisposed() 
        {
            if (_disposed)
            throw new ObjectDisposedException(GetType().Name);
        }

        protected virtual void DisposeManaged() { }

        protected virtual void DisposeUnmanaged() { }

        private void Dispose(bool disposing) 
        {
            if (!_disposed) {
                // explicit calls use Disposing==true
                if (disposing) DisposeManaged();
                // unmanaged resources are released by explicit calls and bay calls from the destructor
                DisposeUnmanaged();
                _disposed = true;
            }
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
