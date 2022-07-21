using Microsoft.Win32;
using Orders.Infrastructure;
using Orders.Infrastructure.Commands;
using Orders.Models;
using Orders.ViewModels.Base;
using Orders.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Orders.ViewModels
{
    internal class OrderWindowViewModel : ViewModel
    {
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
        // Команда Отправить
        //--------------------------------------------------------------------------------
        private readonly ICommand _SendCommand = null;
        public ICommand SendCommand => _SendCommand ?? new LambdaCommand(OnSendCommandExecuted, CanSendCommand);
        private bool CanSendCommand(object p) => true;
        private void OnSendCommandExecuted(object p)
        {

            CurrentStep.RouteAddings = ListFiles;
            CurrentStep.ro_check = 1;
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
                RouteAdding ra = new RouteAdding
                {
                    ad_text = Path.GetFileName(dlgOpen.FileName),
                };

                ListFiles.Add(ra);
                //CurrentStep.RouteAddings.Add(ra);
                //CurrentStep.OnPropertyChanged(nameof(CurrentStep.RouteAddings));
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
