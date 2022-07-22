using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class AddRouteWindowViewModel : ViewModel
    {
        private Order CurrentOrder;
        private List<RouteOrder> ListRouteOrder;

        public List<User> ListUser { get; }
        public User SelectedUser { get; set; }
        public List<RouteType> ListRouteType { get; }
        public RouteType SelectedType { get; set; }

        public bool IsReturn { get; set; } = false;

        RepositoryBase repo = new RepositoryBase();


        public AddRouteWindowViewModel() { }

        public AddRouteWindowViewModel(Order order)
        {
            CurrentOrder = order;

            ListUser = repo.Users.Where(it => it.u_role != 1).ToList();
            ListRouteType = repo.RouteTypes.ToList();

        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => true;
        private void OnAddRouteCommandExecuted(object p)
        {
        }
        #endregion

    }
}
