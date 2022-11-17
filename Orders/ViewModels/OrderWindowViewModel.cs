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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace Orders.ViewModels
{
    internal class OrderWindowViewModel : ViewModel
    {
        public ObservableCollection<RouteOrder> ListRouteOrders { get; set; }

        public User CurrentUser => App.CurrentUser;
        public ObservableCollection<RouteAdding> ListFiles { get; set; }// = new ObservableCollection<RouteAdding>();

        public double HeightSumma { get; set; } = 22;

        #region Права доступа ---------------------------
        private bool _IsEditOtherRoute;     // редактирование чужих маршрутов
        private bool _IsCloseOtherOrder;    // закрытие чужих заказов
        private bool _IsVisibleSumma;       // видимость суммы заказа
        private bool _IsContinueOrder;      // продолжение законченного заказа
        private bool _IsDeleteCurrentStep;  // удаление этапа на рассмотрении
        public bool IsNotEditSumma { get; set; }          
        #endregion



        public RouteOrder SelectedRouteStep { get; set; }

        private bool IsAllSteps =>  order?.RouteOrders.All(it => it.ro_check == EnumCheckedStatus.Checked) ?? false;
        private bool IsWorkUser => CurrentStep?.ro_userId == App.CurrentUser.id;
        private bool IsCreateUser => order?.RouteOrders.Count > 0 && order?.Owner.id == App.CurrentUser.id;
        //&& order?.RouteOrders.FirstOrDefault().ro_userId == App.CurrentUser.id;
        public Order order { get; set; }
        
        private RouteOrder _CurrentStep;
        public RouteOrder CurrentStep { get => _CurrentStep; set { Set(ref _CurrentStep, value); } }
        
        public bool IsDisabledElement => CurrentStep?.ro_userId != App.CurrentUser.id || IsAllSteps 
            || order.o_statusId == EnumStatus.Refused;

        public OrderWindowViewModel() { }

        public OrderWindowViewModel(Order ord)
        {
            order = ord;

            _IsEditOtherRoute = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.EditOtherRoutes);
            _IsCloseOtherOrder = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.CloseOtherOrders);
            _IsVisibleSumma = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.VisibleSumma);
            IsNotEditSumma = !App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.EditSumma);
            _IsContinueOrder = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.ContinueOrder);
            _IsDeleteCurrentStep = App.CurrentUser.RolesUser.Any(it => it.ru_role_id == EnumRoles.DeleteCurrentStep);

            HeightSumma = _IsVisibleSumma || !IsNotEditSumma ? 22 : 0;

            //RouteSteps route = new RouteSteps(order.RouteOrders);
            ListRouteOrders = new ObservableCollection<RouteOrder>(MainWindowViewModel.repo.GetRouteOrders(order.id));
            //ListRouteOrders = new ObservableCollection<RouteOrder>( order.RouteOrders);

            // получение текущего этапа
            //CurrentStep = ListRouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
            CurrentStep = MainWindowViewModel.repo.RouteOrders.FirstOrDefault(it => it.ro_check == EnumCheckedStatus.CheckedProcess
                && it.ro_orderId == order.id
                /*&& it.ro_userId == App.CurrentUser.id*/);

            //CurrentStep = order.RouteOrders.FirstOrDefault(it => it.ro_step == order.o_stepRoute);
            if(CurrentStep != null)
                ListFiles = new ObservableCollection<RouteAdding>(CurrentStep.RouteAddings);
            else
                ListFiles = new ObservableCollection<RouteAdding>();
        }

        #region Команды

        //--------------------------------------------------------------------------------
        // Команда Обновить
        //--------------------------------------------------------------------------------
        private readonly ICommand _RefreshCommand = null;
        public ICommand RefreshCommand => _RefreshCommand ?? new LambdaCommand(OnRefreshCommandExecuted, CanRefreshCommand);
        private bool CanRefreshCommand(object p) => true;
        private void OnRefreshCommandExecuted(object p)
        {
            //string text = CurrentStep.ro_text;

            //MainWindowViewModel.repo.Update<RouteOrder>(CurrentStep);

            MainWindowViewModel.repo.Refresh();

            if(CurrentStep != null)
                MainWindowViewModel.repo.Update<RouteOrder>(CurrentStep);

            ListRouteOrders = new ObservableCollection<RouteOrder>(MainWindowViewModel.repo.GetRouteOrders(order.id));

            //MainWindowViewModel.repo.Update<RouteOrder>(CurrentStep);

            if (CurrentStep != null)
            {
                RouteOrder ro = ListRouteOrders.Single(it => it.id == CurrentStep.id);
                int index = ListRouteOrders.IndexOf(ro);
                ro = CurrentStep;
                ListRouteOrders[index] = CurrentStep;
            }

            //if (CurrentStep is null)
            //{
            //    CurrentStep = MainWindowViewModel.repo.RouteOrders.FirstOrDefault(it => it.ro_check == EnumCheckedStatus.CheckedProcess
            //        && it.ro_orderId == order.id
            //        && it.ro_userId == App.CurrentUser.id);
            //}
            //CurrentStep.ro_text = text;

            //if (CurrentStep != null)
            //    ListFiles = new ObservableCollection<RouteAdding>(CurrentStep.RouteAddings);
            //else
            //    ListFiles = new ObservableCollection<RouteAdding>();

            OnPropertyChanged(nameof(IsDisabledElement));
            OnPropertyChanged(nameof(ListFiles));
            OnPropertyChanged(nameof(ListRouteOrders));
            CommandManager.InvalidateRequerySuggested();

        }

        //--------------------------------------------------------------------------------
        // Команда Вернуть
        //--------------------------------------------------------------------------------
        private readonly ICommand _ReturnCommand = null;
        public ICommand ReturnCommand => _ReturnCommand ?? new LambdaCommand(OnReturnCommandExecuted, CanReturnCommand);
        private bool CanReturnCommand(object p) => IsWorkUser && !IsAllSteps 
            && SelectedRouteStep?.ro_check == EnumCheckedStatus.Checked
            && _CurrentStep?.ro_parentId == null
            && MainWindowViewModel.repo.RouteOrders.Count(it => it.ro_step == _CurrentStep.ro_step
                    && it.ro_orderId == _CurrentStep.ro_orderId
                    && it.ro_parentId == _CurrentStep.ro_parentId
                    && it.ro_check == EnumCheckedStatus.CheckedProcess) == 1
            && _CurrentStep.ro_step != SelectedRouteStep.ro_step
            && SelectedRouteStep?.ro_parentId == null
            && order.o_statusId != EnumStatus.Refused;
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

                MoveOrder move = new MoveOrder(order, ListRouteOrders, EnumAction.Return, CurrentStep, SelectedRouteStep);
                move.MoveToNextStep(ListFiles);

                App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
            }
            
        }


        //--------------------------------------------------------------------------------
        // Команда Добавить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _AddRouteCommand = null;
        public ICommand AddRouteCommand => _AddRouteCommand ?? new LambdaCommand(OnAddRouteCommandExecuted, CanAddRouteCommand);
        private bool CanAddRouteCommand(object p) => 
            ( CurrentStep?.ro_userId == App.CurrentUser.id || _IsEditOtherRoute || IsCreateUser)
            && (!IsAllSteps || _IsContinueOrder)
            && order.o_statusId != EnumStatus.Refused
            && _CurrentStep?.ro_parentId == null;

        private void OnAddRouteCommandExecuted(object p)
        {
            RouteOrder _CurStep;

            if(CurrentStep is null)
            {
                if (MessageBox.Show("Заказ был завершен. После добавления этапа, на него произойдет отправка. Продолжить?","Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;
                _CurStep = ListRouteOrders.LastOrDefault();
            }
            else
                _CurStep = CurrentStep;


            AddRouteWindowViewModel view = new AddRouteWindowViewModel(ListRouteOrders, _CurStep);
            AddRouteWindow winAddRoute = new AddRouteWindow();
            winAddRoute.DataContext = view;

            bool IsSendToStep = IsAllSteps;

            if (winAddRoute.ShowDialog() == true)
            {
                if(IsSendToStep)
                {
                    // были пройдены все этапы
                    MoveOrder move = new MoveOrder(order, ListRouteOrders, EnumAction.Send, _CurStep);
                    move.SendToStep(ListRouteOrders.LastOrDefault());
                    App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
                }

                //order.RouteOrders = ListRouteOrders;

                //ListRouteOrders = new ObservableCollection<RouteOrder>(order.RouteOrders);

                //MainWindowViewModel.repo.Update<RouteOrder>(ListRouteOrders);
                //MainWindowViewModel.repo.Add<RouteOrder>(ro);

                //CurrentOrder.RouteOrders = TempList;
                //MainWindowViewModel.repo.Update(order, true);

                //ListRouteOrders = new ObservableCollection<RouteOrder>(MainWindowViewModel.repo.GetRouteOrders(order.id));

                //OnPropertyChanged(nameof(ListRouteOrders));
                //RepositoryBase repo = new RepositoryBase();
                //repo.Save();
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Удалить этап
        //--------------------------------------------------------------------------------
        private readonly ICommand _DeleteRouteCommand = null;
        public ICommand DeleteRouteCommand => _DeleteRouteCommand ?? new LambdaCommand(OnDeleteRouteCommandExecuted, CanDeleteRouteCommand);
        private bool CanDeleteRouteCommand(object p) => 
            ( SelectedRouteStep?.ro_ownerId == App.CurrentUser.id || _IsEditOtherRoute || IsCreateUser)
            && (SelectedRouteStep?.ro_statusId != EnumStatus.Waiting )
            && (SelectedRouteStep?.ro_check == EnumCheckedStatus.CheckedNone 
                || (_IsDeleteCurrentStep && SelectedRouteStep?.ro_check == EnumCheckedStatus.CheckedProcess));

        private void OnDeleteRouteCommandExecuted(object p)
        {
            if (MessageBox.Show($"Удалить этап № {SelectedRouteStep.NameStep} \"{SelectedRouteStep.User.u_name}\"",
                "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ObservableCollection<RouteOrder> tmpList;
                int index;
                bool IsChild = false;

                if(SelectedRouteStep.ro_check == EnumCheckedStatus.CheckedProcess)
                {
                    if (MessageBox.Show("После удаления будет отправка следующий этап. Продолжить?", "Предупреждение", 
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        return;

                    MoveOrder move = new MoveOrder(order, ListRouteOrders, EnumAction.Send, SelectedRouteStep);
                    move.MoveToNextStep(new List<RouteAdding>());
                    App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
                }

                // это подчиненный маршрут
                if (SelectedRouteStep.ro_parentId != null)
                {
                    index = SelectedRouteStep.ParentRouteOrder.ChildRoutes.IndexOf(SelectedRouteStep);
                    tmpList = SelectedRouteStep.ParentRouteOrder.ChildRoutes;
                    IsChild = true;
                }
                // Это основной маршрут
                else
                {
                    index = ListRouteOrders.IndexOf(SelectedRouteStep);
                    tmpList = ListRouteOrders;
                }

                // проверка на параллельные этапы
                if (tmpList.Count(it => it.ro_step == SelectedRouteStep.ro_step) == 1)
                {
                    // перенумерация следующих после выбранного этапа
                    for (int i = index + 1; i < tmpList.Count; i++)
                    {
                        tmpList[i].ro_step--;
                        var item = tmpList[i];
                        item.OnPropertyChanged(nameof(item.NameStep));
                    }
                }

                // удаление дочерних маршрутов
                List<RouteOrder> ListChild = SelectedRouteStep.ChildRoutes.ToList();
                foreach (var child in ListChild)
                    MainWindowViewModel.repo.Delete<RouteOrder>(child);


                if (MainWindowViewModel.repo.Delete<RouteOrder>(SelectedRouteStep))
                {
                    MainWindowViewModel.repo.Save();
                    if(!IsChild)
                        ListRouteOrders.RemoveAt(index);
                }
            }
        }

        //--------------------------------------------------------------------------------
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => !IsDisabledElement; 
                        // IsWorkUser && order.o_statusId != (int)EnumStatus.Refused  && !IsAllSteps;
        private void OnSendCommandExecuted(object p)
        {

            MoveOrder move = new MoveOrder(order, ListRouteOrders, EnumAction.Send, CurrentStep);
            move.MoveToNextStep(ListFiles);
            //MainWindowViewModel.repo.Update<RouteOrder>(CurrentStep);

            //ShareFunction.SendToNextStep(order, CurrentStep, ListFiles);
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
        private bool CanBrowseCommand(object p) => !IsDisabledElement; // IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
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
        private bool CanDeleteFileCommand(object p) => !IsDisabledElement; // order.o_statusId != (int)EnumStatus.Refused;
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
        private bool CanCloseOrderCommand(object p) =>  (IsCreateUser || _IsCloseOtherOrder) && (IsAllSteps || order?.o_statusId == EnumStatus.Refused);
        private void OnCloseOrderCommandExecuted(object p)
        {
            order.o_statusId = EnumStatus.Closed;
            App.Current.Windows.OfType<OrderWindow>().FirstOrDefault().DialogResult = true;
        }

        //--------------------------------------------------------------------------------
        // Команда Отказать
        //--------------------------------------------------------------------------------
        private readonly ICommand _RefuseCommand = null;
        public ICommand RefuseCommand => _RefuseCommand ?? new LambdaCommand(OnRefuseCommandExecuted, CanRefuseCommand);
        private bool CanRefuseCommand(object p) => !IsDisabledElement; // IsWorkUser && !IsAllSteps && order.o_statusId != (int)EnumStatus.Refused;
        private void OnRefuseCommandExecuted(object p)
        {
            if (MessageBox.Show("Подтверждаете отказ?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (string.IsNullOrEmpty(CurrentStep.ro_text))
                {
                    MessageBox.Show("В сообщении нужно указать причину.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                order.o_statusId = EnumStatus.Refused;
                CurrentStep.ro_date_check = DateTime.Now;
                CurrentStep.ro_statusId = EnumStatus.Refused;
                CurrentStep.ro_check = EnumCheckedStatus.CheckedNone;

                // убрать все статусы рассмотрения
                foreach (var item in order.RouteOrders)
                {
                    if (item.ro_statusId == EnumStatus.Waiting
                        || item.ro_statusId == EnumStatus.ApprovWork
                        || item.ro_statusId == EnumStatus.CoordinateWork)
                    {
                        item.ro_statusId = EnumStatus.None;
                        item.ro_check = EnumCheckedStatus.CheckedNone;
                    }
                }
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
