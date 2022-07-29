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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Orders.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        private readonly DispatcherTimer timer = new DispatcherTimer();

        private bool IsCreateUser => SelectedOrder?.RouteOrders.Count > 0
                                        && SelectedOrder?.RouteOrders.FirstOrDefault().ro_userId == App.CurrentUser.id;


        public static readonly RepositoryBase repo = new RepositoryBase();
        //private readonly IRepository<RouteOrder> repoRouteOrder;
        public ObservableCollection<Order> ListOrders { get; set; }

               
        public Order SelectedOrder { get; set; }

        public User CurrentUser  => App.CurrentUser;

        public bool CheckCreated { get; set; }
        public bool CheckCoordinated { get; set; } = true;
        public bool CheckClosed { get; set; }
        public bool CheckWork { get; set; }
        public bool CheckAll { get; set; }


        public MainWindowViewModel()
        {
            //App.CurrentUser = new User { id = 11, u_name = "Создатель заказов", u_role = 0 };
            //repo = new RepositoryBase();

            //ListOrders = new ObservableCollection<Order>(repo.Orders
            //    .Where(it => it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any() )
            //    );

            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += Timer_Tick;
            timer.Start();

            //CheckCreated = true;
            OnFilterCommandExecuted("");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OnFilterCommandExecuted(null);
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
            //int select = int.Parse(p.ToString());
            timer.Stop();

            repo.Refresh<Order>(ListOrders);
            if (ListOrders != null)
            {
                foreach (var ro in ListOrders)
                {
                    repo.Refresh<RouteOrder>(ro.RouteOrders);
                    foreach (var item2 in ro.RouteOrders)
                    {
                        repo.Refresh<RouteAdding>(item2.RouteAddings);
                    }
                }
            }

            if (CheckCreated)
            {
                // созданные мной заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(i => i.ro_step == 0 && i.ro_userId == App.CurrentUser.id).Any())
                );

            }
            else if(CheckCoordinated)
            {
                // Требующие рассмотрения
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.o_statusId != (int)EnumStatus.Approved && it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders 
                            .Where(r => r.ro_step == it.o_stepRoute && r.ro_userId == App.CurrentUser.id)
                            .Any())
                   );
            }

            else if(CheckClosed)
            {
                // Закрытые заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.o_statusId == (int)EnumStatus.Closed && it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                );
            }
            else if(CheckWork)
            {

                // В работе
                //ListOrders = new ObservableCollection<Order>(from s in repo.Orders where s.o_statusId < (int)EnumStatus.Closed select s);
                //repo.GetAll();
                ListOrders.Clear();

                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                    .Include(it => it.RouteOrders)
                );


            }
            else
            {
                // все заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                );
            }

            foreach (var item in ListOrders)
            {
                item.RouteOrders = SortStepRoute(item.RouteOrders);
            }

            foreach(var item in ListOrders)
            {
                item.WorkUser = item.RouteOrders.FirstOrDefault(it => it.ro_step == item.o_stepRoute && it.ro_check == 0)?.User;
            }


            OnPropertyChanged(nameof(ListOrders));

            timer.Start();
        }


        //--------------------------------------------------------------------------------
        // Команда Удалить заказ
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteCommand = null;
        public ICommand DeleteCommand => _DeleteCommand ?? new LambdaCommand(OnDeleteCommandExecuted, CanDeleteCommand);
        private bool CanDeleteCommand(object p) => SelectedOrder != null 
                            && (SelectedOrder.o_statusId == (int)EnumStatus.Closed || SelectedOrder.o_statusId == (int)EnumStatus.Created)
                            && IsCreateUser;
        private void OnDeleteCommandExecuted(object p)
        {

            if (MessageBox.Show($"Удалить заказ {SelectedOrder.o_name}", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                repo.Delete(SelectedOrder, true);
                ListOrders.Remove(SelectedOrder);
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Отчет
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReportCommand = null;
        public ICommand ReportCommand => _ReportCommand ?? new LambdaCommand(OnReportCommandExecuted, CaReportCommand);
        private bool CaReportCommand(object p) => true;
        private void OnReportCommandExecuted(object p)
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
            timer.Stop();

            OrderWindowViewModel vm = new OrderWindowViewModel(SelectedOrder);
            OrderWindow orderWindow = new OrderWindow();
            orderWindow.DataContext = vm;
            //(orderWindow.DataContext as OrderWindowViewModel).order = SelectedOrder;
            if (orderWindow.ShowDialog() == true)
            {
                repo.Save();

                //SelectedOrder.RouteStatus.OnPropertyChanged(nameof(SelectedOrder.RouteStatus.sr_name));

                OnFilterCommandExecuted(null);
            }
            timer.Start();
        }

        //--------------------------------------------------------------------------------
        // Команда Создать
        //--------------------------------------------------------------------------------
        private readonly ICommand _CreateCommand = null;
        public ICommand CreateCommand => _CreateCommand ?? new LambdaCommand(OnCreateCommandExecuted, CanCreateCommand);
        private bool CanCreateCommand(object p) => true;
        private void OnCreateCommandExecuted(object p)
        {
            timer.Stop();

            CreateOrderWindow orderWindow = new CreateOrderWindow();
            if (orderWindow.ShowDialog() == true)
            {
                Order order = (orderWindow.DataContext as CreateOrderWindowViewModel).Order;

                //ListOrders.Add(order);
                //repo.Add(order, true);
                repo.Save();

                OnFilterCommandExecuted(null);

            }
            timer.Start();
        }


        #endregion

        //--------------------------------------------------------------------------------
        // Сортировка маршрута по номерам
        //--------------------------------------------------------------------------------
        private ICollection<RouteOrder> SortStepRoute(ICollection<RouteOrder> ListRO)
        {
            return ListRO.OrderBy(o => o.ro_step).ToList();
            //return list;
            //List<RouteOrder> list = ListRO.OrderBy(o => o.ro_step).ToList();
            //return list;
        }

    }
}
