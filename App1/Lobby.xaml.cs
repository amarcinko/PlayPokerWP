using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Lobby : Page
    {

        ObservableCollection<PeerAppInfo> _peerApps;    // informacije o spojenom peeru
        StreamSocket _socket;                           // socket za komunikaciju
        string _peerName = string.Empty;                // ime lokalnog peera
        object PlayerName;
        bool result_accept;
        object master = true;
        bool StartGameSent = false;

        // Error kodovi
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // ugasen bt
        const uint ERR_MISSING_CAPS = 0x80070005;       // u manifestu nije dodana mogućnost bt komunikacije
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // PeerFinder.Start() nije pokrenut i reklamiranje je ugašeno

        public Lobby()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            CoreApplication.Properties.TryGetValue("PlayerName", out PlayerName);
            tbPlayerName.Text = PlayerName.ToString();

            // Lista obližnjih peerova bindana sa UI listom
            _peerApps = new ObservableCollection<PeerAppInfo>();
            PeerList.ItemsSource = _peerApps;

            // Event dolaska zahtjeva za komunikacijom
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;

            // Započni reklamiranje
            PeerFinder.DisplayName = PlayerName.ToString();
            PeerFinder.Start();

            RefreshPeerAppList();

            base.OnNavigatedTo(e);
        }

        //Pristanak na komunikaciju
        public void CommandHandlers(IUICommand commandLabel)
        {
            var Actions = commandLabel.Label;
            switch (Actions)
            {
                //Yes gumb.
                case "Yes":
                    result_accept = true;
                    break;
                //No gumb.
                case "No":
                    result_accept = false;
                    break;
            }
        }

        async void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
            try
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    MessageDialog msg = new MessageDialog("Accept connection request?");

                    //Došao je zahtjev -> prihvati ili nemoj
                    msg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(CommandHandlers)));
                    msg.Commands.Add(new UICommand("No", new UICommandInvokedHandler(CommandHandlers)));
                    await msg.ShowAsync();

                    if (result_accept == true)
                    {
                        ConnectToPeer(args.PeerInformation);
                    }
                    else
                    {
                        // Odbijena komunikacija
                    }
                });
            }
            catch (Exception ex)
            {
                /*MessageDialog msg = new MessageDialog(ex.Message);
                await msg.ShowAsync();
                MessageBox.Show(ex.Message);*/
                CloseConnection(true);
            }
        }

        //Funkcija za spajanje
        async void ConnectToPeer(PeerInformation peer)
        {
            try
            {
                _socket = await PeerFinder.ConnectAsync(peer);
                CoreApplication.Properties.Remove("Socket");
                CoreApplication.Properties.Add("Socket", _socket);

                // Prestanak reklamiranja zbog štednje baterije.
                PeerFinder.Stop();

                _peerName = peer.DisplayName;
                CoreApplication.Properties.Add("Opponent", _peerName);
                UpdateChatBox("Chat started", true);

                // Započni slušanje poruka
                ListenForIncomingMessage();
            }
            catch (Exception ex)
            {
                // Implementirati pojedine exceptions
                //MessageBox.Show(ex.Message);
                CloseConnection(false);
            }
        }

        //Čitanje dolaznih poruka
        private DataReader _dataReader;
        private async void ListenForIncomingMessage()
        {
            try
            {
                var message = await GetMessage();
                if (message == "Start Game") { if(!StartGameSent)SendMessage("Start Game"); 
                                                this.Frame.Navigate(typeof(Game)); return; }

                // Dodavanje u chetBox
                UpdateChatBox(message, true);
                // Slusanje sljedece poruke.
                ListenForIncomingMessage();
            }
            catch (Exception)
            {
                UpdateChatBox("Chat Ended", true);
                CloseConnection(true);
            }
        }

        //Zatvaranje konekcije
        private void CloseConnection(bool continueAdvertise)
        {
            if (_dataReader != null)
            {
                _dataReader.Dispose();
                _dataReader = null;
            }

            if (_dataWriter != null)
            {
                _dataWriter.Dispose();
                _dataWriter = null;
            }

            if (_socket != null)
            {
                _socket.Dispose();
                _socket = null;
            }

            if (continueAdvertise)
            {
                // Ponovno reklamiranje.
                PeerFinder.Start();
            }
            else
            {
                PeerFinder.Stop();
            }
        }

        private async Task<string> GetMessage()
        {
            if (_dataReader == null)
                _dataReader = new DataReader(_socket.InputStream);

            /* Svaka poruka se šalje i čita u dva bloka:
             * pošalji/pročitaj veličinu poruke, 
             * pošalji/pročitaj poruku*/

            //var len = await GetMessageSize();
            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            return _dataReader.ReadString(messageLen);
        }

        private void FindPeers_Tap(object sender, RoutedEventArgs e)
        {
            RefreshPeerAppList();
        }

        /// <summary>
        /// Traženje peerova.
        /// </summary>
        private async void RefreshPeerAppList()
        {
            try
            {
                var peers = await PeerFinder.FindAllPeersAsync();

                _peerApps.Clear();

                if (peers.Count == 0)
                {
                    tbPeerList.Text = "No peers!";
                }
                else
                {
                    tbPeerList.Text = "Peers found:" + peers.Count;
                    // Dodaj pronađene peerove
                    foreach (var peer in peers)
                    {
                        _peerApps.Add(new PeerAppInfo(peer));
                    }

                    // Odaberi jedini peer
                    if (PeerList.Items.Count == 1)
                        PeerList.SelectedIndex = 0;

                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == ERR_BLUETOOTH_OFF)
                {
                    this.Frame.Navigate(typeof(MainPage));
                }
                else if ((uint)ex.HResult == ERR_MISSING_CAPS)
                {
                    this.Frame.Navigate(typeof(MainPage));
                }
                else if ((uint)ex.HResult == ERR_NOT_ADVERTISING)
                {
                    this.Frame.Navigate(typeof(MainPage));
                }
                else
                {
                    this.Frame.Navigate(typeof(MainPage));
                }
            }
            finally
            {
                //StopProgress();
            }
        }

        private async void ConnectToSelected_Tap_1(object sender, RoutedEventArgs e)
        {
            if (PeerList.SelectedItem == null)
            {
                var messageDialog = new MessageDialog("Select peer.");
                await messageDialog.ShowAsync();
                return;
            }

            // Spajanje na odabrani peer.
            PeerAppInfo pdi = PeerList.SelectedItem as PeerAppInfo;
            PeerInformation peer = pdi.PeerInfo;

            ConnectToPeer(peer);
        }

        //pisanje poruke
        DataWriter _dataWriter;
        private void SendMessage_Tap_1(object sender, RoutedEventArgs e)
        {
            SendMessage(txtMessage.Text);
        }

        private async void SendMessage(string message)
        {
            if (message.Trim().Length == 0)
            {
                var messageDialog = new MessageDialog("No msg!");
                await messageDialog.ShowAsync();
                return;
            }

            if (_socket == null)
            {
                var messageDialog = new MessageDialog("No peer connected.");
                await messageDialog.ShowAsync();
                return;
            }

            if (_dataWriter == null)
                _dataWriter = new DataWriter(_socket.OutputStream);

            // Slanje poruke (dva dijela)
            _dataWriter.WriteInt32(message.Length);
            await _dataWriter.StoreAsync();

            _dataWriter.WriteString(message);
            await _dataWriter.StoreAsync();

            UpdateChatBox(message, false);
        }

        private async void UpdateChatBox(string message, bool isIncoming)
        {
            if (isIncoming)
            {
                message = (String.IsNullOrEmpty(_peerName)) ? message = "No message" : message = "[" + _peerName + "]: " + message + "\n";
            }
            else
            {
                message = message = "[Me]: " + message + "\n";
            }

            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbChat.Text = message + tbChat.Text;
                txtMessage.Text = (isIncoming) ? txtMessage.Text : string.Empty;
            });
        }

        /// <summary>
        ///  klasa koja sadrži informacije o peer točkama
        /// </summary>

        public class PeerAppInfo
        {
            internal PeerAppInfo(PeerInformation peerInformation)
            {
                this.PeerInfo = peerInformation;
                this.DisplayName = this.PeerInfo.DisplayName;
            }

            public string DisplayName { get; private set; }
            public PeerInformation PeerInfo { get; private set; }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if(_socket != null)
            {
                CoreApplication.Properties.Add("Master", master);
                SendMessage("Start Game");
                StartGameSent = true;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseConnection(true);
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
