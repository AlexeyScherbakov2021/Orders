using Microsoft.Win32;
using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
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

        private Order _order;
        public Order order 
        { 
            get => _order; 
            set 
            { 
                if(Set(ref _order, value))
                {
                    CurrentStep = value.RouteOrders.FirstOrDefault(it => it.ro_step == value.o_stepRoute);
                    ListFiles = new ObservableCollection<RouteAdding>( CurrentStep.RouteAddings);
                    OnPropertyChanged(nameof(ListFiles));
                }
            } 
        }
        
        private RouteOrder _CurrentStep;
        public RouteOrder CurrentStep { get => _CurrentStep; set { Set(ref _CurrentStep, value); } }


        public OrderWindowViewModel()
        {
        }

        public ObservableCollection<RouteAdding> ListFiles { get; set; } // = new ObservableCollection<RouteAdding>();


        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => true;
        private void OnAddRouteCommandExecuted(object p)
        {
            AddRouteWindow winAddRoute = new AddRouteWindow();
            AddRouteWindowViewModel view = new AddRouteWindowViewModel(order);
            winAddRoute.DataContext = view;
            if (winAddRoute.ShowDialog() == true)
            {

            }
        }

        //--------------------------------------------------------------------------------
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => true;
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
                //order.o_statusId = (int)EnumStatus.Approved;
            }
            else
            {
                order.o_stepRoute++;
                NextStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
                //order.o_statusId = (int)EnumStatus.Coordinated;
            }
            SetStatusStep(CurrentStep, NextStep);

            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Обзор файлов
        //--------------------------------------------------------------------------------
        private readonly ICommand _BrowseCommand = null;
        public ICommand BrowseCommand => _BrowseCommand ?? new LambdaCommand(OnBrowseCommandExecuted, CanBrowseCommand);
        private bool CanBrowseCommand(object p) => true;
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
        private bool CanCloseOrderCommand(object p) => true;
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
        private bool CanRefuseCommand(object p) => true;
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


        #endregion



        //--------------------------------------------------------------------------------
        // переустановки статусов при отправке далее
        //--------------------------------------------------------------------------------
        private void SetStatusStep(RouteOrder step, RouteOrder nextStep)
        {
           
            switch((EnumTypesStep)step.ro_typeId)
            {
                case EnumTypesStep.Coordinate:
                    step.ro_statusId = (int)EnumStatus.Coordinated;
                    break;
                
                case EnumTypesStep.Approve:
                    step.ro_statusId = (int)EnumStatus.Approved;
                    break;

                case EnumTypesStep.Review:
                    step.ro_statusId = (int)EnumStatus.Coordinated;
                    break;

                case EnumTypesStep.Notify:
                    step.ro_statusId = (int)EnumStatus.Coordinated;
                    break;

            }

            order.o_statusId = step.ro_statusId;

            if (nextStep != null)
            {
                switch ((EnumTypesStep)nextStep.ro_typeId)
                {
                    case EnumTypesStep.Coordinate:
                        nextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Approve:
                        nextStep.ro_statusId = (int)EnumStatus.ApprovWork;
                        break;

                    case EnumTypesStep.Review:
                        nextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;

                    case EnumTypesStep.Notify:
                        nextStep.ro_statusId = (int)EnumStatus.CoordinateWork;
                        break;
                }

                order.o_statusId = nextStep.ro_statusId;
            }


            //order.o_statusId = nextStep.ro_statusId;

        }
    }
}
