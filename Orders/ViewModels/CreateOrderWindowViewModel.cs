using Microsoft.Win32;
using Orders.Common;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class CreateOrderWindowViewModel : ViewModel
    {
        private readonly RepositoryBase repo = MainWindowViewModel.repo;
        public List<Route> ListRoute { get; set; }
        public Order Order { get; set; }
        public RouteOrder CreateStep { get; set; }
        public ObservableCollection<RouteAdding> ListFiles { get; set; } = new ObservableCollection<RouteAdding>();


        // поле Сумма и ее видимость
        //public decimal Summa { get; set; }

        private double _Height = 24;
        public double Height { get => _Height; set => Set(ref _Height, value); }

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



        //--------------------------------------------------------------------------------
        // конструктор
        //--------------------------------------------------------------------------------
        public CreateOrderWindowViewModel()
        {
            ListRoute = repo.Routes.ToList();
            Order = new Order();
            Order.RouteOrders = new List<RouteOrder>();
            Order.o_date_created = DateTime.Now;
            Order.o_stepRoute = 0;
            Order.o_ownerUserId = App.CurrentUser.id;

            CreateStep = new RouteOrder
            {
                ro_step = 0,
                RouteAddings = ListFiles,
                ro_userId = App.CurrentUser.id,
                ro_typeId = EnumTypesStep.Created,
                //ro_enabled = true,
                ro_statusId = EnumStatus.Created,
                ro_check = EnumCheckedStatus.CheckedProcess,
            ro_ownerId = App.CurrentUser.id
            };

            //Order.o_name = "Калькуляция КЗ ";
            Order.o_number = MainWindowViewModel.repo.GetNumberOrder().ToString("000");

            //MainWindowViewModel.repo.ResetNumberOrder(100);
            //Order.o_number = "001";

            Order.RouteOrders.Add(CreateStep);

            Height = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.EditSumma) ? 24 : 0;
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
            int curStep = 1;

            foreach (var step in ListRouteStep)
            {
                if (step.r_enabled == true)
                {
                    RouteOrder ro = new RouteOrder();
                    ro.ro_step = curStep++;
                    ro.ro_userId = step.r_userId;
                    ro.ro_typeId = step.r_type;
                    //ro.ro_enabled = true;
                    ro.ro_statusId = EnumStatus.None;
                    ro.ro_ownerId = App.CurrentUser.id;
                    Order.RouteOrders.Add(ro);
                }
            }

            // Добавление себя в конце маршрута
            RouteOrder routeOrder = new RouteOrder();
            routeOrder.ro_step = curStep++;
            routeOrder.ro_userId = App.CurrentUser.id;
            routeOrder.ro_typeId = EnumTypesStep.Notify;
            routeOrder.ro_statusId = EnumStatus.None;
            routeOrder.ro_ownerId = App.CurrentUser.id;
            Order.RouteOrders.Add(routeOrder);

            Order.o_statusId = EnumStatus.Created;

            repo.Add(Order, true);

            //TODO Добавить прикрепленные файлы
            RepositoryFiles repoFiles = new RepositoryFiles();
            repoFiles.AddFiles(CreateStep);

            CreateStep.RouteAddings = ListFiles;

            App.Current.Windows.OfType<CreateOrderWindow>().FirstOrDefault().DialogResult = true;

        }

        //--------------------------------------------------------------------------------
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => true;
        private void OnSendCommandExecuted(object p)
        {
            OnCreateCommandExecuted(p);

            MoveOrder move = new MoveOrder(Order, Order.RouteOrders, EnumAction.Send, CreateStep);
            move.MoveToNextStep(ListFiles);

            //ShareFunction.SendToNextStep(Order, CreateStep, ListFiles);
        }

        //--------------------------------------------------------------------------------
        // Команда Отменить
        //--------------------------------------------------------------------------------
        private readonly ICommand _CancelCommand = null;
        public ICommand CancelCommand => _CancelCommand ?? new LambdaCommand(OnCancelCommandExecuted, CanCancelCommand);
        private bool CanCancelCommand(object p) => true;
        private void OnCancelCommandExecuted(object p)
        {
            App.Current.Windows.OfType<CreateOrderWindow>().FirstOrDefault().Close();
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
        // Команда Добавить файл
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddFileCommand = null;
        public ICommand AddFileCommand => _AddFileCommand ?? new LambdaCommand(OnAddFileCommandExecuted, CanAddFileCommand);
        private bool CanAddFileCommand(object p) => true;
        private void OnAddFileCommandExecuted(object p)
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
        private bool CanDeleteFileCommand(object p) => true;
        private void OnDeleteFileCommandExecuted(object p)
        {
            RouteAdding FileName = p as RouteAdding;

            ListFiles.Remove(FileName);

        }


        #endregion

    }
}
