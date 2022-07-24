using Microsoft.Win32;
using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.Repository;
using Orders.ViewModels.Base;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class OrderWindowViewModel : ViewModel
    {
        public User CurrentUser => App.CurrentUser;

        private bool IsAllSteps =>  order?.RouteOrders.All(it => it.ro_check == 1) ?? false;
        private bool IsWorkUser => CurrentStep?.ro_userId == App.CurrentUser.id;
        private bool IsCreateUser => order?.RouteOrders.Count > 0
                                                && order?.RouteOrders.FirstOrDefault().ro_userId == App.CurrentUser.id;

        //private Order _order;
        public Order order { get; set; }
        //{ 
        //    get => _order; 
        //    set 
        //    { 
        //        if(Set(ref _order, value))
        //        {
        //            CurrentStep = value.RouteOrders.FirstOrDefault(it => it.ro_step == value.o_stepRoute);
        //            ListFiles = new ObservableCollection<RouteAdding>( CurrentStep.RouteAddings);
        //            OnPropertyChanged(nameof(ListFiles));
        //        }
        //    } 
        //}
        
        private RouteOrder _CurrentStep;
        public RouteOrder CurrentStep { get => _CurrentStep; set { Set(ref _CurrentStep, value); } }


        public OrderWindowViewModel() { }

        public OrderWindowViewModel(Order ord)
        {
            order = ord;
            CurrentStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
            ListFiles = new ObservableCollection<RouteAdding>(CurrentStep.RouteAddings);

        }

        public ObservableCollection<RouteAdding> ListFiles { get; set; } // = new ObservableCollection<RouteAdding>();


        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => IsWorkUser && !IsAllSteps;
        private void OnAddRouteCommandExecuted(object p)
        {
            AddRouteWindowViewModel view = new AddRouteWindowViewModel(order);
            AddRouteWindow winAddRoute = new AddRouteWindow();
            winAddRoute.DataContext = view;
            if (winAddRoute.ShowDialog() == true)
            {
                OnPropertyChanged(nameof(order));
                //RepositoryBase repo = new RepositoryBase();
                //repo.Save();
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => IsWorkUser
                                        //&& order.o_stepRoute < order.RouteOrders.Count
                                        && !IsAllSteps;
        private void OnSendCommandExecuted(object p)
        {

            CurrentStep.RouteAddings = ListFiles;
            CurrentStep.ro_check = 1;
            CurrentStep.ro_date_check = DateTime.Now;
            RouteOrder NextStep;

            if (order.RouteOrders.All(it => it.ro_check == 1))
            {
                // маршрут окончен
                NextStep = null;
                //order.o_stepRoute = order.RouteOrders.Count;
                //order.o_statusId = (int)EnumStatus.Approved;
            }
            else
            {
                order.o_stepRoute++;
                NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
                //order.o_statusId = (int)EnumStatus.Coordinated;
            }
            SetStatusStep(CurrentStep, NextStep, order);

            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Обзор файлов
        //--------------------------------------------------------------------------------
        private readonly ICommand _BrowseCommand = null;
        public ICommand BrowseCommand => _BrowseCommand ?? new LambdaCommand(OnBrowseCommandExecuted, CanBrowseCommand);
        private bool CanBrowseCommand(object p) => IsWorkUser && !IsAllSteps;
        private void OnBrowseCommandExecuted(object p)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();

            if (dlgOpen.ShowDialog() == true)
            {
                RouteAdding ra = new RouteAdding();
                ra.ad_text = Path.GetFileName(dlgOpen.FileName);

                FileStream fs = new FileStream(dlgOpen.FileName, FileMode.Open);

                ra.ad_file = new byte[fs.Length];
                fs.Read(ra.ad_file, 0, (int)fs.Length);
                fs.Close();

                ListFiles.Add(ra);
                //CurrentStep.RouteAddings.Add(ra);
                //CurrentStep.OnPropertyChanged(nameof(CurrentStep.RouteAddings));
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Закрыть заказ
        //--------------------------------------------------------------------------------
        private readonly ICommand _CloseOrderCommand = null;
        public ICommand CloseOrderCommand => _CloseOrderCommand ?? new LambdaCommand(OnCloseOrderCommandExecuted, CanCloseOrderCommand);
        private bool CanCloseOrderCommand(object p) => IsAllSteps && IsCreateUser;
        private void OnCloseOrderCommandExecuted(object p)
        {
            order.o_statusId = (int)EnumStatus.Closed;
            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Отказать
        //--------------------------------------------------------------------------------
        private readonly ICommand _RefuseCommand = null;
        public ICommand RefuseCommand => _RefuseCommand ?? new LambdaCommand(OnRefuseCommandExecuted, CanRefuseCommand);
        private bool CanRefuseCommand(object p) => IsWorkUser && !IsAllSteps;
        private void OnRefuseCommandExecuted(object p)
        {
            order.o_statusId = (int)EnumStatus.Refused;
            CurrentStep.ro_statusId = (int)EnumStatus.Refused;
            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Открыть файл
        //--------------------------------------------------------------------------------
        private readonly ICommand _OpenFileCommand = null;
        public ICommand OpenFileCommand => _OpenFileCommand ?? new LambdaCommand(OnOpenFileCommandExecuted, CanOpenFileCommand);
        private bool CanOpenFileCommand(object p) => true;
        private void OnOpenFileCommandExecuted(object p)
        {
            if( p is RouteAdding ra)
            {
                string TempFileName = Path.GetTempPath() + ra.ad_text;

                FileStream fs = new FileStream(TempFileName, FileMode.Create);

                fs.Write(ra.ad_file, 0, (int)ra.ad_file.Length);

                fs.Close();

                Process.Start(TempFileName);


            }
        }

        //--------------------------------------------------------------------------------
        // Команда Выйти
        //--------------------------------------------------------------------------------
        private readonly ICommand _CancelCommand = null;
        public ICommand CancelCommand => _CancelCommand ?? new LambdaCommand(OnCancelCommandExecuted, CanCancelCommand);
        private bool CanCancelCommand(object p) => true;
        private void OnCancelCommandExecuted(object p)
        {
            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().Close();
        }



        #endregion



        //--------------------------------------------------------------------------------
        // переустановки статусов при отправке далее
        //--------------------------------------------------------------------------------
        public static void SetStatusStep(RouteOrder step, RouteOrder nextStep, Order order)
        {
            int selectStatus = 0;

            switch((EnumTypesStep)step.ro_typeId)
            {
                case EnumTypesStep.Coordinate:
                    //step.ro_statusId = (int)EnumStatus.Coordinated;
                    selectStatus = (int)EnumStatus.Coordinated;
                    break;
                
                case EnumTypesStep.Approve:
                    selectStatus = (int)EnumStatus.Approved;
                    break;

                case EnumTypesStep.Review:
                    selectStatus = (int)EnumStatus.Coordinated;
                    break;

                case EnumTypesStep.Notify:
                    selectStatus = (int)EnumStatus.Coordinated;
                    break;

                case EnumTypesStep.Created:
                    selectStatus = (int)EnumStatus.Created;
                    break;

            }

            step.ro_statusId = selectStatus;
            order.o_statusId = selectStatus;
            //step.RouteStatus = MainWindowViewModel.repo.RouteStatus.FirstOrDefault(it => it.id == selectStatus);
            //order.RouteStatus = step.RouteStatus;

            if (nextStep != null)
            {
                switch ((EnumTypesStep)nextStep.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Approve:
                        selectStatus = (int)EnumStatus.ApprovWork;
                        break;

                    case EnumTypesStep.Review:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Notify:
                        selectStatus = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Created:
                        selectStatus = (int)EnumStatus.Created;
                        break;

                }

                nextStep.ro_statusId = selectStatus;
                order.o_statusId = selectStatus;
                //nextStep.RouteStatus = MainWindowViewModel.repo.RouteStatus.FirstOrDefault(it => it.id == selectStatus);
                //order.RouteStatus = nextStep.RouteStatus;
            }


            //order.o_statusId = nextStep.ro_statusId;

        }
    }
}
