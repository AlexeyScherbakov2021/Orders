using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using Orders.Wrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class SettingWindowViewModel : ViewModel
    {
        public static RepositoryBase repo = new RepositoryBase();

        public UsersControlViewModel UserViewModel { get; set; }
        public RouteControlViewModel RouteViewModel { get; set; }


        public SettingWindowViewModel()
        {
            UserViewModel = new UsersControlViewModel();
            RouteViewModel = new RouteControlViewModel(repo);

        }

        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Добавить 
        //--------------------------------------------------------------------------------
        public ICommand ClosingCommand => new LambdaCommand(OnClosingCommandExecuted, CanClosingCommand);
        private bool CanClosingCommand(object p) => true;
        private void OnClosingCommandExecuted(object p)
        {
            UserViewModel.SaveUsers();
        }
        #endregion
    }
}
