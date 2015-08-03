using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Windows;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MPlayer : Page
    {
        object PNCheck;
        public MPlayer()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
        }

        private async void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(PlayerName.Text))
            {
                var msg = new MessageDialog("Enter Player Name.");
                await msg.ShowAsync();
            }
            else
            {
                // Note: The chat name is not passed in as part of the query string, since it
                // is stored in ApplicationSettings and that is accessible from every page.
                if(CoreApplication.Properties.TryGetValue("PlayerName", out PNCheck))
                {
                    CoreApplication.Properties.Remove("PlayerName");
                }
                CoreApplication.Properties.Add("PlayerName", PlayerName.Text);
                this.Frame.Navigate(typeof(Lobby));
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
