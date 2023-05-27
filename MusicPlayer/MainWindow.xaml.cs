using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using WpfAnimatedGif;
using Path = System.IO.Path;
using Application = System.Windows.Application;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _save.Filter = "playlist files(*.pfe)|*.pfe";
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
            Sound.Value = _player.Volume;
            MemoryReader();
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            if(_player.Source is not null && _player.NaturalDuration.HasTimeSpan)
            {
                Duration.Minimum = 0;
                Duration.Maximum = _player.NaturalDuration.TimeSpan.TotalSeconds;
                Duration.Value = _player.Position.TotalSeconds;
                ToPass.Text = TimeSpan.FromSeconds(_player.NaturalDuration.TimeSpan.TotalSeconds).ToString(@"hh\:mm\:ss");
                if(TracksList.SelectedIndex < TracksList.Items.Count - 1 && _player.Position == _player.NaturalDuration)
                {
                    Next_Click(null, null);
                }
            }

        }

        OpenFileDialog _ofd = new();

        SaveFileDialog _save = new();
        private List<string> _paths = new();
        private MediaPlayer _player = new();
        private DispatcherTimer _timer = new();
        private void AddTrack_Click(object sender, RoutedEventArgs e)
        {
            _ofd.Multiselect = true;
            _ofd.Filter = "mp3 files(*.mp3)|*.mp3|cda files(*.cda)|*cda|m4a files(*.m4a)|" +
            "flac files(*.flac)|mp4 files(*.mp4)|wav files(*.wav)|wma files(*.wma)|aac files (*.aac)";
            if(!_ofd.ShowDialog().HasValue) return;
            foreach(string path in _ofd.FileNames)
            {
                _paths.Add(path);
            }
            foreach(string path in _ofd.SafeFileNames)
            {
                TracksList.Items.Add(path);
            }
        }

        private void TracksList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _player.Open(new Uri(_paths[TracksList.SelectedIndex], UriKind.RelativeOrAbsolute));
            _player.Play();           
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TracksList.Items.Clear();
            _paths.Clear();
            _player.Stop();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (TracksList.SelectedItem is not null)
            {
                _paths.RemoveAt(TracksList.SelectedIndex);
                TracksList.Items.Remove(TracksList.SelectedItem);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_save.ShowDialog() == false) return;
            StreamWriter save = new(_save.FileName);
            foreach (var path in _paths)
            {
               save.WriteLine(path);
            }
            MessageBox.Show("mission passed respect++");
            save.Close();
            
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            _ofd.Multiselect = false;
            _ofd.Filter = "playlist files(*.pfe)|*.pfe";
            if (!_ofd.ShowDialog().HasValue) return;
            Clear_Click(sender, e);
            using (StreamReader reader = new StreamReader(_ofd.FileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    _paths.Add(line);
                    TracksList.Items.Add(Path.GetFileName(line));
                }
            }
        }
        private void Connect(string style)
        {
            Uri uri = new Uri(style, UriKind.Relative);
            ResourceDictionary res = (ResourceDictionary)Application.LoadComponent(uri);
            if (Application.Current.Resources.MergedDictionaries.Count > 1) Application.Current.Resources.MergedDictionaries.RemoveAt(Application.Current.Resources.MergedDictionaries.Count - 1);
            Application.Current.Resources.MergedDictionaries.Add(res);
        }
        private void Default_Checked(object sender, RoutedEventArgs e)
        {
            if (Default.IsChecked == true) Connect("Styles/Default.xaml");
        }

        private void Matrix_Checked(object sender, RoutedEventArgs e)
        {
            if (Matrix.IsChecked == true) Connect("Styles/Matrix.xaml");
        }

        private void FireFox_Checked(object sender, RoutedEventArgs e)
        {
            if (FireFox.IsChecked == true) Connect("Styles/FireFox.xaml");
        }

        private void VisualStudio_Checked(object sender, RoutedEventArgs e)
        {
            if (VisualStudio.IsChecked == true) Connect("Styles/visualStuStyle.xaml");
        }

        private void Play_Click(object sender, RoutedEventArgs e) => _player.Play();

        private void Stop_Click(object sender, RoutedEventArgs e) => _player?.Stop();

        private void TracksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _player.Open(new Uri(_paths[TracksList.SelectedIndex], UriKind.RelativeOrAbsolute));
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Expand_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;

        private void Hide_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Duration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Passed.Text = TimeSpan.FromSeconds(Duration.Value).ToString(@"hh\:mm\:ss");
        }

        private void Sound_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Volume = Sound.Value;
            SoundLevel.Text = Math.Round(Sound.Value * 100) + "%";
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _player.Volume += e.Delta > 0 ? 0.01 : -0.01;
            Sound.Value = _player.Volume;
        }

        private void Duration_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _player.Position = TimeSpan.FromSeconds((e.GetPosition(Duration).X * Duration.Maximum) / Duration.ActualWidth);

        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (TracksList.SelectedIndex < TracksList.Items.Count - 1)
            {
                TracksList.SelectedIndex++;
                _player.Play();
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (TracksList.SelectedIndex > 0)
            {
                TracksList.SelectedIndex--;
                _player.Play();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            DriveInfo drive = DriveInfo.GetDrives()[0];
            string path = drive.Name + @"\Player";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if(!dirInfo.Exists) dirInfo.Create();
            string filename = drive.Name + @"\Player\tmp.dat";
            File.WriteAllLines(filename, _paths);
            Close();
        }
        public void MemoryReader()
        {
            DriveInfo drive = DriveInfo.GetDrives()[0];
            string path = drive.Name + @"\Player\tmp.dat";
            try
            {
                using(FileStream fs = File.OpenRead(path))
                {
                    string line;
                    StreamReader file = new StreamReader(path);
                    while((line = file.ReadLine()) is not null)
                    {
                        _paths.Add(line);
                        TracksList.Items.Add(Path.GetFileName(line));
                    }
                    file.Close();
                }
            }
            catch (Exception) { }
        }
    }
}
