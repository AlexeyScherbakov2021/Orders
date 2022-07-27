using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class LoginWindowViewModel : ViewModel
    {
        private readonly RepositoryBase repo;
        //private readonly LoginWindow winLogin;

        public User SelectUser { get; set; }
        public IEnumerable<User> ListUser { get; set; }


        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Сохранить дефлятор
        //--------------------------------------------------------------------------------
        //private readonly ICommand _OkCommand = null;
        public ICommand OkCommand => new LambdaCommand(OnOkCommandExecuted, CanOkCommand);
        private bool CanOkCommand(object p) => true;
        private void OnOkCommandExecuted(object p)
        {
            if (p is PasswordBox pass)
            {
                // если праоль неверный, то ничего не делаем - возврат
                if (pass.Password != SelectUser?.u_pass)
                    return;

                App.CurrentUser = SelectUser;
                if (App.CurrentUser.u_role == 1)
                {
                    //если это администратор, то запускаем настройки
                    SettingWindow win = new SettingWindow();
                    win.Show();
                    App.Current.MainWindow = win;
                }
                else
                {
                    // если пользователь, то запускаем табель
                    MainWindow win = new MainWindow();
                    win.Show();
                    App.Current.MainWindow = win;
                }


            }

            App.Current.Windows.OfType<LoginWindow>().First().Close();
        }

        #endregion


        public LoginWindowViewModel()
        {
            App.Log.WriteLineLog("LoginWindowViewModel()");

            //winLogin = App.Current.Windows.OfType<LoginWindow>().First();

            repo = new RepositoryBase();
            ListUser = repo.Users.OrderBy(o => o.u_login).ToArray();

            if( ListUser is null ||  ListUser.Count() == 0 || !ListUser.Any(it => it.u_role == 1))
            {
                ListUser = new List<User> { new User { u_name = "Admin", u_pass = "adm", u_role = 1, u_login="Admin" } };
            }
        }
    }

}
