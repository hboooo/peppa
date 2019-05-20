using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Peppa
{
    public static class MenuCommandExtender
    {
        public static ICommand MusicMenuCommand = new RoutedUICommand("MusicMenuCommand", "MusicMenuCommand", typeof(MenuCommandExtender));

        public static ICommand LoginMenuCommand = new RoutedUICommand("LoginMenuCommand", "LoginMenuCommand", typeof(MenuCommandExtender));

        public static ICommand PhotoMenuCommand = new RoutedUICommand("PhotoMenuCommand", "PhotoMenuCommand", typeof(MenuCommandExtender));

        public static ICommand ContactMenuCommand = new RoutedUICommand("ContactMenuCommand", "ContactMenuCommand", typeof(MenuCommandExtender));
    }
}
