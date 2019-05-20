using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
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
    /// WXLogin.xaml 的交互逻辑
    /// </summary>
    public partial class WXLogin : BaseWindow
    {
        private int scanQRCode = 0;
        private DispatcherTimer timer = null;

        public WXLogin()
        {
            InitializeComponent();
            this.Loaded += WXLogin_Loaded;
            this.Closed += WXLogin_Closed;
        }

        private void WXLogin_Closed(object sender, EventArgs e)
        {
            this.PollEnd();
        }

        private void GetUUID(Action<bool, string> action)
        {
            RestApi.NewInstance(Method.GET)
            .SetUrl("https://wx.qq.com/jslogin")
            .AddQueryParameter("appid", "wx782c26e4c19acffb")
            .AddQueryParameter("fun", "new")
            .AddQueryParameter("lang", "zh_CN")
            .AddQueryParameter("_", DateTime.Now.ToString())
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    string data = restApi.To<string>();
                    string key = "window.QRLogin.uuid";
                    int index = data.IndexOf(key);
                    string tmp = data.Substring(index + key.Length + 2);
                    string uuid = tmp.Replace("\"", "").Replace(";", "").Replace(" ", "");
                    Token.uuid = uuid;
                    PeppaUtils.Debug(uuid);
                    action(true, uuid);
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false, exce.Message);
                }
            });
        }

        private void Poll(string uuid, Action<bool, string> action)
        {
            RestApi.NewInstance(Method.GET)
            .SetUrl("https://login.wx.qq.com/cgi-bin/mmwebwx-bin/login")
            .AddQueryParameter("tip", scanQRCode)
            .AddQueryParameter("uuid", uuid)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    string data = restApi.To<String>();
                    PeppaUtils.Debug(data);
                    if (data.Contains("window.code=201"))
                    {
                        scanQRCode = 1;
                        action(false, null);
                    }
                    else if (data.Contains("window.code=200"))
                    {
                        string key = "window.redirect_uri";
                        int index = data.IndexOf(key);
                        string tmp = data.Substring(index + key.Length + 2);
                        string uri = tmp.Replace("\"", "").Replace(";", "").Replace(" ", "") + "&fun=new";
                        //PeppaUtils.Debug(uri);
                        action(true, uri);
                    }
                    else
                    {
                        action(false, null);
                    }
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false, exce.Message);
                }
            });
        }

        private void GetWXToken(string uri, Action<bool> action)
        {
            PeppaUtils.Debug(uri);
            RestApi.NewInstance(Method.GET)
            .SetUrl(uri)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    string response = restApi.To<String>();
                    PeppaUtils.Debug(response);
                    var resObj = DynamicJson.DeserializeXmlObject<dynamic>(response);
                    Token.skey = resObj.error.skey.ToString();
                    Token.wxsid = resObj.error.wxsid.ToString();
                    Token.wxuin = resObj.error.wxuin.ToString();
                    Token.pass_ticket = resObj.error.pass_ticket.ToString();
                    var cookies = restApi.GetCookie();
                    if (cookies != null && cookies.Count > 0)
                    {
                        Token.cookieObjects = cookies;
                        foreach (var item in cookies)
                        {
                            Token.cookies[item.Name] = item.Value;
                        }
                    }
                    action(true);
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false);
                }
            });
        }

        private void WebWXInit(Action<bool> action)
        {
            var postBody = new
            {
                BaseRequest = new
                {
                    Uin = Token.wxuin,
                    Sid = Token.wxsid,
                    Skey = Token.skey,
                    DeviceID = PeppaUtils.GetDeviceID()
                }
            };
            RestApi.NewInstance(Method.POST)
            .SetUrl("https://wx2.qq.com/cgi-bin/mmwebwx-bin/webwxinit")
            .AddQueryParameter("r", ~PeppaUtils.GetUnixDateTimeNow())
            .AddQueryParameter("pass_ticket", Token.pass_ticket)
            .AddQueryParameter("skey", Token.skey)
            .AddCookie(Token.webwx_auth_ticket_key, Token.webwx_auth_ticket)
            .AddCookie(Token.webwx_data_ticket_key, Token.webwx_data_ticket)
            .AddCookie(Token.webwxuvid_key, Token.webwxuvid)
            .AddJsonBody(postBody)
            .ExecuteAsync((res3, ex3, restApi3) =>
            {
                try
                {
                    var initData = restApi3.To<dynamic>();
                    if (initData.BaseResponse.Ret == 0)
                    {
                        Token.init_data = initData;
                        Token.sync_key_object = initData.SyncKey;
                        action(true);
                    }
                    else
                    {
                        action(false);
                    }
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false);
                }
            });
        }

        private void PollBegin()
        {
            PollEnd();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            PeppaUtils.Debug("正在检测扫码登录状态中...");
            timer.Stop();
            try
            {
                Poll(Token.uuid, (res, val) =>
                {
                    if (res)
                    {
                        PeppaUtils.Debug("登录成功，开始获取token");
                        PollEnd();
                        GetWXToken(val, (tokened) =>
                        {
                            if (tokened)
                            {
                                PeppaUtils.Debug("获取token成功，开始初始化...");
                                WebWXInit((inited) =>
                                {
                                    PeppaUtils.Debug("微信初始化结果：" + inited);
                                    if (inited)
                                    {
                                        PeppaUtils.BeginInvoke(() => { Close(); });
                                    }
                                });
                            }
                        });
                    }
                    else
                    {
                        timer.Start();
                    }
                });
            }
            catch (Exception exce)
            {
                Debug.WriteLine(exce.Message);
                timer.Start();
            }
        }

        private void PollEnd()
        {
            try
            {
                PeppaUtils.Debug("停止登录状态轮询");
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
            }
            catch
            {

            }
        }

        private void WebWXLogin()
        {
            GetUUID((res, val) =>
            {
                if (res)
                {
                    PeppaUtils.BeginInvoke(() =>
                    {
                        loginQRCode.Source = new BitmapImage(new Uri("https://login.wx.qq.com/qrcode/" + val, UriKind.Absolute));
                        scanQRCode = 0;
                        PeppaUtils.Debug("获取QRCode成功，开始轮询登录状态...");
                        PollBegin();
                    });
                }
            });
        }
        
        private void WXLogin_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= WXLogin_Loaded;
            WebWXLogin();
        }
       
    }
}
