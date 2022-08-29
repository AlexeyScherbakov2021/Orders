﻿using Orders.Infrastructure;
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
        //private Order CurrentOrder;
        public ObservableCollection<RouteOrder> ListRouteOrder { get; set; }
        private ObservableCollection<RouteOrder> _ListRouteOrder;
        public RouteOrder SelectedRouteOrder { get; set; }


        public List<User> ListUser { get; }
        public User SelectedUser { get; set; }
        public List<RouteType> ListRouteType { get; }
        public RouteType SelectedType { get; set; }


        public bool IsLaterStep { get; set; } = true;
        public bool IsSameStep { get; set; }

        private bool _IsReturn  = false;
        public bool IsReturn { get => _IsReturn; set { Set(ref _IsReturn, value); SelectSteps(); } }

        //public Visibility IsVisible => IsReturn? Visibility.Collapsed : Visibility.Visible;

        private readonly RouteOrder _CurrentStep;

        //RepositoryBase repo = new RepositoryBase();


        public AddRouteWindowViewModel() { }

        public AddRouteWindowViewModel(ObservableCollection<RouteOrder> listSteps, RouteOrder CurrentStep )
        {
            //CurrentOrder = order;
            _CurrentStep = CurrentStep;
            _ListRouteOrder = listSteps;
            SelectSteps();

            //ListRouteOrder = order.RouteOrders.Where(it => it.ro_step >= order.o_stepRoute && it.ro_parentId == null).ToList();

            //if (CurrentStep.ro_return_step == null)
            //    ListRouteOrder = order.RouteOrders.Where(it => it.ro_step >= order.o_stepRoute).ToList();
            //else
            //{
            //    ListRouteOrder = new List<RouteOrder>();
            //    ListRouteOrder.Add(CurrentStep);
            //}

            //SelectedRouteOrder = ListRouteOrder.FirstOrDefault();

            ListUser = MainWindowViewModel.repo.Users.Where(it => it.u_role != 1).OrderBy(o => o.u_name).ToList();
            ListRouteType = MainWindowViewModel.repo.RouteTypes.Where(it => it.id != (int)EnumTypesStep.Created).ToList();
            SelectedType = ListRouteType[0];
        }

        private void SelectSteps()
        {
            if(IsReturn)
                //ListRouteOrder = new ObservableCollection<RouteOrder>(_CurrentStep.ChildRoutes.Where(it => it.ro_check != EnumCheckedStatus.Checked));
                ListRouteOrder = new ObservableCollection<RouteOrder>(_CurrentStep.ChildRoutes.Where(it => it.ro_check != EnumCheckedStatus.Checked));
            else
                ListRouteOrder = new ObservableCollection<RouteOrder>(_ListRouteOrder.Where(it => it.ro_step >= _CurrentStep.ro_step 
                        && it.ro_parentId == null));

            OnPropertyChanged(nameof(ListRouteOrder));
            SelectedRouteOrder = ListRouteOrder.FirstOrDefault();

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
            //int addStep = IsLaterStep ? 1 : 0;
            //ICollection<RouteOrder> TempList = null;

            RouteOrder ro = new RouteOrder();
            ro.ro_typeId = SelectedType.id;
            ro.ro_userId = SelectedUser.id;
            ro.ro_statusId = (int)EnumStatus.None;
            ro.ro_orderId = _CurrentStep.ro_orderId;
            ro.ro_ownerId = App.CurrentUser.id;
            ro.ro_step = SelectedRouteOrder?.ro_step ?? 0;

            if(IsReturn)
            {
                ro.ro_parentId = _CurrentStep.id;
                AddInRoute( _CurrentStep.ChildRoutes, ro, SelectedRouteOrder, IsLaterStep);
            }
            else
                AddInRoute(_ListRouteOrder, ro, SelectedRouteOrder, IsLaterStep);

            App.Current.Windows.OfType<AddRouteWindow>().FirstOrDefault().DialogResult = true;

        }


        //--------------------------------------------------------------------------------
        // Добавление этапа в указанный подмаршрут
        //--------------------------------------------------------------------------------
        private ICollection<RouteOrder> AddInRoute(ObservableCollection<RouteOrder> ListStep, RouteOrder NewStep, RouteOrder InStep, bool IsAfter)
        {
            //List<RouteOrder> list = ListStep.ToList();

            int index = ListStep.IndexOf(InStep) + 1;
            ListStep.Insert(index, NewStep);

            if (IsAfter)
            {
                // перенумерация 
                for (int i = index; i < ListStep.Count; i++)
                {
                    ListStep[i].ro_step++;
                    var item = ListStep[i];
                    item.OnPropertyChanged(nameof(item.NameStep));
                }
            }

            MainWindowViewModel.repo.Add<RouteOrder>(NewStep, true);
            return ListStep;
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
