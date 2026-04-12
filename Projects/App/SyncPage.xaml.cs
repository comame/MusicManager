using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MusicManager; 
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
            serverStatus.Foreground = AsSolidColorBrush("MediumVioletRed");
            return;
        }

        srv = new SyncServer(library!);
        srv.Listen();
        serverStartButton.Content = "’âŽ~";
        serverStatus.Text = "‹N“®’†";
        serverStatus.Foreground = AsSolidColorBrush("LightSeaGreen");
    }
    private static Brush AsSolidColorBrush(string colorName) {
        var color = System.Drawing.Color.FromName(colorName);
        return new SolidColorBrush(Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B));
    }
}
