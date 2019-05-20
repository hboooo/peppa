using RestSharp;
using System;
using System.Collections.Generic;

namespace Peppa
{
    public static class Token
    {
        public static string uuid { get; set; }

        public static string deviceId { get; set; }

        public static string skey { get; set; }

        public static string wxsid { get; set; }

        public static string wxuin { get; set; }

        public static string pass_ticket { get; set; }

        public static string webwx_data_ticket_key = "webwx_data_ticket";

        public static string webwx_auth_ticket_key = "webwx_auth_ticket";

        public static string webwxuvid_key = "webwxuvid";

        public static string webwxuvid
        {
            get
            {
                if (cookies.ContainsKey(webwxuvid_key))
                {
                    return cookies[webwxuvid_key];
                }
                return null;
            }
        }

        public static string webwx_data_ticket
        {
            get
            {
                if (cookies.ContainsKey(webwx_data_ticket_key))
                {
                    return cookies[webwx_data_ticket_key];
                }
                return null;
            }
        }

        public static string webwx_auth_ticket
        {
            get
            {
                if (cookies.ContainsKey(webwx_auth_ticket_key))
                {
                    return cookies[webwx_auth_ticket_key];
                }
                return null;
            }
        }

        public static string nick_name
        {
            get
            {
                if (init_data != null)
                {
                    return init_data.User.NickName.ToString();
                }
                return "";
            }
        }

        public static string user_name
        {
            get
            {
                if (init_data != null)
                {
                    return init_data.User.UserName.ToString();
                }
                return "";
            }
        }

        public static Dictionary<string, string> sync_key
        {
            get
            {
                Dictionary<string, string> keys = new Dictionary<string, string>();
                if (sync_key_object != null)
                {
                    foreach (var item in sync_key_object.List)
                    {
                        keys[item.Key.ToString()] = item.Val.ToString();
                    }
                }
                return keys;
            }
        }

        public static dynamic sync_key_object { get; set; }


        public static Dictionary<string, string> cookies = new Dictionary<string, string>();

        public static IList<RestResponseCookie> cookieObjects = null;

        private static dynamic _init_data;
        public static dynamic init_data
        {
            get { return _init_data; }
            set
            {
                _init_data = value;
                if (_init_data != null)
                {
                    uuid = null;
                    PeppaUtils.BeginInvoke(() => { MainWindow.NickName = nick_name; });
                    LoginCompleted?.Invoke();
                }
            }
        }

        public static List<dynamic> users = null;
        public static List<dynamic> allContact = null;

        public static HashSet<string> botUsers = null;

        public static bool IsUserBot(string username)
        {
            if (botUsers != null && botUsers.Count > 0)
            {
                if (botUsers.Contains(username))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAuto(string username)
        {
            if (botUsers == null || botUsers.Count == 0)
            {
                return true;
            }
            if (botUsers.Contains(username))
            {
                return true;
            }
            return false;
        }

        public static bool IsGroupAuto { get; set; } = false;

        public static void CleanUserBot()
        {
            botUsers?.Clear();
            botUsers = null;
        }

        public static bool IsLogin
        {
            get
            {
                return init_data != null;
            }
        }

        public static string LastToUserName { get; set; }

        public static void Logout()
        {
            PeppaUtils.Debug("微信连接已断开");
            deviceId = null;
            users?.Clear();
            users = null;
            allContact?.Clear();
            allContact = null;
            CleanUserBot();
            skey = null;
            wxsid = null;
            wxuin = null;
            pass_ticket = null;
            init_data = null;
            cookieObjects = null;
            cookies.Clear();
            PeppaUtils.BeginInvoke(() => { MainWindow.NickName = ""; });
            LogoutCompleted?.Invoke();
        }

        public static MainWindow MainWindow { get; set; }

        public static Action LoginCompleted;

        public static Action LogoutCompleted;

    }
}
