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
using System.Windows;
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

        public bool _IsReturn  = false;
        public bool IsReturn { get => _IsReturn; set { Set(ref _IsReturn, value); OnPropertyChanged(nameof(IsVisible)); } }
        public Visibility IsVisible => IsReturn? Visibility.Collapsed : Visibility.Visible;
    

        //RepositoryBase repo = new RepositoryBase();


        public AddRouteWindowViewModel() { }

        public AddRouteWindowViewModel(Order order )
        {
            CurrentOrder = order;
            ListRouteOrder = order.RouteOrders.Where(it => it.ro_step > order.o_stepRoute).ToList();
            SelectedRouteOrder = ListRouteOrder.FirstOrDefault();

            ListUser = MainWindowViewModel.repo.Users.Where(it => it.u_role != 1).OrderBy(o => o.u_name).ToList();
            ListRouteType = MainWindowViewModel.repo.RouteTypes.Where(it => it.id != (int)EnumTypesStep.Created).ToList();
            SelectedType = ListRouteType[0];
        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => SelectedUser != null && SelectedType != null 
                        && (SelectedRouteOrder != null || IsReturn);
        private void OnAddRouteCommandExecuted(object p)
        {

            RouteOrder ro = new RouteOrder();
            ro.ro_typeId = SelectedType.id;
            ro.ro_userId = SelectedUser.id;
            ro.ro_statusId = (int)EnumStatus.None;
            ro.ro_orderId = CurrentOrder.id;
            ro.ro_ownerId = App.CurrentUser.id;

            int insertToStep;

            // если это был этап с возвратом
            if (IsReturn)
            {
                // вставить этап сразу после текущего
                // или, если уже была вставка с возвратом, то после вставленного
                RouteOrder itemChild = ListRouteOrder.FirstOrDefault(it => it.ro_return_step == CurrentOrder.o_stepRoute);
                if(itemChild != null)
                {
                    //itemChild.ro_return_step = null;
                    insertToStep = itemChild.ro_step + 1;
                }
                else
                    insertToStep = CurrentOrder.o_stepRoute + 1;

                ro.ro_return_step = CurrentOrder.o_stepRoute;
            }
            else
                insertToStep = ro.ro_step = SelectedRouteOrder.ro_step;

            ro.ro_step = insertToStep;

            // перенумерауия этапов
            List<RouteOrder> TempList = new List<RouteOrder>();                
               
            foreach(var item in CurrentOrder.RouteOrders)
            {
                if (item.ro_step == ro.ro_step)
                    TempList.Add(ro);

                if(item.ro_step >= ro.ro_step)
                    item.ro_step++;

                TempList.Add(item);

            }

            MainWindowViewModel.repo.Add<RouteOrder>(ro);
            CurrentOrder.RouteOrders = TempList;
            MainWindowViewModel.repo.Update(CurrentOrder, true);
            App.Current.Windows.OfType<AddRouteWindow>().FirstOrDefault().DialogResult = true;

        }

        //--------------------------------------------------------------------------------
        // Команда Отменить
        //--------------------------------------------------------------------------------
        private readonly ICommand _CancelCommand = null;
        public ICommand CancelCommand => _CancelCommand ?? new LambdaCommand(OnCancelCommandExecuted, CanCancelCommand);
        private bool CanCancelCommand(object p) => true;
        private void OnCancelCommandExecuted(object p)
        {
            App.Current.Windows.OfType<AddRouteWindow>().FirstOrDefault().Close();
        }


        #endregion

    }
}
