using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    /// Photo.xaml 的交互逻辑
    /// </summary>
    public partial class Photo : UserControl
    {
        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(Photo), new PropertyMetadata(null));

        private Storyboard _storyBoard = null;

        public Photo()
        {
            InitializeComponent();
            _storyBoard = this.Resources["heartAnimation"] as Storyboard;
            Thread thread = new Thread(() =>
            {
                int second = new Random().Next(3, 15);
                Thread.Sleep(second * 1000);
                PeppaUtils.BeginInvoke(() => { _storyBoard.Begin(this); });
            });
            thread.Start();
        }
    }
}
