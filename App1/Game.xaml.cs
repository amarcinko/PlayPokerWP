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
using Windows.UI;
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
    /// Multiplayer igra.
    /// </summary>
    public sealed partial class Game : Page
    {
        StreamSocket _socket;
        object sckt;                           // socket korišten za komunikaciju sa peer-om
        string _peerName = string.Empty;                // ime igrača
        object PlayerName;
        bool mastercheck = false;
        object master;
        object pName;
        string sntMessages = "";
        string rcvMssgs = "";
        int state = 1;

        double gain, bet, score, plot, rised = 0, rised_tb=0, old_rise = 0;

        bool card1_check = false;
        bool card2_check = false;
        bool card3_check = false;
        bool card4_check = false;
        bool card5_check = false;
        string[] checkedString = {"0", "0", "0", "0", "0"};

        // Error kodovi
        const uint ERR_BLUETOOTH_OFF = 0x8007048F;      // ugasen bt
        const uint ERR_MISSING_CAPS = 0x80070005;       // u manifestu nije dodana mogućnost bt komunikacije
        const uint ERR_NOT_ADVERTISING = 0x8000000E;    // PeerFinder.Start() nije pokrenut i reklamiranje je ugašeno

        string[] cards = { "Aa", "Ab", "Ac", "Ad", "2a", "2b", "2c", "2d", "3a", "3b", "3c", "3d", "4a", "4b", "4c", "4d", 
                         "5a", "5b", "5c", "5d","6a", "6b", "6c", "6d","7a", "7b", "7c", "7d","8a", "8b", "8c", "8d",
                         "9a", "9b", "9c", "9d","10a", "10b", "10c", "10d","Ja", "Jb", "Jc", "Jd","Qa", "Qb", "Qc", "Qd",
                         "Ka", "Kb", "Kc", "Kd"};
        string[] selected = new string[10];
        string[] slaveHand = new string[5];
        string[] sHand = new string[5];
        int masterInt = 0;
        int slaveInt = 0;
        int masterMax = 0;
        int slaveMax = 0;
        string stringScore = "-";
        List<Border> c_oBdrs = new List<Border>();
        bool fld_o = false;
        bool fld_y = false;
        bool ready = true;

        Random rnd = new Random();

        public Game()
        {
            this.InitializeComponent();

            c_oBdrs.Add(c_o1Bdr);
            c_oBdrs.Add(c_o2Bdr);
            c_oBdrs.Add(c_o3Bdr);
            c_oBdrs.Add(c_o4Bdr);
            c_oBdrs.Add(c_o5Bdr);
        }

        private string selectCard()
        {
            string a;
            do
            {
                a = cards[rnd.Next(0, cards.Length - 1)];
            }
            while (selected.Contains(a));

            return a;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            score = 100;
            bet = 10;
            gain = 0;

            tblockScore.Text = "-";
            tblockBet.Text = bet.ToString();
            tblockTotalScore.Text = score.ToString();
            tblockPlot.Text = "0";

            CoreApplication.Properties.TryGetValue("PlayerName", out PlayerName);
            CoreApplication.Properties.TryGetValue("Socket", out sckt);
            CoreApplication.Properties.TryGetValue("Master", out master);
            CoreApplication.Properties.TryGetValue("Opponent", out pName);
            _peerName = (string)pName;
            if (master != null)
            {
                mastercheck = true;
                mainBtn.Visibility = Visibility.Visible;
            }
            else { tblockWait.Visibility = Visibility.Visible; }
            _socket = (StreamSocket) sckt;
            ListenForIncomingMessage();
            
        }


        private DataReader _dataReader;
        private async void ListenForIncomingMessage()
        {
            try
            {
                var message = await GetMessage();
                rcvMssgs += message + "/";
                var x = message.Split('/');
                    if (x[0] == "1") { State_1(x); }
                    else if (x[0] == "2") { State_2(x); }
                    else if (x[0] == "3") { State_3(x); }
                    else if (x[0] == "4") { State_4(x); }
                    else if (x[0] == "5") { State_5(x); }
                    else if (x[0] == "5S") { State_5S(x); }
                    else if (x[0] == "6") { State_6(x); }
                    else if (x[0] == "7") { State_7(x); }
                    else if (x[0] == "8") { State_8(x); }
                    else if (x[0] == "M") { State_M(x); }
                    else if (x[0] == "S") { State_S(x); }
                    else if (x[0] == "F") { State_F(x); }
                    else if (x[0] == "R") { State_R(x); }
                else { ListenForIncomingMessage(); return; }
                ListenForIncomingMessage();
            }
            catch (Exception)
            {
                CloseConnection(false);
            }
        }

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

            
            //var len = await GetMessageSize();

            await _dataReader.LoadAsync(4);
            uint messageLen = (uint)_dataReader.ReadInt32();
            await _dataReader.LoadAsync(messageLen);
            return _dataReader.ReadString(messageLen);
        }

        DataWriter _dataWriter;
        private void SendMessage_Tap_1(object sender, RoutedEventArgs e)
        {
            SendAndWait();
        }

        private async void SendMessage(string message)
        {
            sntMessages += message + "/";
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
                this.Frame.Navigate(typeof(MainPage));
                return;
            }

            if (_dataWriter == null)
                _dataWriter = new DataWriter(_socket.OutputStream);

            _dataWriter.WriteInt32(message.Length);
            await _dataWriter.StoreAsync();

            _dataWriter.WriteString(message);
            await _dataWriter.StoreAsync();

        }

        private void btnRise_Click(object sender, RoutedEventArgs e)
        {
            gridCards.Visibility = Visibility.Collapsed;
            gridRise.Visibility = Visibility.Visible;
            riseBtn.Visibility = Visibility.Collapsed;
            mainBtn.Content = "Rise";
            ready = true;
            if (state == 3) state = 2;
            else if (state == 7) state = 6;
        }

        private void btnFold_Click(object sender, RoutedEventArgs e)
        {
            fld_y = true;
            if (mastercheck) { state = 8; SendMessage("F"); }
            else { state = 9; SendMessage("F"); }
            SendAndWait();
            gridCards.Visibility = Visibility.Visible;
            gridRise.Visibility = Visibility.Collapsed;
            ready = true;
        }

        private string cardSign(string cardString)
        {
            return cardString.Replace("a", "♥").Replace("b", "♦").Replace("c", "♣").Replace("d", "♠");
        }

        private string cardSign_Reverse(string cardString)
        {
            return cardString.Replace( "♥", "a").Replace("♦", "b").Replace("♣", "c").Replace("♠", "d");
        }

        private string cardString(string[] array)
        {
            return "/" + array[0] + "/" + array[1] + "/" +
                            array[2] + "/" + array[3] + "/" + array[4];
        }

        private void checkScore(string[] hand)
        {
            int[] patternArray = Check.patternArray(hand);

            if (Check.OnePair(hand, patternArray)) { tblockScore.Text = "One Pair";  }
            else if (Check.JacksOrBetter(hand, patternArray)) { tblockScore.Text = "Jacks Or Better"; }
            else if (Check.TwoPairs(hand, patternArray)) { tblockScore.Text = "Two Pairs"; }
            else if (Check.ThreeOfAKind(hand, patternArray)) { tblockScore.Text = "Three Of a Kind";;}
            else if (Check.FullHouse(hand, patternArray)) { tblockScore.Text = "Full House";}
            else if (Check.Poker(hand, patternArray)) { tblockScore.Text = "Poker";}
            else if (Check.Flush(hand)) { tblockScore.Text = "Flush";}
            else if (Check.Straight(hand)) { tblockScore.Text = "Straight";}
            else if (Check.Straight(hand) && Check.Flush(selected)) { tblockScore.Text = "Straight Flush";}
            else { tblockScore.Text = "-"; }

            stringScore = tblockScore.Text;

        }

        //Korisničko sučelje: --------------------------------------------------------------------
        private void cardColor()
        {
            card1.Foreground = new SolidColorBrush(Colors.Black);
            card2.Foreground = new SolidColorBrush(Colors.Black);
            card3.Foreground = new SolidColorBrush(Colors.Black);
            card4.Foreground = new SolidColorBrush(Colors.Black);
            card5.Foreground = new SolidColorBrush(Colors.Black);

            var cd1 = card1.Text.Substring(card1.Text.Length - 1);
            var cd2 = card2.Text.Substring(card2.Text.Length - 1);
            var cd3 = card3.Text.Substring(card3.Text.Length - 1);
            var cd4 = card4.Text.Substring(card4.Text.Length - 1);
            var cd5 = card5.Text.Substring(card5.Text.Length - 1);

            if (cd1 == "♥" || cd1 == "♦") card1.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd2 == "♥" || cd2 == "♦") card2.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd3 == "♥" || cd3 == "♦") card3.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd4 == "♥" || cd4 == "♦") card4.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd5 == "♥" || cd5 == "♦") card5.Foreground = new SolidColorBrush(Colors.DarkRed);
        }

        private void card_oColor()
        {
            for (int i = 0; i < 5; i++)
            {
                c_oBdrs[i].Background = new SolidColorBrush(Colors.White);
            }

            card_o1.Foreground = new SolidColorBrush(Colors.Black);
            card_o2.Foreground = new SolidColorBrush(Colors.Black);
            card_o3.Foreground = new SolidColorBrush(Colors.Black);
            card_o4.Foreground = new SolidColorBrush(Colors.Black);
            card_o5.Foreground = new SolidColorBrush(Colors.Black);

            var cd1 = card_o1.Text.Substring(card_o1.Text.Length - 1);
            var cd2 = card_o2.Text.Substring(card_o2.Text.Length - 1);
            var cd3 = card_o3.Text.Substring(card_o3.Text.Length - 1);
            var cd4 = card_o4.Text.Substring(card_o4.Text.Length - 1);
            var cd5 = card_o5.Text.Substring(card_o5.Text.Length - 1);

            if (cd1 == "♥" || cd1 == "♦") card_o1.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd2 == "♥" || cd2 == "♦") card_o2.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd3 == "♥" || cd3 == "♦") card_o3.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd4 == "♥" || cd4 == "♦") card_o4.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd5 == "♥" || cd5 == "♦") card_o5.Foreground = new SolidColorBrush(Colors.DarkRed);
        }

        private void c1Btn_click(object sender, RoutedEventArgs e)
        {
            if (card1_check == false)
            {
                c1Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card1_check = true;
                checkedString[0] = "1"; 
            }
            else
            {
                c1Btn.Background = new SolidColorBrush(Colors.White);
                card1_check = false;
                checkedString[0] = "0"; 
            }
        }

        private void c2Btn_click(object sender, RoutedEventArgs e)
        {
            if (card2_check == false)
            {
                c2Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card2_check = true;
                checkedString[1] = "2"; 
            }
            else
            {
                c2Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false;
                checkedString[1] = "0"; 
            }
        }

        private void c3Btn_click(object sender, RoutedEventArgs e)
        {
            if (card3_check == false)
            {
                c3Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card3_check = true;
                checkedString[2] = "3"; 
            }
            else
            {
                c3Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false;
                checkedString[2] = "0"; 
            }
        }

        private void c4Btn_click(object sender, RoutedEventArgs e)
        {
            if (card4_check == false)
            {
                c4Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card4_check = true;
                checkedString[3] = "4"; 
            }
            else
            {
                c4Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false;
                checkedString[3] = "0"; 
            }
        }

        private void c5Btn_click(object sender, RoutedEventArgs e)
        {
            if (card5_check == false)
            {
                c5Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card5_check = true;
                checkedString[4] = "5"; 
            }
            else
            {
                c5Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false;
                checkedString[4] = "0"; 
            }
        }

        private void riseUpBtn_Click(object sender, RoutedEventArgs e)
        {
            int adder = 10;
            double temp;

            Double.TryParse(tboxRise.Text, out temp);
            tboxRise.Text = (temp + adder).ToString();
        }

        private void riseDwnBtn_Click(object sender, RoutedEventArgs e)
        {
            int subber = 10;
            double temp;

            Double.TryParse(tboxRise.Text, out temp);
            tboxRise.Text = (temp - subber).ToString();
        }

        //Stanja (primanje poruka)---------------------------------------------------------
        private void State_1(string[] x)
        {
            ready = false;
            card1.Text = cardSign(x[1]);
            card2.Text = cardSign(x[2]);
            card3.Text = cardSign(x[3]);
            card4.Text = cardSign(x[4]);
            card5.Text = cardSign(x[5]);
            cardColor();
            state = 2;

            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            riseBtn.Visibility = Visibility.Visible;
            mainBtn.Content = "Check";
            foldBtn.Visibility = Visibility.Visible;

            tblockScore.Text = "-";

            score = score - bet;
            tblockTotalScore.Text = score.ToString();
            plot = 20;
            tblockPlot.Text = plot.ToString();
        }

        private void State_2(string[] x)
        {
            state = 3;
            mainBtn.Content = "Call";

            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            riseBtn.Visibility = Visibility.Visible;
            foldBtn.Visibility = Visibility.Visible;

            Double.TryParse(x[1], out rised);
            Double.TryParse(x[2], out plot);
            tblockPlot.Text = plot.ToString();
            if(x[1] == "0") tblockScore.Text = _peerName + " checked";
            else tblockScore.Text = _peerName + " rised: " + x[1];
        }

        private void State_3(string[] x)//master
        {
            Double.TryParse(x[1], out old_rise);
            mainBtn.Visibility = Visibility.Visible;
            tblockScore.Text = "-";
            mainBtn.Content = "Change cards";
            riseBtn.Visibility = Visibility.Collapsed;
            gridCards.Visibility = Visibility.Visible;
            gridRise.Visibility = Visibility.Collapsed;
            foldBtn.Visibility = Visibility.Collapsed;
            tblockWait.Visibility = Visibility.Collapsed;

            if (mastercheck)
            {
                rised = rised - old_rise;
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();
                old_rise = 0;

                if (card1_check == false) { selected[0] = selectCard(); }
                if (card2_check == false) { selected[1] = selectCard(); }
                if (card3_check == false) { selected[2] = selectCard(); }
                if (card4_check == false) { selected[3] = selectCard(); }
                if (card5_check == false) { selected[4] = selectCard(); }

                for (int i = 0; i < 5; i++) slaveHand[i] = selectCard();

                string cardstring = cardString(slaveHand);
                SendMessage("4" + cardstring + "/" + plot.ToString());
                state = 5;
            }
            else 
            {
                SendMessage("3/"+old_rise.ToString());
                state = 4;
            }
        }

        private void State_4(string[] x)//samo slave je ovdje
        {
            Double.TryParse(x[6],out plot);
            tblockPlot.Text = plot.ToString();
            mainBtn.Visibility = Visibility.Visible;
            foldBtn.Visibility = Visibility.Collapsed;
            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Content = "Change cards";
            state = 4;

            for (int i = 1; i < 6; i++) selected[i] = x[i];
        }

        private void State_5(string[] x)//samo master
        {
            gridOppo.Visibility = Visibility.Visible;
            int br = 0;
            for(int i = 0; i<5; i++)
            {
                if (x[i+1] != "0") { selected[i + 5] = slaveHand[i]; br++;}
            }

            for(int i = 1; i < br+1; i++)
            {
                c_oBdrs[i-1].Background = new SolidColorBrush(Colors.SlateGray);
            }
        }

        private void State_5S(string[] x)//samo slave (primanje broja promjenjenih karata)
        {
            ready = true;
            gridOppo.Visibility = Visibility.Visible;
            int br = 0;
            for (int i = 0; i < 5; i++)
            {
                if (x[i + 1] != "0") { br++; }
            }

            for (int i = 1; i < br + 1; i++)
            {
                c_oBdrs[i - 1].Background = new SolidColorBrush(Colors.SlateGray);
            }
        }

        private void State_6(string[] x)
        {
            if (x.Length == 3) { state = 7; }

            mainBtn.Content = "Call";

            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            riseBtn.Visibility = Visibility.Visible;
            foldBtn.Visibility = Visibility.Visible;

            Double.TryParse(x[1], out rised);
            Double.TryParse(x[2], out plot);
            tblockPlot.Text = plot.ToString();
            if (x[1] == "0") tblockScore.Text = _peerName + " checked";
            else tblockScore.Text = _peerName + " rised: " + x[1];
        }

        private void State_7(string[] x)//samo slave
        {
            Double.TryParse(x[1], out plot);
            tblockPlot.Text = plot.ToString();
            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            mainBtn.Content = "Next";
            string[] hand = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text };
            string handSend = cardSign_Reverse("/" + card1.Text + "/" + card2.Text + "/" + card3.Text + "/"
                                + card4.Text + "/" + card5.Text);
            if (!mastercheck) { slaveMax = Check.Max(hand); SendMessage("M/" + stringScore + "/" + slaveMax.ToString() + handSend); }
            else { masterMax = Check.Max(hand); }

            state = 9;
        }

        private void State_8(string[] x)//samo master
        {
            Double.TryParse(x[1], out old_rise);
            tblockWait.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            mainBtn.Content = "Next";
            string[] hand = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text }; 
            string handSend = cardSign_Reverse("/" + card1.Text + "/" + card2.Text + "/" + card3.Text + "/"
                                 + card4.Text + "/" + card5.Text);
            if (!mastercheck) { slaveMax = Check.Max(hand); SendMessage("M/" + stringScore + "/" + slaveMax.ToString() + handSend); }
            else { masterMax = Check.Max(hand); }
            riseBtn.Visibility = Visibility.Collapsed;
            gridCards.Visibility = Visibility.Visible;
            gridRise.Visibility = Visibility.Collapsed;
            
            if (mastercheck)
            {
                rised = rised - old_rise;
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();
                old_rise = 0;
                state = 8;
            }
        }

        private void State_M(string[] x)//samo master
        {
            foldBtn.Visibility = Visibility.Collapsed;

            string masterScore = stringScore;
            string slaveScore = x[1];

            string handSend = cardSign_Reverse("/" + card1.Text + "/" + card2.Text + "/" + card3.Text + "/"
                               + card4.Text + "/" + card5.Text);

            if (masterScore == "One Pair") masterInt = 1;
            else if (masterScore == "Jacks Or Better") masterInt = 2;
            else if (masterScore == "Two Pairs") masterInt = 3;
            else if (masterScore == "Three Of a Kind") masterInt = 4;
            else if (masterScore == "Straight") masterInt = 5;
            else if (masterScore == "Flush") masterInt = 6;
            else if (masterScore == "Full House") masterInt = 7;
            else if (masterScore == "Poker") masterInt = 8;
            else if (masterScore == "Straight Flush") masterInt = 9;
            else { masterInt = 0; }

            if (slaveScore == "One Pair") slaveInt = 1;
            else if (slaveScore == "Jacks Or Better") slaveInt = 2;
            else if (slaveScore == "Two Pairs") slaveInt = 3;
            else if (slaveScore == "Three Of a Kind") slaveInt = 4;
            else if (slaveScore == "Straight") slaveInt = 5;
            else if (slaveScore == "Flush") slaveInt = 6;
            else if (slaveScore == "Full House") slaveInt = 7;
            else if (slaveScore == "Poker") slaveInt = 8;
            else if (slaveScore == "Straight Flush") slaveInt = 9;
            else { slaveInt = 0; }

            string winner;
            if (masterInt > slaveInt) { tblockScore.Text = "You Win."; winner = "S/m"; }
            else if (slaveInt > masterInt) { tblockScore.Text = _peerName + " Wins.";  winner = "S/s"; }
            else
            {
                if (masterInt == 9 || masterInt == 7 || masterInt == 6 || masterInt == 5 || masterInt == 0)
                {
                    if (masterMax > slaveMax) { tblockScore.Text = "You Win."; winner = "S/m"; }
                    else if (slaveMax > masterMax) { tblockScore.Text = _peerName + " Wins."; winner = "S/s"; }
                    else { tblockScore.Text = "No Winner."; winner = "S/n"; }
                }
                else 
                {
                    string[] master = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text };
                    string[] slave = { x[3], x[4], x[5], x[6], x[7] };
                    winner = "S/";
                    winner +=  Check.Both_have_same(master, slave);
                    if (winner == "S/m") { tblockScore.Text = "You Win.";}
                    else if (winner == "S/s") { tblockScore.Text = _peerName + " Wins."; }
                    else { tblockScore.Text = "No Winner."; }
                }
            }
            
            SendMessage(winner + handSend);

            card_o1.Text = cardSign(x[3]);
            card_o2.Text = cardSign(x[4]);
            card_o3.Text = cardSign(x[5]);
            card_o4.Text = cardSign(x[6]);
            card_o5.Text = cardSign(x[7]);
            card_oColor();
            gridOppo.Visibility = Visibility.Visible;

        }

        private void State_S(string[] x)//samo slave
        {
            foldBtn.Visibility = Visibility.Collapsed;

            if (x[1] == "m") tblockScore.Text = _peerName + " Wins.";
            else if (x[1] == "s") tblockScore.Text = "You Win.";
            else tblockScore.Text = "No Winner.";

            card_o1.Text = cardSign(x[2]);
            card_o2.Text = cardSign(x[3]);
            card_o3.Text = cardSign(x[4]);
            card_o4.Text = cardSign(x[5]);
            card_o5.Text = cardSign(x[6]);
            card_oColor();

            gridOppo.Visibility = Visibility.Visible;

            SendMessage("8/" + old_rise.ToString());
            state = 9;
        }

        private void State_F(string[] x)// primanje folda
        {
            fld_o = true;
            tblockScore.Text = "You Win.";
            if (mastercheck) state = 8;
            else state = 9;
            ready = true;
            SendAndWait();
        }

        private void State_R(string[] x)// Slave prima da je Ready za Deal
        {
            ready = true;
        }

        //Stanja za slanje poruka-------------------------------------------------
        private async void SendAndWait()
        {
            if (state == 1) 
            {
                if (!ready) { return; }
                ready = false;
                    for (int i = 0; i < 10; i++)
                    {
                        selected[i] = selectCard();
                    }

                    card1.Text = cardSign(selected[0]);
                    card2.Text = cardSign(selected[1]);
                    card3.Text = cardSign(selected[2]);
                    card4.Text = cardSign(selected[3]);
                    card5.Text = cardSign(selected[4]);
                    cardColor();

                    for (int i = 0; i < 5; i++)
                    {
                        sHand[i] = selected[i+5];
                    }
                    
                    string cardstring = cardString(sHand);
                    SendMessage("1" + cardstring);
                    score = score - bet;
                    tblockTotalScore.Text = score.ToString();
                    plot = 20;
                    tblockPlot.Text = plot.ToString();

                    state = 2;
                    mainBtn.Visibility = Visibility.Collapsed;
                    tblockWait.Visibility = Visibility.Visible;
                    foldBtn.Visibility = Visibility.Collapsed;
                    tblockScore.Text = "-";
                }
            else if (state == 2)//oba
            {
                Double.TryParse(tboxRise.Text, out rised_tb);
                rised += rised_tb;
                if (rised % 10 != 0 || rised < 0 || rised > score || rised_tb > 200-(score + plot))
                {
                    MessageDialog m = new MessageDialog("Rise not correct.");
                    await m.ShowAsync();
                    tboxRise.Text = "0";
                    rised -= rised_tb;
                    return;
                }
                mainBtn.Visibility = Visibility.Collapsed;
                riseBtn.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;

                score = score - rised;
                tblockTotalScore.Text = score.ToString();
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();
                SendMessage("2/" + rised.ToString() + "/" + plot.ToString());
                old_rise = rised;
                tboxRise.Text = "0";
                gridCards.Visibility = Visibility.Visible;
                gridRise.Visibility = Visibility.Collapsed;
                state = 3;

                tblockWait.Visibility = Visibility.Visible;
            }
            else if(state == 3)//call
            {
                mainBtn.Visibility = Visibility.Visible;
                tblockScore.Text = "-";
                mainBtn.Content = "Change cards";
                riseBtn.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;
                rised = rised - old_rise;
                score = score - rised;
                if(score >= 0) tblockTotalScore.Text = score.ToString();
                else tblockScore.Text = "0";
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();

                if(mastercheck)
                {
                    if (card1_check == false) { selected[0] = selectCard(); }
                    if (card2_check == false) { selected[1] = selectCard(); }
                    if (card3_check == false) { selected[2] = selectCard(); }
                    if (card4_check == false) { selected[3] = selectCard(); }
                    if (card5_check == false) { selected[4] = selectCard(); }

                    for (int i = 0; i < 5; i++) slaveHand[i] = selectCard();
                    string cardstring = cardString(slaveHand);

                    SendMessage("4" + cardstring + "/" + plot.ToString());
                    state = 5;
                }
                else
                {
                    SendMessage("3/"+old_rise.ToString());
                    state = 4;
                }
                old_rise = 0;
            }
            else if (state == 4)//samo slave
            {
                if (card1_check == false) { card1.Text = cardSign(selected[1]); }
                if (card2_check == false) { card2.Text = cardSign(selected[2]); }
                if (card3_check == false) { card3.Text = cardSign(selected[3]); }
                if (card4_check == false) { card4.Text = cardSign(selected[4]); }
                if (card5_check == false) { card5.Text = cardSign(selected[5]); }
                cardColor();

                string temp = "/" + checkedString[0] + "/" + checkedString[1] + "/" + checkedString[2] + "/" +
                    checkedString[3] + "/" + checkedString[4];
                SendMessage("5" + temp);

                string[] hand = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text };
                checkScore(hand);

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);

                mainBtn.Content = "Check";
                mainBtn.Visibility = Visibility.Visible;
                riseBtn.Visibility = Visibility.Visible;
                foldBtn.Visibility = Visibility.Visible;
                rised = 0;
                state = 6;
            }

            else if (state == 5) // samo master
            {
                if (card1_check == false) { card1.Text = cardSign(selected[0]); }
                if (card2_check == false) { card2.Text = cardSign(selected[1]); }
                if (card3_check == false) { card3.Text = cardSign(selected[2]); }
                if (card4_check == false) { card4.Text = cardSign(selected[3]); }
                if (card5_check == false) { card5.Text = cardSign(selected[4]); }
                cardColor();

                string[] hand = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text };
                string temp = "/" + checkedString[0] + "/" + checkedString[1] + "/" + checkedString[2] + "/" +
                    checkedString[3] + "/" + checkedString[4];
                SendMessage("5S" + temp);

                checkScore(hand);

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);


                tblockWait.Visibility = Visibility.Visible;
                mainBtn.Content = "Check";
                mainBtn.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;
                rised = 0;
                state = 6;
            }

            else if (state == 6) // oba
            {
                if (!ready) { return; }
                ready = false;
                Double.TryParse(tboxRise.Text, out rised_tb);
                rised += rised_tb;
                if (rised % 10 != 0 || rised < 0 || rised > score || rised_tb > 200-(score + plot))
                {
                    MessageDialog m = new MessageDialog("Rise not correct.");
                    await m.ShowAsync();
                    tboxRise.Text = "0";
                    rised -= rised_tb;
                    ready = true;
                    return;
                }
                mainBtn.Visibility = Visibility.Collapsed;
                riseBtn.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;
                score = score - rised;
                tblockTotalScore.Text = score.ToString();
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();
                SendMessage("6/" + rised.ToString() + "/" + plot.ToString());
                old_rise = rised;
                tboxRise.Text = "0";
                gridCards.Visibility = Visibility.Visible;
                gridRise.Visibility = Visibility.Collapsed;
                state = 7;

                tblockWait.Visibility = Visibility.Visible;
            }
            else if (state == 7)//call 2
            {
                mainBtn.Visibility = Visibility.Visible;
                mainBtn.Content = "Next";
                
                string[] hand = { card1.Text, card2.Text, card3.Text, card4.Text, card5.Text };
                string handSend = cardSign_Reverse("/" + card1.Text + "/" + card2.Text + "/" + card3.Text + "/"
                                + card4.Text + "/" + card5.Text);
                if (!mastercheck) { slaveMax = Check.Max(hand); SendMessage("M/" + stringScore + "/" + slaveMax.ToString() + handSend); }
                else { masterMax = Check.Max(hand); }

                riseBtn.Visibility = Visibility.Collapsed;
                rised = rised - old_rise;
                score = score - rised;
                if(score >= 0) tblockTotalScore.Text = score.ToString();
                else tblockScore.Text = "0";
                plot = plot + rised;
                tblockPlot.Text = plot.ToString();

                if (mastercheck)
                {
                    SendMessage("7" + "/" + plot.ToString());
                    state = 8;
                }
                old_rise = 0;

            }
            else if (state == 8)//master u slave
            {
                rised = 0;
                old_rise = 0;
                Double.TryParse(tblockPlot.Text, out plot);
                if (tblockScore.Text == "You Win.")
                { tblockPlot.Text = "0"; score += plot; tblockTotalScore.Text = score.ToString(); plot = 0; }
                else if (tblockScore.Text == "No Winner.")
                { tblockPlot.Text = "0"; score += (plot/2); tblockTotalScore.Text = score.ToString(); plot = 0; }
                else
                { tblockPlot.Text = "0"; plot = 0; }

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);

                tblockScore.Text = "-";

                if (!fld_o && !fld_y) { SendMessage("R"); }
                fld_y = false;
                if (fld_o) { tblockScore.Text = _peerName + " Folded."; }
                fld_o = false;

                card1.Text = "";
                card2.Text = "";
                card3.Text = "";
                card4.Text = "";
                card5.Text = "";

                card_o1.Text = "";
                card_o2.Text = "";
                card_o3.Text = "";
                card_o4.Text = "";
                card_o5.Text = "";

                for (int i = 0; i < 5; i++) checkedString[i] = "0";

                mainBtn.Visibility = Visibility.Collapsed;
                riseBtn.Visibility = Visibility.Collapsed;
                gridOppo.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;

                state = 1;
                if (mastercheck)
                {
                    mastercheck = false;
                }
                else
                {
                    mastercheck = true;
                }

                if (score <= 0 || score >= 200) GameOver();
            }

            else if (state == 9)//slave u master
            {
                rised = 0;
                old_rise = 0;

                Double.TryParse(tblockPlot.Text, out plot);
                if (tblockScore.Text == "You Win.")
                { tblockPlot.Text = "0"; score += plot; tblockTotalScore.Text = score.ToString(); plot = 0; }
                else if (tblockScore.Text == "No Winner.")
                { tblockPlot.Text = "0"; score += (plot / 2); tblockTotalScore.Text = score.ToString(); plot = 0; }
                else
                { tblockPlot.Text = "0"; plot = 0; }

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);

                tblockScore.Text = "-";
                if (fld_o) { tblockScore.Text = _peerName + " Folded."; }
                fld_o = false;
                fld_y = false;
                card1.Text = "";
                card2.Text = "";
                card3.Text = "";
                card4.Text = "";
                card5.Text = "";

                card_o1.Text = "";
                card_o2.Text = "";
                card_o3.Text = "";
                card_o4.Text = "";
                card_o5.Text = "";

                for (int i = 0; i < 5; i++) checkedString[i] = "0";

                mainBtn.Visibility = Visibility.Visible;
                mainBtn.Content = "Deal";
                riseBtn.Visibility = Visibility.Collapsed;
                gridOppo.Visibility = Visibility.Collapsed;
                foldBtn.Visibility = Visibility.Collapsed;
                tblockWait.Visibility = Visibility.Collapsed;

                state = 1;
                if (mastercheck)
                {
                    mastercheck = false;
                }
                else
                {
                    mastercheck = true;
                }

                if (score <= 0 || score >= 200) GameOver();
            }
        }

        public void GameOver()
        {
            if (score == 0) { tblockGOver.Text = "You lost!\n Game Over."; }
            else
            { tblockGOver.Text = "You Won!\n Congratulations."; }
            mainBtn.Visibility = Visibility.Collapsed;
            gridCards.Visibility = Visibility.Collapsed;
            gridGOver.Visibility = Visibility.Visible;
        }

        private void btnGame_Over(object sender, RoutedEventArgs e)
        {
            CloseConnection(false);
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
