using System;

namespace Aeon.Internal.Core
{
    public class PluginObject : MarshalByRefObject, IDisposable
    {
        private void Disconnect()
        {
            // Call a delegate to disconnect this obj.
        }

        public sealed override object InitializeLifetimeService()
        {
            return null;
        }

        public void Dispose()
        {
            // Call a delegate to dispose of this.
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Call delegate to dispose of this.
                }

                _disposed = true;
            }
        }

        protected bool _disposed; 
    }
}
