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

namespace Peppa
{
    /// <summary>
    /// ContactWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ContactWindow : BaseWindow
    {
        public ContactWindow()
        {
            InitializeComponent();
        }

        public void RefreshUsers()
        {
            if (Token.users != null)
            {
                List<WXUser> items = new List<WXUser>();
                foreach (var u in Token.users)
                {
                    string username = u.UserName.ToString();
                    items.Add(new WXUser(username, u.NickName.ToString(), u.Signature.ToString(), Token.IsUserBot(username)));
                }
                this.contactList.ItemsSource = items;
            }
            this.groupAuto.IsChecked = Token.IsGroupAuto;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.contactList.ItemsSource != null)
            {
                Token.CleanUserBot();
                HashSet<string> userBot = new HashSet<string>();
                foreach (WXUser item in this.contactList.ItemsSource)
                {
                    if (item.IsBot)
                    {
                        userBot.Add(item.UserName);
                    }
                }
                Token.botUsers = userBot;
            }
            Token.IsGroupAuto = this.groupAuto.IsChecked.Value;

            this.Close();
        }
    }

    public class WXUser
    {
        public WXUser(string username, string nickname, string signature, bool isbot = false)
        {
            this.UserName = username;
            this.NickName = nickname;
            this.Signature = signature;
            this.IsBot = isbot;
        }

        public string UserName { get; set; }

        public string NickName { get; set; }

        public bool IsBot { get; set; }

        public string Signature { get; set; }
    }
}
