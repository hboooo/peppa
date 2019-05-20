using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Peppa
{
    public class BaseWindow : ModernWindow
    {
        public Visibility MaskVisibility
        {
            get { return (Visibility)GetValue(MaskVisibilityProperty); }
            set { SetValue(MaskVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskVisibilityProperty =
            DependencyProperty.Register("MaskVisibility", typeof(Visibility), typeof(BaseWindow), new PropertyMetadata(Visibility.Collapsed));


        public string MaskIntensiveContent
        {
            get { return (string)GetValue(MaskIntensiveContentProperty); }
            set { SetValue(MaskIntensiveContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskIntensiveContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskIntensiveContentProperty =
            DependencyProperty.Register("MaskIntensiveContent", typeof(string), typeof(BaseWindow), new PropertyMetadata(""));



        public string MaskContent
        {
            get { return (string)GetValue(MaskContentProperty); }
            set { SetValue(MaskContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaskContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaskContentProperty =
            DependencyProperty.Register("MaskContent", typeof(string), typeof(BaseWindow), new PropertyMetadata(""));


        public string PrintLabelCount
        {
            get { return (string)GetValue(PrintLabelCountProperty); }
            set { SetValue(PrintLabelCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrintLabelCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrintLabelCountProperty =
            DependencyProperty.Register("PrintLabelCount", typeof(string), typeof(BaseWindow), new PropertyMetadata("0"));


        public BitmapImage LabelImageSource
        {
            get { return (BitmapImage)GetValue(LabelImageSourceProperty); }
            set { SetValue(LabelImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelImageSourceProperty =
            DependencyProperty.Register("LabelImageSource", typeof(BitmapImage), typeof(BaseWindow), new PropertyMetadata(null));


        public string NickName
        {
            get { return (string)GetValue(NickNameProperty); }
            set { SetValue(NickNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NickName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NickNameProperty =
            DependencyProperty.Register("NickName", typeof(string), typeof(BaseWindow), new PropertyMetadata(""));


        public BaseWindow()
        {
            this.Style = (Style)Application.Current.Resources["baseWindow"];
            this.LogoData = Application.Current.Resources["peppaLogo"].ToString();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape && this.MaskVisibility != Visibility.Visible)
            {
                this.Close();
            }
        }
    }
}
