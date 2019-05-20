using System;
using System.Collections.Generic;
using System.Timers;

namespace Peppa
{
    public class WXMessagePolling
    {
        private Queue<dynamic> wx_message = new Queue<dynamic>();

        private Timer timer = null;
        private Timer post_timer = null;

        private Action<string, string, Action<bool>> messageHandler = null;

        public WXMessagePolling(Action<string, string, Action<bool>> handler)
        {
            this.messageHandler = handler;
            Token.LoginCompleted = WXLoginCompleted;
            Token.LogoutCompleted = WXLogoutCompleted;

        }

        private void PollStart()
        {
            PollEnd();
            timer = new Timer();
            timer.Interval = 100;
            timer.Elapsed += Timer_Tick;
            timer.Start();
        }

        private void PostPollStart()
        {
            PostPollEnd();
            post_timer = new Timer();
            post_timer.Interval = 100;
            post_timer.Elapsed += Post_timer_Tick;
            post_timer.Start();
        }

        private void Post_timer_Tick(object sender, EventArgs e)
        {

            if (!Token.IsLogin)
            {
                Token.Logout();
                return;
            }

            if (wx_message.Count == 0) return;
            PeppaUtils.Debug($"正在处理消息，还剩{wx_message.Count}条未处理");
            post_timer.Stop();
            try
            {
                dynamic msgItem = wx_message.Dequeue();
                string text = msgItem.Content.ToString();
                string toUserName = msgItem.FromUserName.ToString();
                messageHandler?.Invoke(text, toUserName, (res) =>
                {
                    post_timer.Start();
                });
            }
            catch (Exception ex)
            {
                PeppaUtils.Debug(ex);
                post_timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!Token.IsLogin)
            {
                Token.Logout();
                return;
            }

            //PeppaUtils.Debug($"正在检查微信消息...");
            timer.Stop();
            try
            {
                CheckWXMessage((has) =>
                {
                    if (has)
                    {
                        GetWXMessage((res) =>
                        {
                            timer.Start();
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
                PeppaUtils.Debug(exce);
                timer.Start();
            }
        }

        private string BuildSyncKeys()
        {
            string syncKey = "";
            foreach (var item in Token.sync_key)
            {
                if (!string.IsNullOrEmpty(syncKey)) syncKey += "|";
                syncKey += item.Key + "_" + item.Value;
            }
            return syncKey;
        }

        private void GetContact(Action<bool> action)
        {
            var postData = new
            {
                BaseRequest = new
                {
                    Uin = Token.wxuin,
                    Sid = Token.wxsid,
                    Skey = Token.skey,
                    DeviceID = PeppaUtils.GetDeviceID()
                },
                Count = 2,
                List = new List<object>
                {
                    new
                    {
                        UserName="@@e203c90e07d64242336d573acd4a1db1c01ccb92ca05410444a3003c282c21db",
                        EncryChatRoomId=""
                    },
                    new
                    {
                        UserName="@ba194a63575be7fd0f4ea4e4547b2942d0dccc7fffc00ef5a6d141f93adb7739",
                        EncryChatRoomId=""
                    }
                }
            };
            RestApi.NewInstance(Method.GET)
            .SetUrl("https://wx2.qq.com/cgi-bin/mmwebwx-bin/webwxgetcontact")
            .AddQueryParameter("pass_ticket", Token.pass_ticket)
            .AddQueryParameter("type", "ex")
            .AddJsonBody(postData)
            .AddCookie(Token.cookieObjects)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    List<dynamic> users = new List<dynamic>();
                    List<dynamic> allContact = new List<dynamic>();
                    var data = restApi.To<dynamic>();
                    foreach (var item in data.MemberList)
                    {
                        allContact.Add(item);
                        if (((int)item.VerifyFlag & 8) != 0)
                        {
                            continue; //过滤公众号
                        }
                        if (item.UserName.ToString().StartsWith("@@"))
                        {
                            continue; //过滤群
                        }
                        users.Add(item);
                    }
                    Token.users = users;
                    action(true);
                }
                catch (Exception exce)
                {
                    PeppaUtils.Debug(exce);
                    action(false);
                }
            });
        }

        private void CheckWXMessage(Action<bool> action)
        {
            string syncKey = BuildSyncKeys();
            if (string.IsNullOrEmpty(syncKey))
            {
                action(false);
                return;
            }

            RestApi.NewInstance(Method.GET)
            .SetUrl("https://webpush.wx2.qq.com/cgi-bin/mmwebwx-bin/synccheck")
            .AddQueryParameter("deviceid", PeppaUtils.GetDeviceID())
            .AddQueryParameter("sid", Token.wxsid)
            .AddQueryParameter("skey", Token.skey)
            .AddQueryParameter("uin", Token.wxuin)
            .AddQueryParameter("synckey", syncKey)
            .AddQueryParameter("_", PeppaUtils.GetUnixDateTimeNow())
            .AddQueryParameter("r", PeppaUtils.GetUnixDateTimeNow())
            .AddCookie(Token.cookieObjects)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    string data = restApi.To<String>();
                    PeppaUtils.Debug(data);
                    string jsondata = data.Replace("window.synccheck=", "");
                    var jdata = DynamicJson.DeserializeObject<dynamic>(jsondata);
                    if (jdata.retcode.ToString() == "1101")
                    {
                        PeppaUtils.Debug("微信已退出");
                        Token.Logout();
                    }
                    else if (jdata.retcode.ToString() == "0" && jdata.selector.ToString() == "2")
                    {
                        PeppaUtils.Debug("检测到有新消息 Bot");
                        action(true);
                    }
                    else
                    {
                        PeppaUtils.Debug("未检测到新消息");
                        action(false);
                    }
                }
                catch (Exception exec)
                {
                    PeppaUtils.Debug(exec);
                    action(false);
                }
            });
        }

        private void GetWXMessage(Action<bool> action)
        {
            var postdata = new
            {
                BaseRequest = new
                {
                    Uin = Token.wxuin,
                    Sid = Token.wxsid,
                    Skey = Token.skey,
                    DeviceID = PeppaUtils.GetDeviceID()
                },
                SyncKey = Token.sync_key_object,
                rr = ~PeppaUtils.GetUnixDateTimeNow()
            };

            RestApi.NewInstance(Method.POST)
            .SetUrl("https://wx2.qq.com/cgi-bin/mmwebwx-bin/webwxsync")
            .AddQueryParameter("lang", "zh_CN")
            .AddQueryParameter("pass_ticket", Token.pass_ticket)
            .AddQueryParameter("sid", Token.wxsid)
            .AddQueryParameter("skey", Token.skey)
            .AddCookie(Token.cookieObjects)
            .AddJsonBody(postdata)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    var data = restApi.To<dynamic>();
                    Token.sync_key_object = data.SyncKey;
                    int count = 0;
                    foreach (var item in data.AddMsgList)
                    {
                        if (item.MsgType != 1)
                        {
                            continue;        //过滤非文本消息
                        }
                        string fromUserName = item.FromUserName.ToString();
                        if (string.IsNullOrEmpty(item.Content.ToString()) || fromUserName == Token.user_name)
                        {
                            continue;        //过滤掉文本为空，或者自己发送的消息
                        }
                        if (!Token.IsGroupAuto)
                        {
                            if (item.ToUserName.ToString().StartsWith("@@")
                            || item.FromUserName.ToString().StartsWith("@@"))
                            {
                                continue;    //过滤群消息
                            }
                        }

                        if (!Token.IsAuto(fromUserName))
                        {
                            continue;        //未配置AI回复
                        }

                        wx_message.Enqueue(item);
                        count++;
                    }
                    PeppaUtils.Debug("接收到文本消息总数:" + count);
                    action(true);
                }
                catch (Exception exec)
                {
                    PeppaUtils.Debug(exec);
                    action(false);
                }
            });
        }

        public void SendWXMessage(string message, string toUserName, Action<bool> action)
        {
            var res_data = new
            {
                BaseRequest = new
                {
                    Uin = Token.wxuin,
                    Sid = Token.wxsid,
                    Skey = Token.skey,
                    DeviceID = PeppaUtils.GetDeviceID()
                },
                Msg = new
                {
                    Type = 1,
                    Content = message,
                    FromUserName = Token.user_name,
                    ToUserName = toUserName,
                    LocalID = PeppaUtils.GetUnixDateTimeNow(),
                    ClientMsgId = PeppaUtils.GetUnixDateTimeNow(),
                },
                Scene = 0
            };

            Token.LastToUserName = toUserName;
            RestApi.NewInstance(Method.POST)
            .SetUrl("https://wx2.qq.com/cgi-bin/mmwebwx-bin/webwxsendmsg")
            .AddQueryParameter("lang", "zh_CN")
            .AddQueryParameter("pass_ticket", Token.pass_ticket)
            .AddCookie(Token.cookieObjects)
            .AddJsonBody(res_data)
            .ExecuteAsync((res, ex, restApi) =>
            {
                try
                {
                    var ret = restApi.To<dynamic>();
                    if (ret.BaseResponse.Ret == 0)
                    {
                        PeppaUtils.Debug("发送消息成功");
                        action(true);
                    }
                    else
                    {
                        PeppaUtils.Debug("发送消息失败");
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

        private void PollEnd()
        {
            try
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer = null;
                }
            }
            catch (Exception exce)
            {
                PeppaUtils.Debug(exce);
            }
        }

        private void PostPollEnd()
        {
            try
            {
                if (post_timer != null)
                {
                    post_timer.Stop();
                    post_timer = null;
                }
            }
            catch (Exception exce)
            {
                PeppaUtils.Debug(exce);
            }
        }

        private void Dispose()
        {
            PeppaUtils.Debug("停止轮询微信消息");
            PollEnd();
            PostPollEnd();
            wx_message.Clear();
        }

        public void WXLoginCompleted()
        {
            PeppaUtils.Debug("开始轮询微信消息...");
            PollStart();
            PostPollStart();
            GetContact((res) => { });
        }

        public void WXLogoutCompleted()
        {
            Dispose();
        }
    }
}
