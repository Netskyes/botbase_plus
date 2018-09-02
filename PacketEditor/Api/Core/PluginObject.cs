using System;

namespace PacketEditor.Api
{
    public class PluginObject : MarshalByRefObject, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
