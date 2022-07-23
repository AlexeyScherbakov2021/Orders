using Orders.Infrastructure;
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
    internal class AddRouteWindowViewModel : ViewModel
    {
        private Order CurrentOrder;
        public List<RouteOrder> ListRouteOrder { get; set; }
        public RouteOrder SelectedRouteOrder { get; set; }


        public List<User> ListUser { get; }
        public User SelectedUser { get; set; }
        public List<RouteType> ListRouteType { get; }
        public RouteType SelectedType { get; set; }

        public bool IsReturn { get; set; } = false;

        //RepositoryBase repo = new RepositoryBase();


        public AddRouteWindowViewModel() { }

        public AddRouteWindowViewModel(Order order )
        {
            CurrentOrder = order;
            ListRouteOrder = order.RouteOrders.Where(it => it.ro_step > order.o_stepRoute).ToList();

            ListUser = MainWindowViewModel.repo.Users.Where(it => it.u_role != 1).ToList();
            ListRouteType = MainWindowViewModel.repo.RouteTypes.ToList();

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

            RouteOrder ro = new RouteOrder();
            ro.ro_typeId = SelectedType.id;
            ro.ro_userId = SelectedUser.id;
            ro.ro_statusId = (int)EnumStatus.Waiting;
            ro.ro_orderId = CurrentOrder.id;

            //ro.RouteType = SelectedType;
            //ro.User = SelectedUser;
            ro.ro_step = SelectedRouteOrder.ro_step;
            //ro.RouteStatus = repo.RouteStatus.FirstOrDefault(it => it.id == (int)EnumStatus.Waiting);
            //ro.Order = CurrentOrder;

            List<RouteOrder> TempList = new List<RouteOrder>();                
               
            foreach(var item in CurrentOrder.RouteOrders)
            {
                if (item.ro_step == ro.ro_step)
                {
                    TempList.Add(ro);
                }

                if(item.ro_step >= ro.ro_step)
                {
                    item.ro_step++;
                }

                TempList.Add(item);

            }

            MainWindowViewModel.repo.Add<RouteOrder>(ro);

            CurrentOrder.RouteOrders = TempList;

            //MainWindowViewModel.repo.Update<Order>(CurrentOrder);
            MainWindowViewModel.repo.Update(CurrentOrder, true);
            //MainWindowViewModel.repo.Save();

            App.Current.Windows.OfType<AddRouteWindow>().FirstOrDefault().DialogResult = true;

        }
        #endregion

    }
}
