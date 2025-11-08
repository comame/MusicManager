using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MusicManager.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MusicManager {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SyncPage : Page {
        private MusicLibrary? library = null;

        public SyncPage() {
            InitializeComponent();

            library = MusicIndexer.LoadFromIndexFile();
        }


        private SyncServer? srv;
        private void StartButton_Click(object sender, RoutedEventArgs e) {
            if (srv != null) {
                srv.Stop();
                srv = null;
                serverStartButton.Content = "‹N“®";
                serverStatus.Text = "’âŽ~’†";
                return;
            }

            srv = new SyncServer(library!);
            srv.Listen();
            serverStartButton.Content = "’âŽ~";
            serverStatus.Text = "‹N“®’†";
        }
    }
}
