﻿using Microsoft.Win32;
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
        public ObservableCollection<RouteAdding> ListFiles { get; set; }// = new ObservableCollection<RouteAdding>();

        public RouteOrder SelectedRouteStep { get; set; }

        private bool IsAllSteps =>  order?.RouteOrders.All(it => it.ro_check == EnumCheckedStatus.Checked) ?? false;
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

            MainWindowViewModel.repo.LoadAddFiles(order.RouteOrders);

            CurrentStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
            if(CurrentStep != null)
                ListFiles = new ObservableCollection<RouteAdding>(CurrentStep.RouteAddings);
            else
                ListFiles = new ObservableCollection<RouteAdding>();

        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Вернуть
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReturnCommand = null;
        public ICommand ReturnCommand => _ReturnCommand ?? new LambdaCommand(OnReturnCommandExecuted, CanReturnCommand);
        private bool CanReturnCommand(object p) => IsWorkUser && !IsAllSteps 
            && SelectedRouteStep?.ro_check == EnumCheckedStatus.Checked
            && SelectedRouteStep?.ro_return_step == null
            && order.o_statusId != (int)EnumStatus.Refused;
        private void OnReturnCommandExecuted(object p)
        {
            if(MessageBox.Show($"Вернуть заказ на этап № {SelectedRouteStep.ro_step} \"{SelectedRouteStep.User.u_name}\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes )
            {
                if (string.IsNullOrEmpty(CurrentStep.ro_text) && MessageBox.Show("Вы не добавили сообщение. Продолжить возврат? ",
                        "Предупреждение", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }


               foreach (var item in order.RouteOrders)
                {
                   // у промежуточных этапов удаляем статус
                   if (item.ro_step >= SelectedRouteStep.ro_step && item.ro_step < order.o_stepRoute)
                   {
                       item.ro_check = EnumCheckedStatus.CheckedNone;
                       item.ro_statusId = (int)EnumStatus.None;
                       item.ro_date_check = null;
                   }
                }

                CurrentStep.ro_statusId = (int)EnumStatus.Return;
                CurrentStep.ro_date_check = DateTime.Now;
                CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;

                SelectedRouteStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                SelectedRouteStep.ro_check = EnumCheckedStatus.CheckedProcess;
                order.o_stepRoute = SelectedRouteStep.ro_step;
                order.o_statusId = (int)EnumStatus.Return;

                ShareFunction.SendMail(SelectedRouteStep.User.u_email, order.o_number);
                App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
            }

            

            //ReturnOrderWindowViewModel view = new ReturnOrderWindowViewModel(order);
            //ReturnOrderWindow winReturnRoute = new ReturnOrderWindow();
            //winReturnRoute.DataContext = view;
            //view.ReturnMessage = CurrentStep.ro_text;
            //if (winReturnRoute.ShowDialog() == true)
            //{
            //    CurrentStep.ro_text = view.ReturnMessage;
            //    foreach (var item in order.RouteOrders)
            //    {
            //        // у промежуточных этапов удаляем статус
            //        if (item.ro_step >= view.SelectedRouteOrder.ro_step && item.ro_step < order.o_stepRoute)
            //        {
            //            item.ro_check = EnumCheckedStatus.CheckedNone;
            //            item.ro_statusId = (int)EnumStatus.None;
            //            item.ro_date_check = null;
            //        }
            //    }

            //    CurrentStep.ro_statusId = (int)EnumStatus.Return;
            //    CurrentStep.ro_date_check = DateTime.Now;
            //    CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;

            //    view.SelectedRouteOrder.ro_statusId = (int)EnumStatus.CoordinateWork;
            //    view.SelectedRouteOrder.ro_check = EnumCheckedStatus.CheckedProcess;
            //    order.o_stepRoute = view.SelectedRouteOrder.ro_step;
            //    order.o_statusId = (int)EnumStatus.Return;

            //    //MainWindowViewModel.repo.Refresh<RouteOrder>(order.RouteOrders);

            //    ShareFunction.SendMail(view.SelectedRouteOrder?.User.u_email, order.o_number);


            //    //MainWindowViewModel.repo.Save();
            //    //OnPropertyChanged(nameof(order));
            //    App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
            //}
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
        // Команда Удалить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteRouteCommand = null;
        public ICommand DeleteRouteCommand => _DeleteRouteCommand ?? new LambdaCommand(OnDeleteRouteCommandExecuted, CanDeleteRouteCommand);
        private bool CanDeleteRouteCommand(object p) => SelectedRouteStep?.ro_ownerId == App.CurrentUser.id &&
            SelectedRouteStep?.ro_check == EnumCheckedStatus.CheckedNone;

        private void OnDeleteRouteCommandExecuted(object p)
        {
            if (MessageBox.Show($"Удалить этап № {SelectedRouteStep.ro_step} \"{SelectedRouteStep.User.u_name}\"",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                // перенумерауия этапов
                List<RouteOrder> TempList = new List<RouteOrder>();

                foreach (var item in order.RouteOrders)
                {
                    if (item.ro_step == SelectedRouteStep.ro_step)
                        continue;

                    if (item.ro_step >= SelectedRouteStep.ro_step)
                        item.ro_step--;

                    TempList.Add(item);

                }
                MainWindowViewModel.repo.Delete<RouteOrder>(SelectedRouteStep);
                order.RouteOrders = TempList;
                MainWindowViewModel.repo.Update(order, true);

                //order.RouteOrders.Remove(SelectedRouteStep);
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

            // TODO Добавить прикрепленные файлы

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
            RouteAdding FileName = p as RouteAdding;

            MainWindowViewModel.repo.Delete<RouteAdding>(FileName);
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
            if(MessageBox.Show("Подтверждаете отказ?","Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if(string.IsNullOrEmpty(CurrentStep.ro_text))
                {
                    MessageBox.Show("В сообщении нужно указать причину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                order.o_statusId = (int)EnumStatus.Refused;
                CurrentStep.ro_date_check = DateTime.Now;
                CurrentStep.ro_statusId = (int)EnumStatus.Refused;
                App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
            }
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

                RepositoryFiles repoFiles = new RepositoryFiles();

                string TempFileName = repoFiles.GetFile(ra);

                //string TempFileName = Path.GetTempPath() + ra.ad_text;
                //FileStream fs = new FileStream(TempFileName, FileMode.Create);
                //fs.Write(ra.ad_file, 0, (int)ra.ad_file.Length);
                //fs.Close();
                if(TempFileName != null)
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


    }
}
