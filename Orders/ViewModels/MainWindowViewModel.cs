using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class MainWindowViewModel
    {
        private readonly IRepository<Order> repoOrder;
        private readonly IRepository<RouteOrder> repoRouteOrder;
        public ObservableCollection<Order> ListOrders { get; set; }

        public MainWindowViewModel()
        {
            App.CurrentUser = new User { id = 1, u_name = "Admin" };

            repoRouteOrder = new RepositoryMain<RouteOrder>();
            List<RouteOrder> ListRO =  repoRouteOrder.Items
                .Where(it => it.User.id == App.CurrentUser.id).ToList();

            repoOrder = new RepositoryMain<Order>();
            ListOrders = new ObservableCollection<Order>(repoOrder.Items);

            //var value = repoOrder.Items
            //    .Select(it => new Order
            //    {
            //        id = it.id,
            //        o_name = it.o_name,
            //        RouteOrders = it.RouteOrders.Where(n => n.User.id == 1).ToList()

            //    });
                

        }

        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Сохранить дефлятор
        //--------------------------------------------------------------------------------
        private readonly ICommand _CreateCommand = null;
        public ICommand CreateCommand => _CreateCommand ?? new LambdaCommand(OnCreateCommandExecuted, CanCreateCommand);
        private bool CanCreateCommand(object p) => true;
        private void OnCreateCommandExecuted(object p)
        {

            CreateOrderWindow win = new CreateOrderWindow();
            if(win.ShowDialog() == true)
            {



            }


            Order NewOrder = new Order
            {
                o_stepRoute = 0,
                o_statusId = 1,
                o_date_created = DateTime.Now,
                RouteOrders = new List<RouteOrder>()
            };



            //NewOrder.RouteOrders



        }

        #endregion



    }
}
