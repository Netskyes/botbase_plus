using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aeon.Internal.Network;
using Aeon.Internal.Network.ClientExtensions;
using Aeon.Internal.Network.Data;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Aeon.Internal.UI
{
    using Enums;

    public partial class PacketEditor : MetroWindow
    {
        public ICollectionView PacketsListItems { get; set; }
        
        public ObservableCollection<ConnectionModel> ConnectionsListItems
        {
            get { return connectionsListItems; }
        }

        public PacketFilter PacketFilter { get; set; } = new PacketFilter();


        private ObservableCollection<PacketModel> packetsListItems = new ObservableCollection<PacketModel>();
        private ObservableCollection<ConnectionModel> connectionsListItems = new ObservableCollection<ConnectionModel>();
        private CancellationToken token;
        private CancellationTokenSource tokenSource;

        internal void Invoke(Action a) => Dispatcher.Invoke(a);


        public PacketEditor()
        {
            AppInternal.NetworkManager.ClientPacketEx += OnClientPacket;
            AppInternal.NetworkManager.ServerPacketEx += OnServerPacket;

            InitializeComponent();
            PacketsList.DataContext = this;
            ConnectionsList.DataContext = this;

            PacketsListItems = CollectionViewSource.GetDefaultView(packetsListItems);
            PacketsListItems.Filter = PacketsListFilter;

            // Populate lists
            TypeFilter.Items.Add(new KeyValueModel(0, "Show All"));
            TypeFilter.Items.Add(new KeyValueModel(1, "Server"));
            TypeFilter.Items.Add(new KeyValueModel(2, "Client"));

            ProtocolFilter.Items.Add(new KeyValueModel(0, "Show All"));
            ProtocolFilter.Items.Add(new KeyValueModel(1, "TCP"));
            ProtocolFilter.Items.Add(new KeyValueModel(2, "UDP"));
        }

        private void OnClientPacket(Server server, Client client, Packet packet)
        {
            if (!PacketFilter.CapturePackets)
                return;

            Invoke(() => packetsListItems.Add
                (GetPacketModel(PacketSource.Client, server, client, packet)));
        }

        private void OnServerPacket(Server server, Client client, Packet packet)
        {
            if (!PacketFilter.CapturePackets)
                return;

            Invoke(() => packetsListItems.Add
                (GetPacketModel(PacketSource.Server, server, client, packet)));
        }

        private PacketModel GetPacketModel(PacketSource source, Server server, Client client, Packet packet)
        {
            var packetModel = new PacketModel
            {
                Client = client,
                Server = server,
                ID = new Random().Next(999, 9999),
                EndPoint = (packet.UdpPacket) ? packet.EndPoint : client.EndPoint,
                Bytes = packet.Buffer,
                Length = packet.Buffer.Length,
                Type = (packet.UdpPacket) ? PacketType.UDP : PacketType.TCP,
                Source = source
            };

            packetModel.TypeHelp = string.Format
                ("{0}, {1}", packetModel.Type, packetModel.Source);

            return packetModel;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            Task.Run(() =>
                MonitorConnections(), token);
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            tokenSource.Cancel();
        }

        private void MonitorConnections()
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var clients = AppInternal.NetworkManager.Server.Clients.Where(x => x.Connected).ToList();

                    Invoke(() =>
                    {
                        var connList = connectionsListItems.ToArray();
                        foreach (var client in clients)
                        {
                            if (!connList.Any(x => x.Client == client))
                            {
                                connectionsListItems.Add(new ConnectionModel { Client = client });
                                continue;
                            }
                        }

                        for (int i = 0; i < connList.Length; i++)
                        {
                            var client = clients.FirstOrDefault(x => x == connList[i].Client);
                            if (client is null || !client.Connected)
                            {
                                connectionsListItems.Remove(connList[i]);
                            }
                        }
                    });
                }
                catch (AggregateException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Utils.Log(e.Message + " " + e.StackTrace, "error");
                }

                Thread.Sleep(1000);
            }
        }

        private void SendPacketByModel(PacketModel packet)
        {
            if (packet.Type == PacketType.TCP)
            {
                if (packet.Source == PacketSource.Server)
                {
                    var server = packet.Server.Clients.FirstOrDefault
                        (x => (x as ClientMain)?.ID == (packet.Client as ClientRelay)?.ID);
                    if (server != null)
                    {
                        server.Send(packet.Bytes);
                    }
                }
                else
                {
                    (packet.Client as ClientMain).Relay.Send(packet.Bytes);
                }
            }
            else
            {
                var udpLinks = (packet.Client as UdpServer)?.UdpLinks;
                if (udpLinks != null && udpLinks.ContainsKey(packet.EndPoint))
                {
                    var udpPacket = new Packet
                    {
                        Buffer = packet.Bytes,
                        EndPoint = (packet.Source == PacketSource.Client) ? udpLinks[packet.EndPoint] : packet.EndPoint,
                        UdpPacket = true
                    };
                    packet.Client.Send(udpPacket);
                }
            }
        }
        
        private void SavePacketsByModel(List<PacketModel> packets, string alias)
        {
            foreach (var packet in packets)
            {
                string query = string.Format("INSERT INTO packets (length, bytes, type, source, endpoint, alias) VALUES({0}, @Bytes, '{2}', '{3}', '{4}', '{5}');", 
                    packet.Length, packet.Bytes, packet.Type, packet.Source, packet.Remote, alias);

                try
                {
                    AppInternal.SQLiteClient.ExecuteQuery(query, new System.Data.SQLite.SQLiteParameter[] {
                        new System.Data.SQLite.SQLiteParameter("Bytes", System.Data.DbType.Binary) { Value = packet.Bytes }
                    });
                }
                catch (Exception e)
                {
                    Utils.Log(e.Message + " " + e.StackTrace, "error");
                }
            }
        }

        private void PacketsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as DataGrid)?.SelectedValue as PacketModel;
            Invoke(() =>
            {
                if (item != null)
                {
                    PacketHexBox.Text = BitConverter.ToString(item.Bytes).Replace("-", " ");
                    PacketTextBox.Text = Encoding.ASCII.GetString(item.Bytes);
                }
                else
                {
                    PacketHexBox.Clear();
                    PacketTextBox.Clear();
                }
            });
        }

        private void ReplayPacket_Click(object sender, RoutedEventArgs e)
        {
            var packets = Utils.Invoke(() => (PacketsList.SelectedItems.Cast<PacketModel>().ToList()));
            if (packets is null || packets.Count < 0)
                return;

            for (int i = 0; i < packets.Count; i++)
            {
                try
                {
                    SendPacketByModel(packets[i]);
                }
                catch (Exception ex)
                {
                    Utils.Log(ex.Message + " " + ex.StackTrace, "error");
                }
            }
        }

        private void ClearPackets_Click(object sender, RoutedEventArgs e)
        {
            Utils.Invoke(() => packetsListItems.Clear());
        }

        private void PatternFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hex = Utils.Invoke(() => PatternFilter.Text)?.Replace(" ", "");
            byte[] bytePattern = null;

            try
            {
                if (!string.IsNullOrEmpty(hex) && (hex.Length % 2) == 0)
                {
                    bytePattern = Utils.HexToByteArrayNoSpaces(hex);
                }

                PacketFilter.Bytes = bytePattern;
                PacketsListItems.Refresh();
            }
            catch (Exception ex)
            {
                Utils.Log(ex.Message + " " + ex.StackTrace, "error");
            }
        }

        private void ProtocolFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((sender as ComboBox).SelectedItem as KeyValueModel);
            if (item != null)
            {
                int type = (int)item.Key;

                PacketFilter.Type = (PacketType)type;
                PacketsListItems.Refresh();
            }
        }

        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ((sender as ComboBox).SelectedItem as KeyValueModel);
            if (item != null)
            {
                int type = (int)item.Key;

                PacketFilter.Source = (PacketSource)type;
                PacketsListItems.Refresh();
            }
        }

        private void LengthFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            var lenghts = Utils.Invoke(() => LengthFilter.Text).Split(',');
            if (lenghts.Count() > 0)
            {
                PacketFilter.Length.Clear();
                foreach (var len in lenghts)
                {
                    try
                    {
                        int length = int.Parse(len);
                        PacketFilter.Length.Add(length);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            PacketsListItems.Refresh();
        }

        private async void SavePackets_Click(object sender, RoutedEventArgs e)
        {
            var items = Utils.Invoke(() => packetsListItems).ToList();
            if (items.Count > 0)
            {
                var alias = await this.ShowInputAsync("Packets Alias", "Please enter a packet alias name:");
                if (!string.IsNullOrEmpty(alias))
                {
                    await Task.Run(() => SavePacketsByModel(items, alias));
                }
            }
        }

        private bool PacketsListFilter(object item)
        {
            var packet = (item as PacketModel);
            var result = true;

            result &= (PacketFilter.Remote == null || packet.Remote == PacketFilter.Remote);
            result &= (PacketFilter.Length.Count == 0 || !PacketFilter.Length.Contains(packet.Length));
            result &= (PacketFilter.Bytes == null || packet.Bytes.MatchPattern(PacketFilter.Bytes));
            result &= (PacketFilter.Source == PacketSource.Unknown) || (PacketFilter.Source == packet.Source);
            result &= (PacketFilter.Type == PacketType.Unknown) || (PacketFilter.Type == packet.Type);

            return result;
        }

        private void CapturePackets_Click(object sender, RoutedEventArgs e)
        {
            Invoke(() => PacketFilter.CapturePackets = (!PacketFilter.CapturePackets));
        }
    }
}
