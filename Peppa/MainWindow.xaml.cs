using Peppa.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Peppa
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        private MusicPlayer _musicPlay = new MusicPlayer();

        private bool? _isPause = null;
        private bool _isPhotoDisplay = true;

        private Storyboard _storyBoard = null;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            _storyBoard = Application.Current.Resources["musicAnimation"] as Storyboard;
            this.CommandBindings.Add(new CommandBinding(MenuCommandExtender.MusicMenuCommand, new ExecutedRoutedEventHandler(MusicMenuCommand)));
            this.CommandBindings.Add(new CommandBinding(MenuCommandExtender.LoginMenuCommand, new ExecutedRoutedEventHandler(LoginMenuCommand)));
            this.CommandBindings.Add(new CommandBinding(MenuCommandExtender.PhotoMenuCommand, new ExecutedRoutedEventHandler(PhotoMenuCommand)));
            this.CommandBindings.Add(new CommandBinding(MenuCommandExtender.ContactMenuCommand, new ExecutedRoutedEventHandler(ContactMenuCommand)));
            Token.MainWindow = this;
        }

        protected void MusicMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            Path path = null;
            Button bu = e.OriginalSource as Button;
            if (bu != null)
            {
                path = bu.Template.FindName("musicPath", bu) as Path;
            }

            if (_isPause == null)
            {
                _musicPlay.Play();
                if (path != null)
                    _storyBoard?.Begin(path, true);
                _isPause = false;
            }
            else if (_isPause == true)
            {
                _musicPlay.Play();
                _isPause = false;
                if (path != null)
                    _storyBoard?.Resume(path);
            }
            else if (_isPause == false)
            {
                _musicPlay.Pause();
                _isPause = true;
                if (path != null)
                    _storyBoard?.Pause(path);
            }

        }

        protected void LoginMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            WXLogin login = new WXLogin();
            login.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            login.Show();
        }

        protected void PhotoMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isPhotoDisplay)
            {
                this.chat.PhotoVisibility = Visibility.Collapsed;
                _isPhotoDisplay = false;
            }
            else
            {
                this.chat.PhotoVisibility = Visibility.Visible;
                _isPhotoDisplay = true;
            }
        }

        protected void ContactMenuCommand(object sender, ExecutedRoutedEventArgs e)
        {
            ContactWindow c = new ContactWindow();
            c.Show();
            c.RefreshUsers();
        }
    }
}