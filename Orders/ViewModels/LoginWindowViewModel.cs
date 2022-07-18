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
        private readonly LoginWindow winLogin;


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
                if (App.CurrentUser.u_role == 100)
                {
                    // если это администратор, то запускаем настройки
                    //EditTablesWindow win = new EditTablesWindow();
                    //win.Show();
                    //App.Current.MainWindow = win;
                }
                else
                {
                    // если пользователь, то запускаем табель
                    MainWindow win = new MainWindow();
                    win.Show();
                    App.Current.MainWindow = win;
                }


            }

            winLogin.Close();
        }

        #endregion


        public LoginWindowViewModel()
        {
            winLogin = App.Current.Windows.OfType<LoginWindow>().First();
            //winLogin.Closing += Win_Closing;

            repo = new RepositoryBase();
            ListUser = repo.Users.ToArray();
        }
    }

}
