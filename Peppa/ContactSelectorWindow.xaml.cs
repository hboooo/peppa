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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Peppa
{
    /// <summary>
    /// ContactSelectorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ContactSelectorWindow : BaseWindow
    {
        public string SelectToUser { get; set; }

        public ContactSelectorWindow()
        {
            InitializeComponent();
            RefreshContact();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.contactList.ItemsSource != null)
            {
                foreach (WXUser item in this.contactList.ItemsSource)
                {
                    if (item.IsBot)
                    {
                        SelectToUser = item.UserName;
                        break;
                    }
                }
            }
            this.DialogResult = true;
        }

        public void RefreshContact()
        {
            if (Token.users != null)
            {
                List<WXUser> items = new List<WXUser>();
                foreach (var u in Token.users)
                {
                    string username = u.UserName.ToString();
                    items.Add(new WXUser(username, u.NickName.ToString(), u.Signature.ToString(), Token.LastToUserName == username));
                }
                this.contactList.ItemsSource = items;
            }
        }

        private void TxtKey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Start();
        }

        private DispatcherTimer timer = null;

        private void QuickSearch()
        {
            string key = txtKey.Text;
            if (String.IsNullOrEmpty(key))
            {
                RefreshContact();
            }
            else
            {
                List<WXUser> items = new List<WXUser>();
                foreach (var u in Token.users)
                {
                    string nickname = u.NickName.ToString();
                    string username = u.UserName.ToString();
                    if (nickname.Contains(key))
                    {
                        items.Add(new WXUser(username, nickname, u.Signature.ToString(), Token.LastToUserName == username));
                    }
                }
                this.contactList.ItemsSource = items;
            }
        }

        private void Start()
        {
            Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(350);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                QuickSearch();
            }
            catch (Exception ex)
            {
                PeppaUtils.Debug(ex);
            }
            finally
            {
                Stop();
            }
        }

        private void Stop()
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
            }
            catch (Exception ex)
            {
                PeppaUtils.Debug(ex);
            }
        }
    }
}
