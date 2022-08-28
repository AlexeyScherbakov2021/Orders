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
using System.Xml.Linq;

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

            // подготовка к обновлению всех записей
            repo.Refresh();

            //if (ListOrders != null)
            //{
            //    foreach (var ro in ListOrders)
            //    {
            //        repo.Refresh<RouteOrder>(ro.RouteOrders);
            //        foreach (var item2 in ro.RouteOrders)
            //        {
            //            repo.Refresh<RouteAdding>(item2.RouteAddings);
            //        }
            //    }
            //}


            if (CheckCreated)
            {
                // созданные мной заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(i => i.ro_step == 0 
                && i.ro_userId == App.CurrentUser.id).Any())
                //.Include(it => it.RouteOrders)
                );

            }
            else if(CheckCoordinated)
            {
                // Требующие рассмотрения
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.RouteOrders 
                            .Where(r => r.ro_step == it.o_stepRoute && r.ro_userId == App.CurrentUser.id
                                && r.ro_check == EnumCheckedStatus.CheckedProcess && r.ro_parentId == null)
                            .Any())
                    //.Include(it => it.RouteOrders)
                   );
            }

            else if(CheckClosed)
            {
                // Закрытые заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.o_statusId == (int)EnumStatus.Closed && it.RouteOrders.Where(r => 
                    r.ro_userId == App.CurrentUser.id).Any())
                    //.Include(it => it.RouteOrders)
                );
            }
            else if(CheckWork)
            {

                // В работе
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(r =>
                    r.ro_userId == App.CurrentUser.id).Any())
                );


                //ListOrders = new ObservableCollection<Order>(repo.Orders
                //    .Select(c => new
                //    {
                //        id = c.id,
                //        name = c.o_name,
                //        o_body = c.o_body,
                //        o_buyer = c.o_buyer,
                //        o_stepRoute = c.o_stepRoute,
                //        o_number = c.o_number,
                //        o_statusId = c.o_statusId,
                //        o_date_created = c.o_date_created,
                //        o_CardOrder = c.o_CardOrder,
                //        list = c.RouteOrders.Where(it => it.ro_parentId == null),
                //        RouteStatus = c.RouteStatus

                //    }
                //    ).AsEnumerable()
                //    .Select( res => new Order
                //    {
                //        id = res.id,
                //        o_name = res.name,
                //        o_body = res.o_body,
                //        o_buyer = res.o_buyer,
                //        o_stepRoute = res.o_stepRoute,
                //        o_number = res.o_number,
                //        o_statusId = res.o_statusId,
                //        o_CardOrder = res.o_CardOrder,
                //        o_date_created = res.o_date_created,
                //        RouteOrders = res.list.ToList(),
                //        RouteStatus = res.RouteStatus
                //    }

                //    )
                //    .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(r =>
                //    r.ro_userId == App.CurrentUser.id).Any())

                //);


                //ListOrders = new ObservableCollection<Order>(repo.Orders
                //    .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(r =>
                //    r.ro_userId == App.CurrentUser.id && r.ro_parentId != null).Any())
                //    .Include(it => it.RouteOrders)
                //);

                //ListOrders = new ObservableCollection<Order>(repo.Orders
                //    .Where(it => it.o_statusId < (int)EnumStatus.Closed && it.RouteOrders.Where(r =>
                //    r.ro_userId == App.CurrentUser.id && r.ro_parentId != null).Any())
                //    .Include(it => it.RouteOrders)
                //);


            }
            else if(CheckAll)
            {
                // все заказы
                ListOrders = new ObservableCollection<Order>(repo.Orders
                    .Where(it => it.RouteOrders.Where(r => r.ro_userId == App.CurrentUser.id).Any())
                    //.Include(it => it.RouteOrders)
                );
            }

            // сортировка этапов для всех заказов
            foreach (var item in ListOrders)
            {
                item.RouteOrders = SortStepRoute(item.RouteOrders);
            }

            // установка пользователей в работе для каждого заказа
            foreach(var item in ListOrders)
            {
                item.WorkUser = item.RouteOrders.FirstOrDefault(it => it.ro_step == item.o_stepRoute 
                            && it.ro_check == EnumCheckedStatus.CheckedProcess)?.User;
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
                RepositoryFiles repoFiles = new RepositoryFiles();
                repoFiles.DeleteFiles(SelectedOrder);
                repo.Delete(SelectedOrder, true);
                ListOrders.Remove(SelectedOrder);
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Отчет по заказам
        //----------        ----------------------------------------------------------------------

        //--------------------------------------------------------------------------------
        // Команда Отчет
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReportCommand = null;
        public ICommand ReportCommand => _ReportCommand ?? new LambdaCommand(OnReportCommandExecuted, CaReportCommand);
        private bool CaReportCommand(object p) => true;
        private void OnReportCommandExecuted(object p)
        {
            timer.Stop();
            //ReportRoutesWindowViewModel vm = new ReportRoutesWindowViewModel();
            ReportRoutesWindow reportWindow = new ReportRoutesWindow();
            reportWindow.ShowDialog();
            timer.Start();
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

            //repo.LoadRouteOrders(SelectedOrder);

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
