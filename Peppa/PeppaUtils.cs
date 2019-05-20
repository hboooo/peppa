using System;
using System.Windows;

namespace Peppa
{
    public class PeppaUtils
    {
        public static long GetUnixDateTimeNow()
        {
            return ConvertDateTimeInt(DateTime.Now);
        }

        public static string GetDeviceID()
        {
            return "e" + DateTime.Now.ToString("yyyyMMddHHmmssf");
        }

        public static long ConvertDateTimeInt(System.DateTime time)
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static void BeginInvoke(Action action)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { action?.Invoke(); }));
            }
        }

        public static void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now + "  " + message);
        }

        public static void Debug(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now + "  异常:" + ex.Message);
        }
    }
}
