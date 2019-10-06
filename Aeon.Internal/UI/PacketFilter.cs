namespace Aeon.Internal.UI
{
    using Enums;
    using System.Collections.Generic;
    using System.ComponentModel;

    public sealed class PacketFilter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PacketFilter()
        {
            Length = new List<int>();
        }

        private bool capturePackets = true;

        private void NotifyChange(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public bool CapturePackets
        {
            get => capturePackets;
            set
            {
                capturePackets = value;
                NotifyChange("CapturePackets");
            }
        }

        public List<int> Length { get; set; }
        public byte[] Bytes { get; set; }
        public string Remote { get; set; }
        public PacketType Type { get; set; }
        public PacketSource Source { get; set; }
    }
}
