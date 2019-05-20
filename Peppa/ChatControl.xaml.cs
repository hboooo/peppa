using Peppa.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Peppa
{
    /// <summary>
    /// ChatControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChatControl : UserControl
    {

        public ObservableCollection<ChatMessage> MessageItems
        {
            get { return (ObservableCollection<ChatMessage>)GetValue(MessageItemsProperty); }
            set { SetValue(MessageItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessageItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageItemsProperty =
            DependencyProperty.Register("MessageItems", typeof(ObservableCollection<ChatMessage>), typeof(ChatControl), new PropertyMetadata(null));


        public Visibility TooltipVisibility
        {
            get { return (Visibility)GetValue(TooltipVisibilityProperty); }
            set { SetValue(TooltipVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TootipVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TooltipVisibilityProperty =
            DependencyProperty.Register("TooltipVisibility", typeof(Visibility), typeof(ChatControl), new PropertyMetadata(Visibility.Collapsed));


        public Visibility PhotoVisibility
        {
            get { return (Visibility)GetValue(PhotoVisibilityProperty); }
            set { SetValue(PhotoVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PhotoVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PhotoVisibilityProperty =
            DependencyProperty.Register("PhotoVisibility", typeof(Visibility), typeof(ChatControl), new PropertyMetadata(Visibility.Visible));



        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ChatControl), new PropertyMetadata(null));


        private BitmapImage _robotHeader = null;
        private BitmapImage _userHeader = null;
        private Brush _left = new SolidColorBrush(Color.FromRgb(152, 225, 101));
        private Brush _right = new SolidColorBrush(Color.FromRgb(245, 245, 245));

        public static String BotUID = "LJ6525995561521152";
        public static String BotWXUID = "LJWX6525995561521152";
        public static String TestUID = "DEBUG6525991488847872";

        private WXMessagePolling wxMessagePolling = null;

        public ChatControl()
        {
            InitializeComponent();
            this.Loaded += ChatControl_Loaded;
            Init();
            tbMessage.Focus();
            wxMessagePolling = new WXMessagePolling(MessageHandler);
        }

        private void MessageHandler(string message, string toUserName, Action<bool> action)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(toUserName))
            {
                action(false);
                return;
            }
            bool isFinished = false;
            PeppaUtils.BeginInvoke(() =>
            {
                AddMessageItem(message, FlowDirection.LeftToRight);
                isFinished = true;
            });
            PeppaApp.UiThreadAlive(ref isFinished);

            PostToAIPoll(message, BotWXUID, (res, responseMsg) =>
            {
                if (res)
                {
                    PeppaUtils.BeginInvoke(() =>
                    {
                        AddMessageItem(responseMsg, FlowDirection.RightToLeft);
                    });
                    wxMessagePolling.SendWXMessage(responseMsg, toUserName, action);
                }
                else
                {
                    action(false);
                }
            });
        }

        private void ChatControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ChatControl_Loaded;
        }

        private void Init()
        {
            _robotHeader = new BitmapImage(new Uri("pack://application:,,,/Resources/robot_peppa.jpg", UriKind.RelativeOrAbsolute));
            _userHeader = new BitmapImage(new Uri("pack://application:,,,/Resources/robot_peppa.jpg", UriKind.RelativeOrAbsolute));

            if (MessageItems == null)
            {
                MessageItems = new ObservableCollection<ChatMessage>();
            }

            Hello();
        }

        public void Hello()
        {
            AddMessageItem("小姐姐，我满腹经纶博古通今，来撩我鸭", FlowDirection.LeftToRight);
        }

        private void AddMessageItem(string message, FlowDirection direction, int type = 0)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.FlowDir = direction;
            if (direction == FlowDirection.LeftToRight)
            {
                chatMessage.HeadImage = _robotHeader;
                chatMessage.FlowDir = FlowDirection.LeftToRight;
                chatMessage.BackColor = _left;
            }
            else if (direction == FlowDirection.RightToLeft)
            {
                chatMessage.HeadImage = _userHeader;
                chatMessage.FlowDir = FlowDirection.RightToLeft;
                chatMessage.BackColor = _right;
            }
            chatMessage.Message = message;
            MessageItems.Add(chatMessage);
        }

        public void Clean()
        {
            if (MessageItems != null)
            {
                MessageItems.Clear();
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Message) || string.IsNullOrEmpty(Message.Trim()))
            {
                TooltipVisibility = Visibility.Visible;
                return;
            }
            MusicPlayer.MessageAlarm();
            TooltipVisibility = Visibility.Collapsed;
            if (Token.IsLogin)
            {
                PostWXChart(Message, Token.LastToUserName);
            }
            else
            {
                PostChat(Message);
            }
            Message = "";
        }

        private void PostToAIPoll(string message, string uid, Action<bool, string> action)
        {
            int index = 0;
            PostToAI(index, message, uid, action);
        }

        private void PostToAI(int index, string message, string uid, Action<bool, string> action)
        {
            if (index >= TulingBot.Bot.Count)
            {
                action(false, null);
                return;
            }
#if DEBUG
            string ai_uid = TestUID;
#else
            string ai_uid = uid;
#endif
            string api_key = null;
            if (TulingBot.ExceedLimit.Contains(TulingBot.Bot[index]))
            {
                PostToAI(++index, message, uid, action);
                return;
            }
            else
            {
                api_key = TulingBot.Bot[index];
            }

            var postData = new
            {
                perception = new
                {
                    inputText = new
                    {
                        text = message
                    }
                },
                userInfo = new
                {
                    apiKey = api_key,
                    userId = ai_uid,
                }
            };

            RestApi.NewInstance(Method.POST)
            .SetUrl("http://openapi.tuling123.com/openapi/api/v2")
            .AddJsonBody(postData)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    dynamic data = restApi.To<object>();
                    string content = null;
                    if (!TulingBot.Error.Contains(Convert.ToInt32(data.intent.code)))
                    {
                        foreach (var item in data.results)
                        {
                            content = item.values.text.ToString();
                        }
                    }
                    else
                    {
                        if (data.intent.code == 4003)
                        {
                            TulingBot.ExceedLimit.Add(TulingBot.Bot[index]);
                        }
                        PeppaUtils.Debug($"Tuling AI bot [{TulingBot.Bot[index]}] error. code:{data.intent.code.ToString()}");
                    }
                    if (!string.IsNullOrEmpty(content))
                    {
                        action(true, content);
                    }
                    else
                    {
                        PostToAI(++index, message, uid, action);
                    }
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false, null);
                }
            });
        }

        private void PostWXChart(string message, string toUserName)
        {
            string toUser = toUserName;
            if (string.IsNullOrEmpty(toUser))
            {
                ContactSelectorWindow contactSelectorWindow = new ContactSelectorWindow();
                bool? res = contactSelectorWindow.ShowDialog();
                if (res == true)
                {
                    toUser = contactSelectorWindow.SelectToUser;
                }
            }
            if (string.IsNullOrWhiteSpace(toUser))
            {
                return;
            }

            AddMessageItem(message, FlowDirection.RightToLeft);
            wxMessagePolling.SendWXMessage(message, toUser, (res) =>
            {
                PeppaUtils.Debug("Peppa 发送消息到微信.结果:" + res);
            });
        }

        private void PostChat(string message)
        {
            AddMessageItem(message, FlowDirection.RightToLeft);
            PostToAIPoll(message, BotUID, (res, msg) =>
            {
                if (res)
                {
                    PeppaUtils.BeginInvoke(() => { AddMessageItem(msg, FlowDirection.LeftToRight); });
                }
            });
        }

        private void BuildResponseMessage(dynamic item)
        {
            if (item.resultType == "text")
            {
                //TextBlock textBlock = new TextBlock();
                //textBlock.TextWrapping = TextWrapping.Wrap;
                //textBlock.Text = item.values.text.ToString();
                AddMessageItem(item.values.text.ToString(), FlowDirection.LeftToRight);
            }
            else if (item.resultType == "url")
            {
                TextBlock textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                string url = item.values.url.ToString();
                Hyperlink link = new Hyperlink(new Run(url));
                link.NavigateUri = new Uri(url);
                textBlock.Inlines.Add(link);
            }
            else if (item.resultType == "voice")
            {

            }
            else if (item.resultType == "video")
            {

            }
            else if (item.resultType == "image")
            {
                string imageuri = item.values.image;
                Image image = new Image();
                image.Source = new BitmapImage(new Uri(imageuri));
                image.Stretch = Stretch.Uniform;
                image.Width = 150;
                image.Height = 150;
                image.MouseLeftButtonDown += Image_MouseLeftButtonDown;
            }
            else if (item.resultType == "news")
            {
                string output = string.Empty;
                int index = 1;
                WrapPanel wp = new WrapPanel();
                foreach (var child in item.values.news)
                {
                    string title = child.name;
                    string source = child.info;
                    string url = child.detailurl;
                    string img = child.icon;

                    index++;
                }
            }
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                var img = sender as Image;
                string strUrl = img.Source.ToString();
                Process.Start(new ProcessStartInfo(strUrl));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
