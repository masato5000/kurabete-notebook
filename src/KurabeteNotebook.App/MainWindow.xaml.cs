using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using KurabeteNotebook.Core.Models;
using KurabeteNotebook.Core.Services;
using KurabeteNotebook.Data.Repositories;
using Microsoft.Win32;

namespace KurabeteNotebook.App;

public partial class MainWindow : Window
{
    private readonly VideoRepository _videoRepo = new(App.DbConnection);
    private readonly CaseRepository _caseRepo = new(App.DbConnection);
    private readonly CaseFieldRepository _fieldRepo = new(App.DbConnection);

    private Video? _currentVideo;
    private Case? _selectedCase;
    private bool _isPlaying;
    private bool _seekBarDragging;
    private readonly DispatcherTimer _timer;

    // ViewModel wrapper for DataGrid
    private class CaseRow
    {
        public int Id { get; set; }
        public string TimestampDisplay { get; set; } = "";
        public string OutcomeDisplay { get; set; } = "";
        public string Memo { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public MainWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += Timer_Tick;
    }

    // ── Video Controls ────────────────────────────────────────────────

    private void OpenBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "動画ファイル (*.mp4;*.mkv;*.avi)|*.mp4;*.mkv;*.avi|すべてのファイル|*.*",
            Title = "動画を選択"
        };
        if (dlg.ShowDialog() != true) return;

        var path = dlg.FileName;
        var title = Path.GetFileNameWithoutExtension(path);

        _currentVideo = _videoRepo.Insert(title, path);
        CurrentVideoLabel.Text = _currentVideo.Title;

        VideoPlayer.Source = new Uri(path);
        VideoPlayer.Play();
        _isPlaying = true;
        PlayPauseBtn.Content = "⏸ 停止";
        PlayPauseBtn.IsEnabled = true;
        SeekBar.IsEnabled = true;
        _timer.Start();

        LoadCaseList();
    }

    private void PlayPauseBtn_Click(object sender, RoutedEventArgs e) => TogglePlayPause();

    private void TogglePlayPause()
    {
        if (_isPlaying) { VideoPlayer.Pause(); _isPlaying = false; PlayPauseBtn.Content = "▶ 再生"; }
        else { VideoPlayer.Play(); _isPlaying = true; PlayPauseBtn.Content = "⏸ 停止"; }
    }

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
            SeekBar.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _isPlaying = false;
        PlayPauseBtn.Content = "▶ 再生";
        _timer.Stop();
    }

    private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => _seekBarDragging = true;

    private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _seekBarDragging = false;
        VideoPlayer.Position = TimeSpan.FromSeconds(SeekBar.Value);
    }

    private void SeekBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_seekBarDragging)
            VideoPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (VideoPlayer.NaturalDuration.HasTimeSpan && !_seekBarDragging)
        {
            SeekBar.Value = VideoPlayer.Position.TotalSeconds;
            TimeLabel.Text = VideoPlayer.Position.ToString(@"hh\:mm\:ss");
        }
    }

    // ── Keyboard Shortcuts ────────────────────────────────────────────

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // Don't capture keys if a text box is focused
        if (Keyboard.FocusedElement is TextBox) return;

        switch (e.Key)
        {
            case Key.D: CreateCase(); break;
            case Key.G: SetOutcome(Outcome.Good); break;
            case Key.B: SetOutcome(Outcome.Bad); break;
            case Key.N: SetOutcome(Outcome.Neutral); break;
        }
    }

    // ── Case Operations ───────────────────────────────────────────────

    private void CreateCase()
    {
        if (_currentVideo == null)
        {
            MessageBox.Show("先に動画を開いてください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var ts = VideoPlayer.Position.TotalSeconds;
        var c = _caseRepo.Insert(_currentVideo.Id, ts);
        _selectedCase = c;
        LoadCaseList();
        RefreshLectureCard();
        MessageBox.Show($"ケース #{c.Id} を作成しました（{VideoPlayer.Position:hh\\:mm\\:ss}）", "ケース作成");
    }

    private void SetOutcome(Outcome outcome)
    {
        if (_selectedCase == null) return;
        _selectedCase.Outcome = outcome;
        _caseRepo.UpdateOutcome(_selectedCase.Id, outcome);
        LoadCaseList();
        RefreshLectureCard();
    }

    private void SaveFields_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedCase == null)
        {
            MessageBox.Show("ケースを選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        _fieldRepo.Upsert(_selectedCase.Id, "場面", FieldScene.Text);
        _fieldRepo.Upsert(_selectedCase.Id, "判断", FieldDecision.Text);
        _caseRepo.UpdateMemo(_selectedCase.Id, FieldMemo.Text);
        _selectedCase.Memo = FieldMemo.Text;
        LoadCaseList();
        RefreshLectureCard();
    }

    private void OutcomeGood_Click(object sender, RoutedEventArgs e) => SetOutcome(Outcome.Good);
    private void OutcomeBad_Click(object sender, RoutedEventArgs e) => SetOutcome(Outcome.Bad);
    private void OutcomeNeutral_Click(object sender, RoutedEventArgs e) => SetOutcome(Outcome.Neutral);

    // ── Case List ─────────────────────────────────────────────────────

    private void LoadCaseList()
    {
        if (_currentVideo == null) return;
        var cases = _caseRepo.GetByVideo(_currentVideo.Id).ToList();
        CaseList.ItemsSource = cases.Select(c => new CaseRow
        {
            Id = c.Id,
            TimestampDisplay = TimeSpan.FromSeconds(c.TimestampSeconds).ToString(@"hh\:mm\:ss"),
            OutcomeDisplay = c.Outcome switch
            {
                Outcome.Good => "✓ 良い",
                Outcome.Bad => "✗ 悪い",
                Outcome.Neutral => "△ 中立",
                _ => "未設定",
            },
            Memo = c.Memo,
            CreatedAt = c.CreatedAt,
        }).ToList();
    }

    private void CaseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CaseList.SelectedItem is not CaseRow row) return;
        var cases = _caseRepo.GetByVideo(_currentVideo!.Id).ToList();
        _selectedCase = cases.FirstOrDefault(c => c.Id == row.Id);
        if (_selectedCase == null) return;

        _selectedCase.Fields = _fieldRepo.GetByCase(_selectedCase.Id).ToList();

        FieldScene.Text = _selectedCase.Fields.FirstOrDefault(f => f.Key == "場面")?.Value ?? "";
        FieldDecision.Text = _selectedCase.Fields.FirstOrDefault(f => f.Key == "判断")?.Value ?? "";
        FieldMemo.Text = _selectedCase.Memo;

        VideoPlayer.Position = TimeSpan.FromSeconds(_selectedCase.ClipStartSeconds > 0 ? _selectedCase.ClipStartSeconds : 0);

        RefreshLectureCard();
    }

    private void RefreshLectureCard()
    {
        if (_selectedCase == null) { LectureCard.Text = "（ケースを選択してください）"; return; }
        _selectedCase.Fields = _fieldRepo.GetByCase(_selectedCase.Id).ToList();
        LectureCard.Text = LectureCardService.Generate(_selectedCase);
        LectureCard.Foreground = System.Windows.Media.Brushes.Black;
    }

    // ── Comparison View ───────────────────────────────────────────────

    private void CompareBtn_Click(object sender, RoutedEventArgs e)
    {
        var win = new ComparisonWindow();
        win.Show();
    }
}
