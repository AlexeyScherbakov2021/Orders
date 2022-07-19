using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
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
    internal class MainWindowViewModel : ViewModel
    {
        private readonly RepositoryBase repo;
        //private readonly IRepository<RouteOrder> repoRouteOrder;
        public ObservableCollection<Order> ListOrders { get; set; }

               
        public Order SelectedOrder { get; set; }

        public User CurrentUser  => App.CurrentUser;

        public bool CheckCreated { get; set; }
        public bool CheckCoordinated { get; set; }
        public bool CheckClosed { get; set; }
        public bool CheckAll { get; set; }


        public MainWindowViewModel()
        {
            App.CurrentUser = new User { id = 3, u_name = "Иванов" };
            repo = new RepositoryBase();

            //ListOrders = new ObservableCollection<Order>(repo.Orders
            //    .Where(it => it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any() )
            //    );
            
            CheckCreated = true;
            OnFilterCommandExecuted("1");
        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Фильтровать заказы
        //--------------------------------------------------------------------------------
        private readonly ICommand _FilterCommand = null;
        public ICommand FilterCommand => _FilterCommand ?? new LambdaCommand(OnFilterCommandExecuted, CanFilterCommand);
        private bool CanFilterCommand(object p) => true;
        private void OnFilterCommandExecuted(object p)
        {
            int select = int.Parse(p.ToString());

            switch(select)
            {
                case 1:
                    ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.RouteOrders.Where(i => i.ro_step == 0 && i.ro_userId == App.CurrentUser.id).Any())
                    );
                    break;

                case 2:
                    ListOrders = new ObservableCollection<Order>(repo.Orders
                        .Where(it => it.RouteOrders.Where(r => r.ro_step == it.o_stepRoute && r.ro_userId == App.CurrentUser.id).Any())
                        );
                    break;

                case 3:
                    ListOrders = new ObservableCollection<Order>(repo.Orders
                        .Where(it => it.o_statusId == (int)EnumStatus.Closed && it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                    );
                    break;

                case 4:
                    ListOrders = new ObservableCollection<Order>(repo.Orders
                        .Where(it => it.o_statusId < (int)EnumStatus.Approved && it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                    );
                    break;

            }

            OnPropertyChanged(nameof(ListOrders));

        }
        //ListOrdersClosed = new ObservableCollection<Order>( ListOrders.Where(it => it.o_statusId == (int)EnumStatus.Closed) );
        //ListOrdersWorks = new ObservableCollection<Order>( ListOrders.Where(it => it.o_statusId < (int)EnumStatus.Approved) );


        //--------------------------------------------------------------------------------
        // Команда Отправить заказ
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => true;
        private void OnSendCommandExecuted(object p)
        {

        }

        //--------------------------------------------------------------------------------
        // Команда Двойной щелчок
        //--------------------------------------------------------------------------------
        private readonly ICommand _DblClickCommand = null;
        public ICommand DblClickCommand => _DblClickCommand ?? new LambdaCommand(OnDblClickCommandExecuted, CanDblClickCommand);
        private bool CanDblClickCommand(object p) => SelectedOrder != null;
        private void OnDblClickCommandExecuted(object p)
        {
            OrderWindow orderWindow = new OrderWindow();
            (orderWindow.DataContext as OrderWindowViewModel).order = SelectedOrder;
            if (orderWindow.ShowDialog() == true)
            {
                repo.Save();
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Создать
        //--------------------------------------------------------------------------------
        private readonly ICommand _CreateCommand = null;
        public ICommand CreateCommand => _CreateCommand ?? new LambdaCommand(OnCreateCommandExecuted, CanCreateCommand);
        private bool CanCreateCommand(object p) => true;
        private void OnCreateCommandExecuted(object p)
        {
            CreateOrderWindow orderWindow = new CreateOrderWindow();
            if (orderWindow.ShowDialog() == true)
            {
                Order order = (orderWindow.DataContext as CreateOrderWindowViewModel).Order;

                ListOrders.Add(order);
                //repo.Save();
            }
        }


        #endregion



    }
}
