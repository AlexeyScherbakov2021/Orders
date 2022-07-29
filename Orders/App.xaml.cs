using Orders.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Orders
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User CurrentUser;
        public static LogFile Log = new LogFile(1);

        public App()
        {
            Log.WriteLineLog("Start");
        }

    }
}
