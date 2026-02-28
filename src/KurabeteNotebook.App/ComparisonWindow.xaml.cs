using System.Windows;
using Microsoft.Win32;

namespace KurabeteNotebook.App;

public partial class ComparisonWindow : Window
{
    public ComparisonWindow() => InitializeComponent();

    private void OpenA_Click(object sender, RoutedEventArgs e) => OpenVideo(p => PlayerA.Source = p);
    private void OpenB_Click(object sender, RoutedEventArgs e) => OpenVideo(p => PlayerB.Source = p);
    private void PlayA_Click(object sender, RoutedEventArgs e) => PlayerA.Play();
    private void PauseA_Click(object sender, RoutedEventArgs e) => PlayerA.Pause();
    private void PlayB_Click(object sender, RoutedEventArgs e) => PlayerB.Play();
    private void PauseB_Click(object sender, RoutedEventArgs e) => PlayerB.Pause();
    private void SyncPlay_Click(object sender, RoutedEventArgs e) { PlayerA.Play(); PlayerB.Play(); }
    private void SyncPause_Click(object sender, RoutedEventArgs e) { PlayerA.Pause(); PlayerB.Pause(); }

    private void OpenVideo(Action<Uri> setSource)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "動画ファイル (*.mp4;*.mkv;*.avi)|*.mp4;*.mkv;*.avi|すべてのファイル|*.*"
        };
        if (dlg.ShowDialog() == true)
            setSource(new Uri(dlg.FileName));
    }
}
