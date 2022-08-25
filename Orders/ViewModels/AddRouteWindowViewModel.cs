using Orders.Infrastructure;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class AddRouteWindowViewModel : ViewModel
    {
        private Order CurrentOrder;
        public ObservableCollection<RouteOrder> ListRouteOrder { get; set; }

        private RouteOrder _SelectedRouteOrder;
        public RouteOrder SelectedRouteOrder 
        { 
            get => _SelectedRouteOrder; 
            set 
            {
                if (Set(ref _SelectedRouteOrder, value))
                {
                    OnPropertyChanged(nameof(IsEnabledParallel));
                    IsLaterStep = true;
                    IsSameStep = false;
                }
            } 
        }

        public bool IsLaterStep { get; set; } = true;
        public bool IsSameStep { get;set; }
        //private bool _IsEnabledParallel;
        //public bool IsEnabledParallel { get => _IsEnabledParallel; set { Set(ref _IsEnabledParallel, value); } }
        public bool IsEnabledParallel => SelectedRouteOrder != null;


        public List<User> ListUser { get; }
        public User SelectedUser { get; set; }
        public List<RouteType> ListRouteType { get; }
        public RouteType SelectedType { get; set; }

        //public bool _IsReturn  = false;
        //public bool IsReturn { get => _IsReturn; set { Set(ref _IsReturn, value); GetListSteps(); } }
        public bool IsReturn { get; set; }
        
        //public Visibility IsVisible => IsReturn? Visibility.Collapsed : Visibility.Visible;

        private readonly RouteOrder _CurrentStep;

        //RepositoryBase repo = new RepositoryBase();


        public AddRouteWindowViewModel() { }

        public AddRouteWindowViewModel(Order order, RouteOrder CurrentStep )
        {
            CurrentOrder = order;
            _CurrentStep = CurrentStep;

            //if (CurrentStep.ro_return_step == null)
            //    ListRouteOrder = order.RouteOrders.Where(it => it.ro_step >= order.o_stepRoute 
            //        && it.ro_return_step == null ).ToList();
            //else
            //{
            //    ListRouteOrder = new List<RouteOrder>();
            //    ListRouteOrder.Add(CurrentStep);
            //}
            GetListSteps();

            SelectedRouteOrder = ListRouteOrder.FirstOrDefault();

            ListUser = MainWindowViewModel.repo.Users.Where(it => it.u_role != 1).OrderBy(o => o.u_name).ToList();
            ListRouteType = MainWindowViewModel.repo.RouteTypes.Where(it => it.id != (int)EnumTypesStep.Created).ToList();
            SelectedType = ListRouteType[0];
        }


        //--------------------------------------------------------------------------------
        // получение списка этапов
        //--------------------------------------------------------------------------------
        private void GetListSteps()
        {

            if (_CurrentStep.ro_return_step == null)
            {
                // это корневой текущий этап
                if(IsReturn)
                    // с возвратом. Список подчиненных
                    ListRouteOrder = new ObservableCollection<RouteOrder>( CurrentOrder.RouteOrders.Where(it => 
                            it.ro_return_step == _CurrentStep.ro_step));
                else
                    // обычный. Список кроме подчиненных
                    ListRouteOrder = new ObservableCollection<RouteOrder>(CurrentOrder.RouteOrders.Where(it => it.ro_step >= _CurrentStep.ro_step
                            && it.ro_return_step == null));

            }
            else
                // Подчиненный текущий этап
                ListRouteOrder = new ObservableCollection<RouteOrder>(CurrentOrder.RouteOrders.Where(it => 
                            it.ro_return_step == _CurrentStep.ro_return_step && it.ro_step >= _CurrentStep.ro_step));

            OnPropertyChanged(nameof(ListRouteOrder));
        }


        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Check Вернуть после рассмотрения
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReturnBackCommand = null;
        public ICommand ReturnBackCommand => _ReturnBackCommand ?? new LambdaCommand(OnReturnBackCommandExecuted, CanReturnBackCommand);
        private bool CanReturnBackCommand(object p) => _CurrentStep?.ro_return_step == null;
        private void OnReturnBackCommandExecuted(object p)
        {
            GetListSteps();
        }


        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => SelectedUser != null && SelectedType != null 
                        && (SelectedRouteOrder != null || IsReturn);
        private void OnAddRouteCommandExecuted(object p)
        {
            int AddStep = IsSameStep ? 0 : 1;
            List<RouteOrder> TempList = CurrentOrder.RouteOrders.ToList();
            int index;

            RouteOrder ro = new RouteOrder();
            ro.ro_typeId = SelectedType.id;
            ro.ro_userId = SelectedUser.id;
            ro.ro_statusId = (int)EnumStatus.None;
            ro.ro_orderId = CurrentOrder.id;
            ro.ro_ownerId = App.CurrentUser.id;

            if (_CurrentStep.ro_return_step != null)
            {
                ro.ro_step = _CurrentStep.ro_step + AddStep;
                ro.ro_return_step = _CurrentStep.ro_return_step;
                // ищем первый свободный этап и его индекс
                RouteOrder Next = TempList.FirstOrDefault(it => it.ro_step > _CurrentStep.ro_step);
                index = TempList.IndexOf(Next);
            }

            // если это был этап с возвратом
            else if (IsReturn)
            {
                if(SelectedRouteOrder is null)
                    // если выбранного нет или пусто, то отсчитываем от текущего
                    ro.ro_step = CurrentOrder.o_stepRoute + AddStep;
                else
                {
                    // если был выбран этап
                    RouteOrder itemChild = ListRouteOrder.FirstOrDefault(it => it.ro_return_step == CurrentOrder.o_stepRoute);
                    if (itemChild != null)
                        // был получен списоку уже имеющихсф подэтапоа
                        ro.ro_step = itemChild.ro_step + AddStep;
                    else
                        // берем текущий для отсчета
                        ro.ro_step = CurrentOrder.o_stepRoute + AddStep;
                }

                ro.ro_return_step = CurrentOrder.o_stepRoute;
                RouteOrder Next = TempList.FirstOrDefault(it => it.ro_step > ro.ro_step - AddStep);
                index = TempList.IndexOf(Next);

            }
            else
            {
                // добавление в корневой маршрут
                RouteOrder Next = IsSameStep 
                    ? SelectedRouteOrder
                    : TempList.FirstOrDefault(it => it.ro_step > SelectedRouteOrder.ro_step && it.ro_return_step is null);
                // индекс и этап для следующего шага
                index = TempList.IndexOf(Next);
                ro.ro_step = Next is null ? TempList.Last().ro_step + 1 : Next.ro_step;
            }


            if (index < 0)
                TempList.Add(ro);
            else
            {
                TempList.Insert(index, ro);
                // перенумерация этапов после добавленного
                for (int i = index + 1; i < TempList.Count; i++)
                    TempList[i].ro_step += AddStep;
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
