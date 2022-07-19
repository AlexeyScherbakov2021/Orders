using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class CreateOrderWindowViewModel : ViewModel
    {
        public Order order { get; set; }
        private readonly RepositoryBase repo = new RepositoryBase();
        public List<Route> ListRoute { get; set; }
        public Order Order { get; set; }

        private Route _SelectedRoute;
        public Route SelectedRoute 
        { 
            get => _SelectedRoute;
            set 
            {
                _SelectedRoute = value;
                ListRouteStep = new ObservableCollection<RouteStep>(repo.RouteSteps
                    .Where(it => it.r_routeId == _SelectedRoute.id)
                    .OrderBy(it => it.r_step));
            }
        }

        private ObservableCollection<RouteStep> _ListRouteStep;
        public ObservableCollection<RouteStep> ListRouteStep { get => _ListRouteStep;
            set { Set(ref _ListRouteStep, value ); } }



        public CreateOrderWindowViewModel()
        {
            ListRoute = repo.Routes.ToList();
            Order = new Order();
            Order.RouteOrders = new List<RouteOrder>();
            Order.o_date_created = DateTime.Now;
            Order.o_stepRoute = 0;
            Order.o_statusId = 1;

            RouteOrder ro = new RouteOrder
            {
                ro_step = 0,
                RouteAddings = new List<RouteAdding>(),
                //User = App.CurrentUser,
                ro_userId = App.CurrentUser.id,
                ro_typeId = 5,
            };
            Order.RouteOrders.Add(ro);
        }


        #region Команды
        //--------------------------------------------------------------------------------
        // Команда Создать
        //--------------------------------------------------------------------------------
        private readonly ICommand _CreateCommand = null;
        public ICommand CreateCommand => _CreateCommand ?? new LambdaCommand(OnCreateCommandExecuted, CanCreateCommand);
        private bool CanCreateCommand(object p) => true;
        private void OnCreateCommandExecuted(object p)
        {
            foreach(var step in ListRouteStep)
            {
                if (step.r_disabled == false)
                {
                    RouteOrder ro = new RouteOrder();
                    ro.ro_step = step.r_step;
                    ro.ro_userId = step.r_userId;
                    ro.ro_typeId = step.r_type;
                    ro.ro_disabled = false;
                    Order.RouteOrders.Add(ro);
                }
            }


            repo.Add<Order>(Order, true);

            App.Current.Windows.OfType<CreateOrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Отменить
        //--------------------------------------------------------------------------------
        private readonly ICommand _CancelCommand = null;
        public ICommand CancelCommand => _CancelCommand ?? new LambdaCommand(OnCancelCommandExecuted, CanCancelCommand);
        private bool CanCancelCommand(object p) => true;
        private void OnCancelCommandExecuted(object p)
        {
        }

        //--------------------------------------------------------------------------------
        // Команда Добавить файл
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddFileCommand = null;
        public ICommand AddFileCommand => _AddFileCommand ?? new LambdaCommand(OnAddFileCommandExecuted, CanAddFileCommand);
        private bool CanAddFileCommand(object p) => true;
        private void OnAddFileCommandExecuted(object p)
        {
        }

        //--------------------------------------------------------------------------------
        // Команда Удалить файл
        //--------------------------------------------------------------------------------
        private readonly ICommand _RemoveFileCommand = null;
        public ICommand RemoveFileCommand => _RemoveFileCommand ?? new LambdaCommand(OnRemoveFileCommandExecuted, CanRemoveFileCommand);
        private bool CanRemoveFileCommand(object p) => true;
        private void OnRemoveFileCommandExecuted(object p)
        {
        }


        #endregion

    }
}
