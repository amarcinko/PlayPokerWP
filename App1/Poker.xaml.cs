using App1.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI;
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
    public sealed partial class Poker : Page
    {
        string[] cards = { "A♥", "2♥", "3♥","4♥","5♥","6♥","7♥","8♥","9♥","10♥","J♥","Q♥","K♥",
                             "A♣", "2♣", "3♣","4♣","5♣","6♣","7♣","8♣","9♣","10♣","J♣","Q♣","K♣",
                             "A♦", "2♦", "3♦","4♦","5♦","6♦","7♦","8♦","9♦","10♦","J♦","Q♦","K♦",
                              "A♠", "2♠", "3♠","4♠","5♠","6♠","7♠","8♠","9♠","10♠","J♠","Q♠","K♠"  };

        string[] selected = new string[5];
        bool click;
        Random rnd = new Random();
        double gain, bet, score, tempScore, netoGain;

        bool card1_check = false;
        bool card2_check = false;
        bool card3_check = false;
        bool card4_check = false;
        bool card5_check = false;

        SQLiteAsyncConnection conn = new SQLiteAsyncConnection("Top10.db");
        double lastScore;
        bool endCheck;

        public Poker()
        {
            this.InitializeComponent();
            LoadDatabase();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void LoadDatabase()
        {
            await conn.CreateTableAsync<Player>();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            gridSave.Visibility = Visibility.Collapsed;
            gridCards.Visibility = Visibility.Visible;
            gridDouble.Visibility = Visibility.Collapsed;
            click = true;

            score = 100;
            bet = 10;
            gain = 0;
            netoGain = 0;

            lastScore = 0;
            endCheck = false;

            tblockScore.Text = "-";
            tboxBet.Text = "10";
            tblockBet.Text = bet.ToString();
            tblockGain.Text = gain.ToString();
            tblockNetoGain.Text = netoGain.ToString();
            tblockTotalScore.Text = score.ToString();

            mainBtn.Visibility = Visibility.Visible;
            mainBtn.Content = "New";
            mainBtn.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x81, 0x2e, 0x2e));
            backBtn.Visibility = Visibility.Collapsed;
            card1.Text = "";
            card2.Text = "";
            card3.Text = "";
            card4.Text = "";
            card5.Text = "";

            
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

        private void checkScore()
        {
             int[] patternArray = Check.patternArray(selected);

             if (Check.JacksOrBetter(selected, patternArray)) { tblockScore.Text = "Jacks Or Better"; gain = bet * 1.1; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; }
             else if (Check.Blaze(selected, patternArray)) { tblockScore.Text = "Blaze"; gain = bet * 2.5; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.TwoPairs(selected, patternArray)) { tblockScore.Text = "Two Pairs"; gain = bet * 2; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.ThreeOfAKind(selected, patternArray)) { tblockScore.Text = "Three Of a Kind"; gain = bet * 3; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.Flush(selected)) { tblockScore.Text = "Flush"; gain = bet * 9; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible;
                                                 if (Check.Straight(selected)) { tblockScore.Text = "Straight Flush"; gain = bet * 50; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible;}}
             else if (Check.Straight(selected)) { tblockScore.Text = "Straight"; gain = bet * 6; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.CornerStraight(selected)) { tblockScore.Text = "Corner Straight"; gain = bet * 6; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.KangarooStraight(selected)) { tblockScore.Text = "Kangaroo Straight"; gain = bet * 8; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.FullHouse(selected, patternArray)) { tblockScore.Text = "Full House"; gain = bet * 10; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else if (Check.Poker(selected, patternArray)) { tblockScore.Text = "Poker"; gain = bet * 25; tempScore = score; score += gain; BtnDouble.Visibility = Visibility.Visible; ;}
             else { gain = 0; tblockScore.Text = "-"; }

      	    netoGain = gain - bet;

            tblockBet.Text = bet.ToString();
            tblockGain.Text = gain.ToString();
            tblockNetoGain.Text = netoGain.ToString();
            tblockTotalScore.Text = score.ToString();
        }

        private void cardColor()
        {
            card1.Foreground = new SolidColorBrush(Colors.Black);
            card2.Foreground = new SolidColorBrush(Colors.Black);
            card3.Foreground = new SolidColorBrush(Colors.Black);
            card4.Foreground = new SolidColorBrush(Colors.Black);
            card5.Foreground = new SolidColorBrush(Colors.Black);

            var cd1 = selected[0].Substring(selected[0].Length-1);
            var cd2 = selected[1].Substring(selected[1].Length-1);
            var cd3 = selected[2].Substring(selected[2].Length-1);
            var cd4 = selected[3].Substring(selected[3].Length-1);
            var cd5 = selected[4].Substring(selected[4].Length-1);

            if (cd1 == "♥" || cd1 == "♦") card1.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd2 == "♥" || cd2 == "♦") card2.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd3 == "♥" || cd3 == "♦") card3.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd4 == "♥" || cd4 == "♦") card4.Foreground = new SolidColorBrush(Colors.DarkRed);
            if (cd5 == "♥" || cd5 == "♦") card5.Foreground = new SolidColorBrush(Colors.DarkRed);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (click == false)
            {

                if(card1_check == false) {selected[0] = selectCard();}
                if(card2_check == false) {selected[1] = selectCard();}
                if(card3_check == false) {selected[2] = selectCard();}
                if(card4_check == false) {selected[3] = selectCard();}
                if(card5_check == false) {selected[4] = selectCard();}

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);

                card1.Text = selected[0];
                card2.Text = selected[1];
                card3.Text = selected[2];
                card4.Text = selected[3];
                card5.Text = selected[4];

                cardColor();
	    	    checkScore();

	    	    if(score <= 0)
	             {
	         	    tblockScore.Text = "Game Over";
                    mainBtn.Visibility = Visibility.Collapsed;
                    backBtn.Visibility = Visibility.Visible;
	             }

	    	    mainBtn.Content = "New";
                mainBtn.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x81, 0x2e, 0x2e));
                click = true;
            }
            else {
                if(score <= 0)
	             {
	         	     var messageDialog = new MessageDialog("Game Over.");
                     await messageDialog.ShowAsync(); 
	         	     return;
	             }
                Double.TryParse(tboxBet.Text, out bet);
                if(bet > score || bet % 1 != 0 || bet < 0)
	             {
                     var messageDialog = new MessageDialog("Change your bet.");
                     await messageDialog.ShowAsync();
                     return;
	             }
                BtnDouble.Visibility = Visibility.Collapsed;

                card1_check = false; c1Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false; c2Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false; c3Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false; c4Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false; c5Btn.Background = new SolidColorBrush(Colors.White);

                for (int i = 0; i < 5; i++)
                {
                    selected[i] = selectCard();
                }

                tblockScore.Text = "-";
                score = score - bet;
                gain = 0;
                netoGain = 0;

                tblockBet.Text = bet.ToString();
                tblockGain.Text = gain.ToString();
                tblockNetoGain.Text = netoGain.ToString();
                tblockTotalScore.Text = score.ToString();

                card1.Text = selected[0];
                card2.Text = selected[1];
                card3.Text = selected[2];
                card4.Text = selected[3];
                card5.Text = selected[4];

                cardColor();

                mainBtn.Content = "Next";
                mainBtn.Background = new SolidColorBrush(Color.FromArgb(0xff, 0x4d, 0xb8, 0x70));
                click = false;
            }
        }

        private void c1Btn_click(object sender, RoutedEventArgs e)
        {
            if (click == true) return;
            if (card1_check == false)
            {
                c1Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card1_check = true;
            }
            else 
            {
                c1Btn.Background = new SolidColorBrush(Colors.White);
                card1_check = false;
            }  
        }

        private void c2Btn_click(object sender, RoutedEventArgs e)
        {
            if (click == true) return;
            if (card2_check == false)
            {
                c2Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card2_check = true;
            }
            else
            {
                c2Btn.Background = new SolidColorBrush(Colors.White);
                card2_check = false;
            }
        }

        private void c3Btn_click(object sender, RoutedEventArgs e)
        {
            if (click == true) return;
            if (card3_check == false)
            {
                c3Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card3_check = true;
            }
            else
            {
                c3Btn.Background = new SolidColorBrush(Colors.White);
                card3_check = false;
            }
        }

        private void c4Btn_click(object sender, RoutedEventArgs e)
        {
            if (click == true) return;
            if (card4_check == false)
            {
                c4Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card4_check = true;
            }
            else
            {
                c4Btn.Background = new SolidColorBrush(Colors.White);
                card4_check = false;
            }
        }

        private void c5Btn_click(object sender, RoutedEventArgs e)
        {
            if (click == true) return;
            if (card5_check == false)
            {
                c5Btn.Background = new SolidColorBrush(Colors.SlateGray);
                card5_check = true;
            }
            else
            {
                c5Btn.Background = new SolidColorBrush(Colors.White);
                card5_check = false;
            }
        }

        private void betUpBtn_Click(object sender, RoutedEventArgs e)
        { 
            int adder;
            double temp;

            if (score <= 10) adder = 1;
            else if (score <= 500) adder = 10;
            else if (score > 500 && score <= 5000) adder = 100;
            else if (score > 5000 && score <= 50000) adder = 1000;
            else adder = 10000;
                
            Double.TryParse(tboxBet.Text, out temp);
            tboxBet.Text = (temp + adder).ToString();
        }

        private void betDwnBtn_Click(object sender, RoutedEventArgs e)
        {
            int suber;
            double temp;

            if (score <= 10) suber = 1;
            else if (score <= 500) suber = 10;
            else if (score > 500 && score <= 5000) suber = 100;
            else if (score > 5000 && score <= 50000) suber = 1000;
            else suber = 10000;

            Double.TryParse(tboxBet.Text, out temp);
            tboxBet.Text = (temp - suber).ToString();
        }

        private void btnDouble_Click(object sender, RoutedEventArgs e)
        {
            gridCards.Visibility = Visibility.Collapsed;
            gridDouble.Visibility = Visibility.Visible;
            mainBtn.Visibility = Visibility.Collapsed;
            BtnDouble.Visibility = Visibility.Collapsed;
            tblockScore.Text = "-";
            card1.Text = "";
            card2.Text = "";
            card3.Text = "";
            card4.Text = "";
            card5.Text = "";
            Array.Clear(selected,0,5);
            score = score - gain;
            tblockTotalScore.Text = score.ToString();
        }

        private void btnHigh_Click(object sender, RoutedEventArgs e)
        {
            string hl = selectCard();
            hlCard.Text = hl;
            int hl_int;
    	    hl = hl.Remove(hl.Length - 1);
    	    if (hl == "J") hl_int = 11;
		    else if (hl == "Q") hl_int = 12;
		    else if (hl == "K") hl_int = 13;
		    else if (hl == "A") hl_int = 1;
            else { Int32.TryParse(hl, out hl_int); }

            if (hl_int > 7)
            {
                gain = gain * 2; netoGain = gain - bet; tblockScore.Text = "High";
                btnSaveHalf.Visibility = Visibility.Visible;
            }
            else if (hl_int == 7) { tblockScore.Text = "-"; }
            else { gain = 0; netoGain = 0; score = tempScore; tblockScore.Text = "Low";
            btnHigh.Visibility = Visibility.Collapsed; btnLow.Visibility = Visibility.Collapsed;
            btnSaveHalf.Visibility = Visibility.Collapsed;
            }
            tblockGain.Text = gain.ToString();
            tblockNetoGain.Text = netoGain.ToString();
        }

        private void btnLow_Click(object sender, RoutedEventArgs e)
        {
            string hl = selectCard();
            hlCard.Text = hl;
            int hl_int;
            hl = hl.Remove(hl.Length - 1);

            if (hl == "J") hl_int = 11;
            else if (hl == "Q") hl_int = 12;
            else if (hl == "K") hl_int = 13;
            else if (hl == "A") hl_int = 1;
            else { Int32.TryParse(hl, out hl_int); }

            if (hl_int < 7)
            {
                gain = gain * 2; netoGain = gain - bet; tblockScore.Text = "Low";
                btnSaveHalf.Visibility = Visibility.Visible;
            }
            else if (hl_int == 7) { tblockScore.Text = "-"; }
            else { gain = 0; netoGain = 0; score = tempScore; tblockScore.Text = "High";
            btnHigh.Visibility = Visibility.Collapsed; btnLow.Visibility = Visibility.Collapsed;
            btnSaveHalf.Visibility = Visibility.Collapsed;
            }
            tblockGain.Text = gain.ToString();
            tblockNetoGain.Text = netoGain.ToString();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            gridCards.Visibility = Visibility.Visible;
            gridDouble.Visibility = Visibility.Collapsed;
            mainBtn.Visibility = Visibility.Visible;
            BtnDouble.Visibility = Visibility.Collapsed;
            btnHigh.Visibility = Visibility.Visible; 
            btnLow.Visibility = Visibility.Visible;
            btnSaveHalf.Visibility = Visibility.Collapsed;
            tblockScore.Text = "-";
            
            score += gain;
            tblockTotalScore.Text = score.ToString();
            hlCard.Text = "";
            if (score <= 0)
            {
                tblockScore.Text = "Game Over";
                mainBtn.Visibility = Visibility.Collapsed;
                backBtn.Visibility = Visibility.Visible;
            }
        }

        private void btnSaveHalf_Click(object sender, RoutedEventArgs e)
        {
            gain = gain / 2;
            score += gain;
            tempScore = score;
            tblockTotalScore.Text = score.ToString();
            tblockGain.Text = gain.ToString(); 
            hlCard.Text = "";
            btnSaveHalf.Visibility = Visibility.Collapsed;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            if(!endCheck)
            {
                endCheck = true;
                mainBtn.Visibility = Visibility.Collapsed;
                BtnDouble.Visibility = Visibility.Collapsed;

                var q = conn.Table<Player>().OrderByDescending(x => x.PlayerScore).Take(10);
                int numPl = q.CountAsync().Result;
                if (numPl != 0)
                {
                    var top10 = await q.ToListAsync();
                    top10.Reverse();
                    lastScore = top10.First().PlayerScore;
                }

                if (score > lastScore || numPl < 10)
                {
                    gridDouble.Visibility = Visibility.Collapsed;
                    gridCards.Visibility = Visibility.Collapsed;
                    gridSave.Visibility = Visibility.Visible;
                }
                else
                {
                    Frame.Navigate(typeof(MainPage));
                }
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
            
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(tboxPlayerName.Text == "")
            {
                var messageDialog = new MessageDialog("Write your name.");
                await messageDialog.ShowAsync();
                return;
            }
            else
            {
                Player nPl = new Player ()
                {
                    Name = tboxPlayerName.Text,
                    PlayerScore = score
                };
                await conn.InsertAsync(nPl);
                this.Frame.Navigate(typeof(MainPage));
            }
        }
        
    }
}
