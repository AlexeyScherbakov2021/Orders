using Microsoft.Win32;
using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Infrastructure.Common;
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
using System.Windows;
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
        // Команда Вернуть
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReturnCommand = null;
        public ICommand ReturnCommand => _ReturnCommand ?? new LambdaCommand(OnReturnCommandExecuted, CanReturnCommand);
        private bool CanReturnCommand(object p) => IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
        private void OnReturnCommandExecuted(object p)
        {
            ReturnOrderWindowViewModel view = new ReturnOrderWindowViewModel(order);
            ReturnOrderWindow winReturnRoute = new ReturnOrderWindow();
            winReturnRoute.DataContext = view;
            if (winReturnRoute.ShowDialog() == true)
            {
                foreach(var item in order.RouteOrders)
                {
                    // у промежуточных этапов удаляем статус
                    if (item.ro_step >= view.SelectedRouteOrder.ro_step && item.ro_step < order.o_stepRoute)
                    {
                        item.ro_check = 0;
                        item.ro_statusId = (int)EnumStatus.None;
                        item.ro_date_check = null;
                    }
                }

                view.SelectedRouteOrder.ro_statusId = (int)EnumStatus.CoordinateWork;
                CurrentStep.ro_statusId = (int)EnumStatus.Return;
                order.o_stepRoute = view.SelectedRouteOrder.ro_step;
                order.o_statusId = (int)EnumStatus.Return;

                //MainWindowViewModel.repo.Refresh<RouteOrder>(order.RouteOrders);

                //MainWindowViewModel.repo.Save();
                //OnPropertyChanged(nameof(order));
                App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
            }
        }


        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
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
                                        && order.o_statusId != (int)EnumStatus.Refused
                                        && !IsAllSteps;
        private void OnSendCommandExecuted(object p)
        {

            //CurrentStep.RouteAddings = ListFiles;
            //CurrentStep.ro_check = 1;
            //CurrentStep.ro_date_check = DateTime.Now;
            //RouteOrder NextStep;

            //if (order.RouteOrders.All(it => it.ro_check == 1))
            //{
            //    // маршрут окончен
            //    NextStep = null;
            //}
            //else
            //{
            //    //order.o_stepRoute++;
            //    NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute + 1);

            //    if(NextStep.ro_return_step != null && CurrentStep.ro_return_step is null)
            //    {
            //        // следующий этап подчиненный после главного
            //        if(NextStep.ro_check == 1)
            //        {
            //            // он уже был рассмотрен,  делаем прыжок
            //            NextStep = order.RouteOrders
            //                .FirstOrDefault(it => it.ro_step > NextStep.ro_step && it.ro_return_step is null);

            //        }
            //        else
            //            CurrentStep.ro_check = 0;

            //    }
            //    else if(NextStep.ro_return_step is null && CurrentStep.ro_return_step != null)
            //    {
            //        // следующий этап после подчиненного уже основной
            //        // вохвращаемся на главный
            //        NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == CurrentStep.ro_return_step);
            //        //order.o_stepRoute = NextStep.ro_step;
            //    }


            //    //order.o_statusId = (int)EnumStatus.Coordinated;
            //}
            //ShareFunction.SetStatusStep(CurrentStep, NextStep, order);

            ShareFunction.SendToNextStep(order, CurrentStep, ListFiles);


            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;


        }

        //--------------------------------------------------------------------------------
        // Команда Drop
        //--------------------------------------------------------------------------------

        public void ItemsControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                ShareFunction.AddFiles(files, ListFiles);

            }
        }


        //--------------------------------------------------------------------------------
        // Команда Обзор файлов
        //--------------------------------------------------------------------------------
        private readonly ICommand _BrowseCommand = null;
        public ICommand BrowseCommand => _BrowseCommand ?? new LambdaCommand(OnBrowseCommandExecuted, CanBrowseCommand);
        private bool CanBrowseCommand(object p) => IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
        private void OnBrowseCommandExecuted(object p)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.Multiselect = true;

            if (dlgOpen.ShowDialog() == true)
            {
                ShareFunction.AddFiles(dlgOpen.FileNames, ListFiles);
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Удалить файл
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteFileCommand = null;
        public ICommand DeleteFileCommand => _DeleteFileCommand ?? new LambdaCommand(OnDeleteFileCommandExecuted, CanDeleteFileCommand);
        private bool CanDeleteFileCommand(object p) => order.o_statusId != (int)EnumStatus.Refused;
        private void OnDeleteFileCommandExecuted(object p)
        {
            RouteAdding FileName = p as RouteAdding ;

            ListFiles.Remove(FileName);

        }

        //--------------------------------------------------------------------------------
        // Команда Закрыть заказ
        //--------------------------------------------------------------------------------
        private readonly ICommand _CloseOrderCommand = null;
        public ICommand CloseOrderCommand => _CloseOrderCommand ?? new LambdaCommand(OnCloseOrderCommandExecuted, CanCloseOrderCommand);
        private bool CanCloseOrderCommand(object p) =>  IsCreateUser && (IsAllSteps || order.o_statusId == (int)EnumStatus.Refused);
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
        private bool CanRefuseCommand(object p) => IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
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



        ////--------------------------------------------------------------------------------
        //// переустановки статусов при отправке далее
        ////--------------------------------------------------------------------------------
        //public static void SetStatusStep(RouteOrder step, RouteOrder nextStep, Order order)
        //{
        //    int selectStatus = 0;

        //    if (step.ro_check > 0)
        //    {
        //        switch ((EnumTypesStep)step.ro_typeId)
        //        {
        //            case EnumTypesStep.Coordinate:
        //                //step.ro_statusId = (int)EnumStatus.Coordinated;
        //                selectStatus = (int)EnumStatus.Coordinated;
        //                break;

        //            case EnumTypesStep.Approve:
        //                selectStatus = (int)EnumStatus.Approved;
        //                break;

        //            case EnumTypesStep.Review:
        //                selectStatus = (int)EnumStatus.Coordinated;
        //                break;

        //            case EnumTypesStep.Notify:
        //                selectStatus = (int)EnumStatus.Coordinated;
        //                break;

        //            case EnumTypesStep.Created:
        //                selectStatus = (int)EnumStatus.Created;
        //                break;

        //        }

        //        step.ro_statusId = selectStatus;
        //        order.o_statusId = selectStatus;
        //    }
        //    else
        //        step.ro_statusId = (int)EnumStatus.Waiting;

        //    order.o_stepRoute = step.ro_step;

        //    //step.RouteStatus = MainWindowViewModel.repo.RouteStatus.FirstOrDefault(it => it.id == selectStatus);
        //    //order.RouteStatus = step.RouteStatus;

        //    if (nextStep != null)
        //    {
        //        switch ((EnumTypesStep)nextStep.ro_typeId)
        //        {
        //            case EnumTypesStep.Coordinate:
        //                selectStatus = (int)EnumStatus.CoordinateWork;
        //                break;

        //            case EnumTypesStep.Approve:
        //                selectStatus = (int)EnumStatus.ApprovWork;
        //                break;

        //            case EnumTypesStep.Review:
        //                selectStatus = (int)EnumStatus.CoordinateWork;
        //                break;

        //            case EnumTypesStep.Notify:
        //                selectStatus = (int)EnumStatus.CoordinateWork;
        //                break;

        //            case EnumTypesStep.Created:
        //                selectStatus = (int)EnumStatus.Created;
        //                break;

        //        }

        //        nextStep.ro_statusId = selectStatus;
        //        order.o_statusId = selectStatus;
        //        order.o_stepRoute = nextStep.ro_step;
        //        //nextStep.RouteStatus = MainWindowViewModel.repo.RouteStatus.FirstOrDefault(it => it.id == selectStatus);
        //        //order.RouteStatus = nextStep.RouteStatus;
        //    }

        //    //order.o_statusId = nextStep.ro_statusId;

        //}
    }
}
