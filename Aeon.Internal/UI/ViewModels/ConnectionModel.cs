using Aeon.Internal.Network;
using System.ComponentModel;

namespace Aeon.Internal.UI
{
    public sealed class ConnectionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Private Fields

        private Client client;
        private string name;
        private int packetsCount;
        
        #endregion

        public Server Server { get; set; }

        public Client Client
        {
            get { return client; }
            set
            {
                if (value != null)
                {
                    client = value;
                    NotifyPropertyChanged("Client");
                }
            }
        }

        public string Name
        {
            get { return client?.EndPoint.ToString() ?? "Unknown"; }
            set
            {
                if (value != null)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public int PacketsCount
        {
            get { return packetsCount; }
            set
            {
                packetsCount = value;
                NotifyPropertyChanged("PacketsCount");
            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
